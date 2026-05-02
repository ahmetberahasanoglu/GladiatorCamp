using UnityEngine;
using System.Collections.Generic;

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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

    // Savaş, Olay veya Sandık bittiğinde çağrılacak!
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
        
        // 1 Düğüm = 1 Gün. Askerlerin maaşı ödenir, moraller hesaplanır!
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.EndOfDay(1);
        }

        // 5. Encounter meta-progression (Ata Yadigarı) kontrolü
       // 5. Encounter meta-progression (Ata Yadigarı) kontrolü
        if (currentEncounterCount % 5 == 0)
        {
            Debug.Log("<color=yellow>MİRAS ZAMANI!</color> 5 Encounter geçildi.");
            MetaProgressionManager.Instance.TriggerRelicChoice(); 
        }
       
    BuildingClickable[] allBuildings = FindObjectsByType<BuildingClickable>(FindObjectsSortMode.None);
    foreach (var building in allBuildings)
    {
        building.AdvanceConstructionTimer();
    }
    }

    // --- KAMPA DÖNÜŞ (BAŞARILI) ---
    public void ReturnToCampSafely()
    {
        if (!isExpeditionActive) return;

        // 1. Altınları Kasaya Aktar
        if (tempGold > 0) MoneyManager.Instance.Add(tempGold);
        else if (tempGold < 0) MoneyManager.Instance.Spend(Mathf.Abs(tempGold)); 

        // 2. İtibarı İncele ve Aktar (Eğer 0'a düşerse SÜRGÜN YER!)
        if (tempReputation != 0)
        {
            ReputationManager.Instance.ChangeReputation(tempReputation);
        }

        // 3. Eşyaları Depoya Aktar
        if (InventoryStorage.Instance != null)
        {
            foreach (var item in tempItems)
            {
                InventoryStorage.Instance.AddItem(item);
            }
        }

        // 4. Gazeteyi Göster (Sürgün yemediysek)
        if (ReputationManager.Instance.GetReputation() > 0)
        {
            ShowExpeditionSummary(true);
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

    // --- SEFER BAŞARISIZ (ÖLÜM VEYA İFLAS) ---
    public void FailExpedition()
    {
        if (!isExpeditionActive) return;

        // Altın ve Eşyalar HARİTADA KALDI! (Çöpe gitti)
        
        // Ancak Kötü Şöhret (Negatif İtibar) kampa ulaşır!
        if (tempReputation < 0)
        {
            ReputationManager.Instance.ChangeReputation(tempReputation);
        }

        ShowExpeditionSummary(false);
        ResetExpedition();
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
        // Eğer TopInfoBarUI scriptin varsa, oraya bu geçici değerleri gönder.
        // Örnek: TopInfoBarUI.Instance.UpdateRisk(tempGold, tempReputation);
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