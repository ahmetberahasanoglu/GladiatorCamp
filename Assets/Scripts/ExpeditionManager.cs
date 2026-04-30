using UnityEngine;
using System.Collections.Generic;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance;

    [Header("Sefer Durumu")]
    public bool isExpeditionActive = false;
    public int currentEncounterCount = 0; // Haritada atılan adım sayısı

    [Header("Geçici Sefer Çantası")]
    public int tempGold = 0;
    public int tempReputation = 0;
    public List<ItemData> tempItems = new List<ItemData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Haritaya ilk çıkıldığında çağrılır
    public void StartExpedition()
    {
        isExpeditionActive = true;
        currentEncounterCount = 0;
        
        // Çantayı sıfırla
        tempGold = 0;
        tempReputation = 0;
        tempItems.Clear();

        Debug.Log("Sefer Başladı! Geçici çanta sıfırlandı.");
        UpdateTopBarUI();
    }

    // Savaş veya olay kazanıldığında ganimetleri çantaya atar
    public void AddLoot(int goldReward, int repReward, List<ItemData> itemRewards = null)
    {
        tempGold += goldReward;
        tempReputation += repReward;

        if (itemRewards != null)
        {
            tempItems.AddRange(itemRewards);
        }

        // Haritada atılan adımı (Encounter) 1 artır ve günü geçir
        currentEncounterCount++;
        if (DayManager.Instance != null) DayManager.Instance.NextDay(1); // Her encounter = 1 Gün

        UpdateTopBarUI();
        
        // 5. Encounter kontrolü (Ata Yadigarı / Meta Progression için)
        if (currentEncounterCount % 5 == 0)
        {
            TriggerRelicChoice();
        }
    }

    // OYUNCU KAMPA DÖNME KARARI ALDIĞINDA ÇALIŞIR
    public void ReturnToCampSafely()
    {
        if (!isExpeditionActive) return;

        // 1. Kazançları asıl depolara aktar
        // (Buradaki ResourceManager, CurrencyManager vs. senin kendi oyunundaki yöneticilerdir)
        // Örnek: ResourceManager.Instance.AddGold(tempGold);
        // Örnek: ReputationManager.Instance.AddReputation(tempReputation);

        if (InventoryStorage.Instance != null)
        {
            foreach (var item in tempItems)
            {
                InventoryStorage.Instance.AddItem(item);
            }
        }

        // 2. Kamp Gazetesini / Özet Ekranını Göster
        ShowExpeditionSummary(true);

        // 3. Seferi bitir ve çantayı boşalt
        ResetExpedition();
    }

    // KERVANSARAY MANTIĞI (%10 Komisyonla güvenli aktarım)
    public void SendLootViaCaravan()
    {
        int fee = Mathf.RoundToInt(tempGold * 0.1f);
        int safeGold = tempGold - fee;

        // Geçici altını ve eşyaları ana kampa gönder
        // ResourceManager.Instance.AddGold(safeGold);
        if (InventoryStorage.Instance != null)
        {
            foreach (var item in tempItems) InventoryStorage.Instance.AddItem(item);
        }

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show($"Kervan yola çıktı! (-{fee} Altın Kesinti)", NotificationType.Success);

        // Gönderilenleri çantadan sil, ama sefer devam ediyor (İtibar hariç, itibar kampa dönünce işlenir)
        tempGold = 0;
        tempItems.Clear();
        UpdateTopBarUI();
    }

    // ASKERLER ÖLÜR VEYA İTİBAR 0 OLURSA ÇALIŞIR (GAME OVER / SÜRGÜN)
    public void FailExpedition()
    {
        if (!isExpeditionActive) return;

        // Çantadaki her şey (Altın, Eşyalar) BOŞA GİTTİ!
        
        // Yenilgi gazetesini göster
        ShowExpeditionSummary(false);

        ResetExpedition();
        
        // Padişahın sürgün fermanı tetiklenebilir...
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

    // UI'daki o gerilimli yazıyı (Örn: Altın 500 (+100)) güncellemek için
    private void UpdateTopBarUI()
    {
        if (TopInfoBarUI.Instance != null)
        {
            // Bu metodu TopInfoBarUI scriptinin içine ekleyeceğiz
            TopInfoBarUI.Instance.UpdateExpeditionRiskUI(tempGold, tempReputation);
        }
    }

    private void TriggerRelicChoice()
    {
        Debug.Log("5 Encounter geçildi! Ata Yadigarı seçme ekranı açılmalı.");
        // MetaProgressionManager.Instance.ShowRelicSelectionUI();
    }

    private void ShowExpeditionSummary(bool isVictory)
    {
        string title = isVictory ? "Ordu Kampa Döndü!" : "Seferde Felaket!";
        string detail = isVictory 
            ? $"Kazanılan Altın: {tempGold}\nKazanılan İtibar: {tempReputation}\nToplanan Eşya: {tempItems.Count}" 
            : "Tüm ganimetler haritada kaybedildi...";
            
        // NewspaperManager.Instance.ShowHeadline(title, detail);
    }
}