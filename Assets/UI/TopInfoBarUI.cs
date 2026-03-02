using UnityEngine;
using UnityEngine.UI; // Fill bar (Image) kullanacağımız için eklendi
using TMPro;

public class TopInfoBarUI : MonoBehaviour
{
    public static TopInfoBarUI Instance;

    [Header("Mevcut UI Textleri")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI capacityText; // "Asker: 3/5"

    [Header("YENİ: Hayatta Kalma UI Textleri")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI tempText;
    public Image tempFillBar; // Opsiyonel: Sıcaklık Barı
    public Gradient tempGradient; // Mavi(0) -> Kırmızı(1) renk geçişi

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 1. EVENTLERE ABONE OL (Eskiler)
        if (DayManager.Instance != null) 
            DayManager.Instance.OnDayChanged += UpdateDay;

        if (MoneyManager.Instance != null) 
            MoneyManager.Instance.OnGoldChanged += UpdateGold;
        
        if (SupplyManager.Instance != null) 
            SupplyManager.Instance.OnFoodChanged += UpdateFood;

        if (RecruitManager.Instance != null)
            RecruitManager.Instance.OnSoldierCountChanged += UpdateCapacity;

        if (CampManager.Instance != null)
            CampManager.Instance.OnCampUpdated += UpdateCapacity;

        // --- YENİ EVENT ABONELİKLERİ ---
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged += UpdateWood;

        if (CampSurvivalManager.Instance != null)
            CampSurvivalManager.Instance.OnTemperatureChanged += UpdateTemp;

        // 2. BAŞLANGIÇTA GÜNCELLE
        ForceUpdateAll();
    }

    void OnDestroy()
    {
        // ABONELİKLERİ İPTAL ET (Eskiler)
        if (DayManager.Instance != null) DayManager.Instance.OnDayChanged -= UpdateDay;
        if (MoneyManager.Instance != null) MoneyManager.Instance.OnGoldChanged -= UpdateGold;
        if (SupplyManager.Instance != null) SupplyManager.Instance.OnFoodChanged -= UpdateFood;
        if (RecruitManager.Instance != null) RecruitManager.Instance.OnSoldierCountChanged -= UpdateCapacity;
        if (CampManager.Instance != null) CampManager.Instance.OnCampUpdated -= UpdateCapacity;

        // --- YENİ ABONELİK İPTALLERİ ---
        if (ResourceManager.Instance != null) ResourceManager.Instance.OnResourcesChanged -= UpdateWood;
        if (CampSurvivalManager.Instance != null) CampSurvivalManager.Instance.OnTemperatureChanged -= UpdateTemp;
    }

    // --- GÜNCELLEME FONKSİYONLARI ---

    void UpdateDay(int day)
    {
        if(dayText != null)
            dayText.text = $"{day} / {100}";
            
        if (day > 80 && dayText != null) dayText.color = Color.red;
    }

    void UpdateGold(int gold)
    {
        if (goldText != null) goldText.text = $"{gold}";
    }

    void UpdateFood()
    {
        if(SupplyManager.Instance != null && foodText != null)
            foodText.text = $"{SupplyManager.Instance.currentFood}";
    }

    // --- YENİ: ODUN GÜNCELLEMESİ ---
    void UpdateWood()
    {
        if (ResourceManager.Instance != null && woodText != null)
        {
            woodText.text = $"{ResourceManager.Instance.wood}";
        }
    }

    // --- YENİ: SICAKLIK GÜNCELLEMESİ ---
    void UpdateTemp()
    {
        if (CampSurvivalManager.Instance == null) return;

        int currentTemp = CampSurvivalManager.Instance.currentTemperature;

        if (tempText != null)
        {
            tempText.text = $"{currentTemp}°C";
        }

        if (tempFillBar != null)
        {
            float fillAmount = currentTemp / 100f; // 0 ile 1 arasına al
            tempFillBar.fillAmount = fillAmount;
            tempFillBar.color = tempGradient.Evaluate(fillAmount); // Rengi gradyandan seç
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
        string color = (currentCount >= maxCap) ? "red" : "white";

        if (capacityText != null)
            capacityText.text = $"<color={color}>{currentCount} / {maxCap}</color>";
    }

    public void ForceUpdateAll()
    {
        if(DayManager.Instance) UpdateDay(DayManager.Instance.currentDay);
        if(MoneyManager.Instance) UpdateGold(MoneyManager.Instance.gold);
        UpdateFood();
        UpdateCapacity();
        UpdateWood(); // YENİ
        UpdateTemp(); // YENİ
    }
}