using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TopInfoBarUI : MonoBehaviour
{
    public static TopInfoBarUI Instance;

    [Header("Mevcut UI Textleri")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI capacityText; 
    public TextMeshProUGUI nasipText; 

    [Header("Hayatta Kalma UI Textleri")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI tempText;
  
    public Gradient tempGradient; 



    [Header("Slider (Fill) İmajları")]
    public Image goldFillBar;
    public Image foodFillBar;
    public Image woodFillBar;
    public Image capacityFillBar;
    public Image dayFillBar;
    public Image nasipFillBar;
  public Image tempFillBar; 
    // --- PARLAMA (FLASH) EFEKTİ İÇİN ---
    private Dictionary<TextMeshProUGUI, Coroutine> activeFlashes = new Dictionary<TextMeshProUGUI, Coroutine>();
    public Color flashColor = new Color(0.1f, 1f, 0.2f); // Altın Sarısı / Parlak bir renk
    private bool isInitialized = false; // Başlangıçtaki toplu parlamayı engellemek için
    // -----------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (DayManager.Instance != null) DayManager.Instance.OnDayChanged += UpdateDay;
        if (MoneyManager.Instance != null) MoneyManager.Instance.OnGoldChanged += UpdateGold;
        if (SupplyManager.Instance != null) SupplyManager.Instance.OnFoodChanged += UpdateFood;
        if (RecruitManager.Instance != null) RecruitManager.Instance.OnSoldierCountChanged += UpdateCapacity;
        if (CampManager.Instance != null) CampManager.Instance.OnCampUpdated += UpdateCapacity;
        if (ResourceManager.Instance != null) ResourceManager.Instance.OnResourcesChanged += UpdateWood;
        if (CampSurvivalManager.Instance != null) CampSurvivalManager.Instance.OnTemperatureChanged += UpdateTemp;
        if (NasipManager.Instance != null) NasipManager.Instance.OnNasipChanged += UpdateNasipFromEvent;

        ForceUpdateAll();
        isInitialized = true; // İlk yükleme bitti, artık tetiklenen her şey parlayabilir!
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null) DayManager.Instance.OnDayChanged -= UpdateDay;
        if (MoneyManager.Instance != null) MoneyManager.Instance.OnGoldChanged -= UpdateGold;
        if (SupplyManager.Instance != null) SupplyManager.Instance.OnFoodChanged -= UpdateFood;
        if (RecruitManager.Instance != null) RecruitManager.Instance.OnSoldierCountChanged -= UpdateCapacity;
        if (CampManager.Instance != null) CampManager.Instance.OnCampUpdated -= UpdateCapacity;
        if (ResourceManager.Instance != null) ResourceManager.Instance.OnResourcesChanged -= UpdateWood;
        if (CampSurvivalManager.Instance != null) CampSurvivalManager.Instance.OnTemperatureChanged -= UpdateTemp;
        if (NasipManager.Instance != null) NasipManager.Instance.OnNasipChanged -= UpdateNasipFromEvent;
    }

    // --- GÜNCELLEME FONKSİYONLARI ---

   void UpdateDay(int day)
    {
        // ... (Eski text güncelleme kodun) ...
        if(dayText != null) dayText.text = $" {day}/{DayManager.Instance.maxDays}";
        if (day > 20 && dayText != null) dayText.color = Color.red;


        if (dayFillBar != null && DayManager.Instance != null)
        {
          
            dayFillBar.fillAmount = Mathf.Clamp01((float)day / DayManager.Instance.maxDays);
        }
    }

 void UpdateGold(int gold)
    {
        if (goldText != null && MoneyManager.Instance != null) 
        {
            int current = MoneyManager.Instance.gold;
            int cost = MoneyManager.Instance.GetExpectedDailyWageCost();

            goldText.text = $"{current} <size=70%><color=#ff6666>(-{cost})</color></size>";
            if (isInitialized) FlashUI(goldText);

            if (goldFillBar != null)
            {
                // UI limitleri direkt Manager'dan alıyor!
                goldFillBar.fillAmount = Mathf.Clamp01((float)current / MoneyManager.Instance.maxGold);
            }
        }
    }

    void UpdateFood()
    {
        if(SupplyManager.Instance != null && foodText != null)
        {
            int current = SupplyManager.Instance.currentFood;
            int cost = SupplyManager.Instance.GetExpectedDailyFoodCost();
            
            foodText.text = $"{current} <size=70%><color=#ff6666>(-{cost})</color></size>";
            if (isInitialized) FlashUI(foodText);

            if (foodFillBar != null)
            {
                // UI limitleri direkt Manager'dan alıyor!
                foodFillBar.fillAmount = Mathf.Clamp01((float)current / SupplyManager.Instance.maxFood);
            }
        }
    }

    void UpdateWood()
    {
        if (ResourceManager.Instance != null && woodText != null)
        {
            int current = ResourceManager.Instance.wood;
            woodText.text = $"{current}";
            if (isInitialized) FlashUI(woodText);

            if (woodFillBar != null)
            {
                // UI limitleri direkt Manager'dan alıyor!
                woodFillBar.fillAmount = Mathf.Clamp01((float)current / ResourceManager.Instance.maxWood);
            }
        }
    }

    void UpdateNasip()
    {
        if(NasipManager.Instance != null && nasipText != null)
        {
            int current = NasipManager.Instance.currentNasip;
            nasipText.text = $"{current}";
            if (isInitialized) FlashUI(nasipText);

            if (nasipFillBar != null)
            {
                // İnatçı 100f hatası tamamen silindi! Direkt Manager'a bakıyor.
                nasipFillBar.fillAmount = Mathf.Clamp01((float)current / NasipManager.Instance.maxNasip);
            }
        }
    }

    void UpdateNasipFromEvent(int nasip)
    {
        UpdateNasip();
    }

  

  void UpdateTemp()
    {
        if (CampSurvivalManager.Instance == null) return;

        int currentTemp = CampSurvivalManager.Instance.currentTemperature;
        int minTemp = CampSurvivalManager.Instance.minTemperature;
        int maxTemp = CampSurvivalManager.Instance.maxTemperature;

        if (tempText != null)
        {
            string colorHex = currentTemp <= 0 ? "#88CCFF" : "#000000";
            tempText.text = $"<color={colorHex}>{currentTemp}°C</color>";
            if (isInitialized) FlashUI(tempText);
        }

        if (tempFillBar != null)
        {
            // MATEMATİK DÜZELTİLDİ: Artık Integer bölmesi yapıp 1 veya 0 vermeyecek!
            float fillAmount = Mathf.Clamp01((float)(currentTemp - minTemp) / (float)(maxTemp - minTemp)); 
            
            tempFillBar.fillAmount = fillAmount;
            tempFillBar.color = tempGradient.Evaluate(fillAmount); 
        }
    }

   
   public void UpdateCapacity() 
    {
        if (CampManager.Instance == null) return;

        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        int currentCount = 0;

        foreach (var soldier in allSoldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead))
            {
                currentCount++;
            }
        }

        int maxCap = CampManager.Instance.GetMaxSoldierCapacity(); // 3, 5, 7 diye değişen değer
        string color = (currentCount >= maxCap) ? "red" : "black";

        if (capacityText != null)
            capacityText.text = $"<color={color}>{currentCount} / {maxCap}</color>";

        if (isInitialized) FlashUI(capacityText);

        // YENİ: Kapasite Barını Güncelle (Dinamik Max Değere Göre)
        if (capacityFillBar != null)
        {
            // O anki maksimum çadır kapasiten neyse oran ona göre hesaplanır
            capacityFillBar.fillAmount = Mathf.Clamp01((float)currentCount / maxCap);
        }

        UpdateFood(); 
        if (MoneyManager.Instance != null) UpdateGold(MoneyManager.Instance.gold);
    }

    public void ForceUpdateAll()
    {
        if(DayManager.Instance) UpdateDay(DayManager.Instance.currentDay);
        if(MoneyManager.Instance) UpdateGold(MoneyManager.Instance.gold);
        UpdateFood();
        UpdateCapacity();
        UpdateWood(); 
        UpdateTemp(); 
        UpdateNasip(); 
    }

    // =========================================================
    // SİHİRLİ PARLAMA (FLASH) SİSTEMİ
    // =========================================================
   public void FlashUI(TextMeshProUGUI uiText)
    {
        if (uiText == null) return;

       
        if (!gameObject.activeInHierarchy) return;

        // Eski animasyon devam ediyorsa durdur
        if (activeFlashes.ContainsKey(uiText))
        {
            if (activeFlashes[uiText] != null) StopCoroutine(activeFlashes[uiText]);
            activeFlashes.Remove(uiText);
        }

        // Yeni parlamayı başlat
        Coroutine newFlash = StartCoroutine(FlashRoutine(uiText));
        activeFlashes.Add(uiText, newFlash);
    }

    private IEnumerator FlashRoutine(TextMeshProUGUI uiText)
    {
        Color originalColor = Color.black; 
       // Color originalColor = uiText.color;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = new Vector3(1.3f, 1.3f, 1.3f); 

        float duration = 0.15f; 
        float t = 0;

        // 1. AŞAMA: Büyü ve Renk Değiştir
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float normalizedTime = t / duration;
            
            uiText.color = Color.Lerp(originalColor, flashColor, normalizedTime);
            uiText.transform.localScale = Vector3.Lerp(originalScale, targetScale, normalizedTime);
            yield return null;
        }

        // 2. AŞAMA: Yavaşça Sön
        t = 0;
        duration = 0.35f; 
        
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float normalizedTime = t / duration;
            
            uiText.color = Color.Lerp(flashColor, originalColor, normalizedTime);
            uiText.transform.localScale = Vector3.Lerp(targetScale, originalScale, normalizedTime);
            yield return null;
        }

        uiText.color = originalColor;
        uiText.transform.localScale = originalScale;
        activeFlashes.Remove(uiText);
    }
}