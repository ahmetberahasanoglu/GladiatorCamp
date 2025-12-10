using UnityEngine;
using TMPro;

public class TopInfoBarUI : MonoBehaviour
{
    [Header("UI Textleri")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI capacityText; // "Asker: 3/5"

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
        // ABONELİKLERİ İPTAL ET (Hata almamak için şart)
        if (DayManager.Instance != null) DayManager.Instance.OnDayChanged -= UpdateDay;
        if (MoneyManager.Instance != null) MoneyManager.Instance.OnGoldChanged -= UpdateGold;
        if (SupplyManager.Instance != null) SupplyManager.Instance.OnFoodChanged -= UpdateFood;
        if (RecruitManager.Instance != null) RecruitManager.Instance.OnSoldierCountChanged -= UpdateCapacity;
        if (CampManager.Instance != null) CampManager.Instance.OnCampUpdated -= UpdateCapacity;
    }

    // --- GÜNCELLEME FONKSİYONLARI ---

    void UpdateDay(int day)
    {
        dayText.text = $"Gün: {day}";
    }

    void UpdateGold(int gold)
    {
        goldText.text = $"{gold}";
    }

    void UpdateFood()
    {
        // SupplyManager'ın event'i parametre almıyorsa direkt instance'tan çekiyoruz
        if(SupplyManager.Instance != null)
            foodText.text = $"{SupplyManager.Instance.currentFood} Erzak";
    }

    void UpdateCapacity()
    {
        if (RecruitManager.Instance == null || CampManager.Instance == null) return;

        // Mevcut asker sayısı
        int currentCount = FindObjectsOfType<Gladiator>().Length;
        // Maksimum kapasite
        int maxCap = CampManager.Instance.GetMaxSoldierCapacity();

        // Renklendirme: Doluysa Kırmızı, Boş yer varsa Beyaz
        string color = (currentCount >= maxCap) ? "red" : "white";

        capacityText.text = $"Asker: <color={color}>{currentCount} / {maxCap}</color>";
    }

    public void ForceUpdateAll()
    {
        if(DayManager.Instance) UpdateDay(DayManager.Instance.currentDay);
        if(MoneyManager.Instance) UpdateGold(MoneyManager.Instance.gold);
        UpdateFood();
        UpdateCapacity();
    }
}