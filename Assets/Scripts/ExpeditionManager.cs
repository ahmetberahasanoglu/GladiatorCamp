using UnityEngine;
using System.Collections.Generic;
using System;


public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance;

    [Header("Sefer Durumu")]
    public bool isExpeditionActive = false;
    public int currentEncounterCount = 0; 

    [Header("Geçici Sefer Çantası")]
    public int tempGold = 0;
    public int tempReputation = 0;
    public List<ItemData> tempItems = new List<ItemData>();
    public static event Action OnEncounterAdvanced;
    [Header("UI Referansları")]
    public TMPro.TextMeshProUGUI relicProgressText;

    void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }
    else
    {
        Destroy(gameObject);
    }
}

void Start()
    {
        // Eğer sefer açıksa (isExpeditionActive true ise), kamp yüklendiği an MapPanel'i aç
        if (isExpeditionActive && MapEventManager.Instance != null)
        {
           MapManager.Instance.ShowMap();
        }
    }
    public void StartExpedition()
    {
        isExpeditionActive = true;
        currentEncounterCount = 0;
        
        tempGold = 0;
        tempReputation = 0;
        tempItems.Clear();

        Debug.Log("Sefer Başladı! Geçici çanta sıfırlandı.");
        UpdateTopBarUI();
    }

  
    public void AddLoot(int goldReward, int repReward, List<ItemData> itemRewards = null)
    {
        tempGold += goldReward;
        tempReputation += repReward;

        if (itemRewards != null)
        {
            tempItems.AddRange(itemRewards);
        }

        UpdateTopBarUI();
    }

    // YENİ: Haritada atılan her adımda zamanı geçirir. 
    // MapManager içindeki MoveIconRoutine bittiğinde çağıracağız.
    public void AdvanceEncounter()
    {
        if (!isExpeditionActive) return;

        currentEncounterCount++;
        
        if (DayManager.Instance != null)
        {
            DayManager.Instance.NextDay(1); 
        }

     

       OnEncounterAdvanced?.Invoke();
        UpdateTopBarUI();
    }

  
    public void ReturnToCampSafely()
    {
        if (!isExpeditionActive) return;

        // Özet için mevcut değerleri kaydet
        int summaryGold  = tempGold;
        int summaryRep   = tempReputation;
        int summaryItems = tempItems.Count;
        int summaryDays  = currentEncounterCount;

        // Transferleri yap
        if (tempGold > 0) MoneyManager.Instance.Add(tempGold);
        else if (tempGold < 0) MoneyManager.Instance.Spend(Mathf.Abs(tempGold));

        if (tempReputation != 0) ReputationManager.Instance.ChangeReputation(tempReputation);

        if (InventoryStorage.Instance != null)
            foreach (var item in tempItems) InventoryStorage.Instance.AddItem(item);

        // Miras hakları
        int picksPerMilestone = MetaProgressionManager.Instance != null
            ? MetaProgressionManager.Instance.GetRelicPicksPerMilestone() : 1;
        int earnedRelics = (currentEncounterCount / 5) * picksPerMilestone;
        if (earnedRelics > 0 && MetaProgressionManager.Instance != null)
            MetaProgressionManager.Instance.AddPendingRelics(earnedRelics);

        // Asker listesi & hasar kontrolü
        var soldiers = new System.Collections.Generic.List<Gladiator>();
        bool hadCasualties = false;
        foreach (var g in UnityEngine.Object.FindObjectsByType<Gladiator>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (g.CompareTag("MySoldier"))
            {
                soldiers.Add(g);
                if (g.currentHealth <= 0) hadCasualties = true;
            }
        }

        // Özet ekranını göster (miras ekranı CloseSummary'de açılacak)
        if (ExpeditionSummaryUI.Instance != null)
            ExpeditionSummaryUI.Instance.ShowSummary(
                summaryGold, summaryRep, summaryItems,
                summaryDays, summaryDays, earnedRelics,
                soldiers, hadCasualties);

        ResetExpedition();
    }
    public void FailExpedition()
    {
        if (!isExpeditionActive) return;

        if (tempReputation < 0) ReputationManager.Instance.ChangeReputation(tempReputation);

        // Ölsek bile kazandığımız miras hakları bizimle gelir!
        int earnedRelics = currentEncounterCount / 5;
        if (earnedRelics > 0 && MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.AddPendingRelics(earnedRelics);
        }

        ResetExpedition();
    }

    // --- KERVANSARAY MANTIĞI (%10 Komisyon) ---
    public void SendLootViaCaravan()
    {
        if (tempGold <= 0 && tempItems.Count == 0) return;

        int fee = Mathf.RoundToInt(tempGold * 0.1f);
        int safeGold = tempGold - fee;

        if (safeGold > 0) MoneyManager.Instance.Add(safeGold);

        if (InventoryStorage.Instance != null)
        {
            foreach (var item in tempItems) InventoryStorage.Instance.AddItem(item);
        }

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show($"Kervan yola çıktı! (-{fee} Akçe Kesinti)", NotificationType.Success);

        // Gönderilenler çantadan silinir, sefer devam eder. (İtibar GÖNDERİLEMEZ!)
        tempGold = 0;
        tempItems.Clear();
        UpdateTopBarUI();
    }

   

    private void ResetExpedition()
    {
        isExpeditionActive = false;
        tempGold = 0;
        tempReputation = 0;
        tempItems.Clear();
        currentEncounterCount = 0;
        UpdateTopBarUI();
    }

    private void UpdateTopBarUI()
    {
        // 1/5 Görselini ayarlama
        if (relicProgressText != null)
        {
            int currentStep = currentEncounterCount % 5;
            int earned = currentEncounterCount / 5;
            relicProgressText.text = $"Miras: {currentStep}/5\n<size=70%>(Kazanılan: {earned})</size>";
        }
        if (ExpeditionBagUI.Instance != null)
        {
            ExpeditionBagUI.Instance.UpdateBagUI(tempGold, tempReputation, tempItems.Count);
        }
    }

    private void ShowExpeditionSummary(bool isVictory)
    {
        string title = isVictory ? "Ordu Kampa Döndü!" : "Seferde Felaket!";
        string detail = isVictory 
            ? $"Ganimet: {tempGold} Akçe\nİtibar Değişimi: {tempReputation}\nToplanan Eşya: {tempItems.Count}" 
            : "Tüm ganimetler kaybedildi! Sadece yaralılar döndü...";
            
        // NewspaperManager.Instance.ShowHeadline(title, detail);
    }
}