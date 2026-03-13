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
    public Image tempFillBar; 
    public Gradient tempGradient; 

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
        if(dayText != null)
            dayText.text = $"{day} / {30}";//100 belki
            
        if (day > 80 && dayText != null) dayText.color = Color.red;
    }

 void UpdateGold(int gold)
{
    if (goldText != null && MoneyManager.Instance != null) 
    {
        int current = MoneyManager.Instance.gold;
        int cost = MoneyManager.Instance.GetExpectedDailyWageCost();

        string formattedGold = current.ToString("N0");
        string formattedCost = cost.ToString("N0");

        goldText.text = $"{formattedGold} <size=70%><color=#ff6666>(-{formattedCost})</color></size>";
        
        if (isInitialized) FlashUI(goldText);
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
        }
    }

    void UpdateWood()
    {
        if (ResourceManager.Instance != null && woodText != null)
        {
            woodText.text = $"{ResourceManager.Instance.wood}";
            if (isInitialized) FlashUI(woodText);
        }
    }

    void UpdateNasipFromEvent(int nasip)
    {
        UpdateNasip();
    }

    void UpdateNasip()
    {
        if(NasipManager.Instance != null && nasipText != null)
        {
            nasipText.text = $"{NasipManager.Instance.currentNasip}";
            if (isInitialized) FlashUI(nasipText);
        }
    }

    void UpdateTemp()
    {
        if (CampSurvivalManager.Instance == null) return;

        int currentTemp = CampSurvivalManager.Instance.currentTemperature;

        if (tempText != null)
        {
            tempText.text = $"{currentTemp}°C";
            if (isInitialized) FlashUI(tempText);
        }

        if (tempFillBar != null)
        {
            float fillAmount = currentTemp / 100f; 
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

        int maxCap = CampManager.Instance.GetMaxSoldierCapacity();
        string color = (currentCount >= maxCap) ? "red" : "black";

        if (capacityText != null)
            capacityText.text = $"<color={color}>{currentCount} / {maxCap}</color>";

        if (isInitialized) FlashUI(capacityText);

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