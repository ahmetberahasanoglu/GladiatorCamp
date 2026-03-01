using UnityEngine;
using TMPro;

public class TopInfoBarUI : MonoBehaviour
{
    // --- YENİ EKLENEN KISIM: İstenilen yerden ulaşabilmek için Instance ---
    public static TopInfoBarUI Instance;

    [Header("UI Textleri")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI capacityText; // "Asker: 3/5"

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 1. EVENTLERE ABONE OL
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

        // 2. BAŞLANGIÇTA GÜNCELLE
        ForceUpdateAll();
    }

    void OnDestroy()
    {
        // ABONELİKLERİ İPTAL ET
        if (DayManager.Instance != null) DayManager.Instance.OnDayChanged -= UpdateDay;
        if (MoneyManager.Instance != null) MoneyManager.Instance.OnGoldChanged -= UpdateGold;
        if (SupplyManager.Instance != null) SupplyManager.Instance.OnFoodChanged -= UpdateFood;
        if (RecruitManager.Instance != null) RecruitManager.Instance.OnSoldierCountChanged -= UpdateCapacity;
        if (CampManager.Instance != null) CampManager.Instance.OnCampUpdated -= UpdateCapacity;
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
        goldText.text = $"{gold}";
    }

    void UpdateFood()
    {
        if(SupplyManager.Instance != null)
            foodText.text = $"{SupplyManager.Instance.currentFood}";
    }

    // YENİ: Başka scriptlerden çağrılabilmesi için "public" yapıldı
    public void UpdateCapacity() 
    {
        if (CampManager.Instance == null) return;

        // DÜZELTME: Artık cesetleri ve düşmanları saymıyoruz!
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        int currentCount = 0;

        foreach (var soldier in allSoldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            
            // Sadece bizim askerimizse ("MySoldier") VE ölmediyse sayıyı artır
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead))
            {
                currentCount++;
            }
        }

        int maxCap = CampManager.Instance.GetMaxSoldierCapacity();
        string color = (currentCount >= maxCap) ? "red" : "white";

        capacityText.text = $"<color={color}>{currentCount} / {maxCap}</color>";
    }

    public void ForceUpdateAll()
    {
        if(DayManager.Instance) UpdateDay(DayManager.Instance.currentDay);
        if(MoneyManager.Instance) UpdateGold(MoneyManager.Instance.gold);
        UpdateFood();
        UpdateCapacity();
    }
}