using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    public int currentDay = 1;
    public int maxDays = 100;
    
    [Header("Olay Sistemi")]
    public List<GameEvent> possibleEvents; 
    [Range(0, 100)] public int eventChance = 10; 

    // --- YENİ EKLENEN SİNEMATİK UI DEĞİŞKENLERİ ---
    [Header("Geçiş Efekti (UI)")]
    public CanvasGroup blackFadeGroup; // Unity'den Siyah CanvasGroup objeni buraya sürükle
    public float fadeDuration = 0.5f;  // Kararma hızı
    private bool isTransitioning = false; // Spam engelleyici kilit
    // ----------------------------------------------

    public event Action OnNewDay; 
    public event Action<int> OnDayChanged;
    public event Action<GameEvent> OnEventTriggered;
    public event System.Action OnWinterArrived;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        OnDayChanged?.Invoke(currentDay);
    }

    // =========================================================
    // 1. AŞAMA: TOPBAR'DAKİ BUTONA BAĞLAYACAĞIN YENİ FONKSİYON
    // =========================================================
    public void UITriggerNextDay()
    {
        // Oyuncu butona 50 kere bassa bile sistem geçiş bitene kadar tepki vermez (Spam Koruması)
        if (isTransitioning) return; 

        // Sadece butonla tetiklendiğinde sinematik geçişi başlat (1 Gün geçer)
        StartCoroutine(NextDayRoutine(1));
    }

    private IEnumerator NextDayRoutine(int amount)
    {
        isTransitioning = true;

        // 1. Ekranı Karart
        if (blackFadeGroup != null)
        {
            blackFadeGroup.blocksRaycasts = true; // Oyuncu ekrana tıklayamasın
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                blackFadeGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
                yield return null;
            }
            blackFadeGroup.alpha = 1;
        }

        // 2. Ekran karanlıkken senin orijinal kodunu çağırıp hesapları kesiyoruz!
        NextDay(amount);

        // Gece-Gündüz geçişini hissettirmek için kısa bir es
        yield return new WaitForSecondsRealtime(0.8f);

        // 3. Ekranı Aydınlat
        if (blackFadeGroup != null)
        {
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                blackFadeGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
                yield return null;
            }
            blackFadeGroup.alpha = 0;
            blackFadeGroup.blocksRaycasts = false; // Tıklama kilidini aç
        }

        // 4. Parşömeni aç ve raporu göster!
        ShowMorningReport();

        isTransitioning = false; // Kilit açıldı
    }

    // =========================================================
    // 2. AŞAMA: SENİN KODUN (Hiçbir harita/savaş sistemini bozmasın diye orijinal bırakıldı)
    // =========================================================
    public void NextDay(int amount)
    {
        currentDay += amount;
        
        OnNewDay?.Invoke();
        OnDayChanged?.Invoke(currentDay);
        HealAllSoldiers(20 * amount);
        
        CheckForRandomEvent();
        
        if (WorkplaceManager.Instance != null) WorkplaceManager.Instance.EndOfDayPayment(amount);
        if (MoneyManager.Instance != null) MoneyManager.Instance.EndOfDay(amount); 
        if (SupplyManager.Instance != null) SupplyManager.Instance.ConsumeDailyFood(amount);

        if (currentDay >= maxDays)
        {
            Debug.Log("KIŞ GELDİ! OYUN BİTTİ.");
            OnWinterArrived?.Invoke();
        }
    }

    // =========================================================
    // 3. AŞAMA: SABAH RAPORU GÖSTERİMİ
    // =========================================================
    private void ShowMorningReport()
    {
        // Eğer sahnede MapEventManager yoksa (Örn: Savaş ekranındaysak) hata vermesin
        if (MapEventManager.Instance == null || MapEventManager.Instance.eventPanel == null) return;

        MapEventManager.Instance.eventPanel.SetActive(true);
        AudioManager.Instance.PlayPaper(); // Parşömen sesi

        MapEventManager.Instance.titleText.text = $"GÜN {currentDay} RAPORU";
        
        // Not: Şimdilik temsili yazıyor, ileride bu stringlerin içine MoneyManager'dan çektiğin net sayıları da yazdırabilirsin.
        string report = "Ordunun giderleri:\n\n";
        
        report += $"<color=red> {SupplyManager.Instance.GetExpectedDailyFoodCost()} erzak tüketildi.</color>\n";
        report += $"<color=red> {MoneyManager.Instance.GetExpectedDailyWageCost()} akçe maaş ödendi.</color>\n";
        report += $"<color=green> Birlikler dinlendi ve yaralarını sardı.</color>\n";

        MapEventManager.Instance.descText.text = report;

        // Önceki butonları temizle (Ters döngü ile)
        for (int i = MapEventManager.Instance.buttonContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = MapEventManager.Instance.buttonContainer.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        // "Emredersin" butonu ekle
        GameObject btnObj = Instantiate(MapEventManager.Instance.buttonPrefab, MapEventManager.Instance.buttonContainer);
        btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Anlaşıldı";
        btnObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
            MapEventManager.Instance.ClosePanel();
        });
    }

    // ... (Senin HealAllSoldiers, CheckForRandomEvent ve TriggerEvent fonksiyonların eskisi gibi duruyor, onlara dokunmadım) ...
    
    public void HealAllSoldiers(int percentageAmount)
    {
        var soldiers = FindObjectsOfType<Gladiator>();
        
        foreach(var soldier in soldiers)
        {
            float healAmount = soldier.maxHealth * (percentageAmount / 100.0f);
            soldier.currentHealth += healAmount;
            
            if(soldier.currentHealth > soldier.maxHealth) 
                soldier.currentHealth = soldier.maxHealth;

            if(soldier.healthBar != null)
                soldier.healthBar.UpdateBar(soldier.currentHealth, soldier.maxHealth);
        }
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show($"Tüm ordu %{percentageAmount} iyileşti.", NotificationType.Success);
    }

    void CheckForRandomEvent()
    {
        if (possibleEvents.Count == 0) return;

        if (UnityEngine.Random.Range(0, 100) < eventChance)
        {
            GameEvent randomEvent = possibleEvents[UnityEngine.Random.Range(0, possibleEvents.Count)];
            TriggerEvent(randomEvent);
        }
    }

    void TriggerEvent(GameEvent e)
    {
        if (e.moneyChange != 0) MoneyManager.Instance.Add(e.moneyChange); 
        if (e.foodChange != 0) SupplyManager.Instance.BuyFood(e.foodChange); 
        
        OnEventTriggered?.Invoke(e);
        Debug.Log($"OLAY VAR: {e.title}");
    }

    public void RefreshUI()
    {
        OnDayChanged?.Invoke(currentDay);
    }
}