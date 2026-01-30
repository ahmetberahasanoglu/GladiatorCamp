using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class WorkPlaceManager : MonoBehaviour
{
    public static WorkPlaceManager Instance;

    [Header("Ayarlar")]
    public bool resetAtNight = false; // TRUE yaparsan her sabah herkes talime döner (Bugünlük çalışırlar)

    [Header("UI Bağlantıları")]
    public GameObject workPanel;
    public Transform contentArea;
    public GameObject slotPrefab;
    
    // Toplam kazancı anlık görmek istersen panele bir text koyabilirsin
    public TextMeshProUGUI totalIncomeText; 

    void Awake()
    {
        Instance = this;
        workPanel.SetActive(false);
    }

    void Start()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged += EndOfDayRoutine;
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= EndOfDayRoutine;
    }

    public void OpenPanel()
    {
        workPanel.SetActive(true);
        RefreshList();
        UpdateTotalIncomeText();
    }

    public void ClosePanel()
    {
        workPanel.SetActive(false);
    }

    public void RefreshList()
    {
        // Önce temizle
        foreach (Transform child in contentArea) Destroy(child.gameObject);

        // Tüm askerleri bul (veya RecruitManager listesinden çek)
        var allSoldiers = FindObjectsOfType<Gladiator>();

        foreach (var soldier in allSoldiers)
        {
            // Sadece MÜSAİT olanları listele (Ölüler veya savaştakiler gelmesin)
            if (soldier.IsAvailableForWork())
            {
                GameObject newSlot = Instantiate(slotPrefab, contentArea);
                newSlot.GetComponent<WorkSlotUI>().Setup(soldier);
            }
        }
    }

    // Her butona basıldığında UI'daki toplam kazanç yazısını güncellemek için
    // Bunu WorkSlotUI'dan çağırmak gerekebilir ama şimdilik panel açılışında hesaplıyoruz
    void UpdateTotalIncomeText()
    {
        if (totalIncomeText == null) return;
        // Basit bir döngü ile hesaplanabilir...
    }

    // --- GÜN SONU HESAPLAMASI ---
    void EndOfDayRoutine(int day)
    {
        int totalIncome = 0;
        int workerCount = 0;
        
        var allSoldiers = FindObjectsOfType<Gladiator>();

        foreach (var soldier in allSoldiers)
        {
            // Sadece "Working" modunda olanlar para getirir
            if (soldier.currentActivity == SoldierActivity.Working)
            {
                totalIncome += soldier.dailyWage;
                workerCount++;

                // Eğer "Bugünlük" çalışsın istiyorsan, parayı aldıktan sonra resetle:
                if (resetAtNight)
                {
                    soldier.SetActivity(SoldierActivity.Training);
                }
            }
        }

        if (totalIncome > 0)
        {
            MoneyManager.Instance.Add(totalIncome);
            
            // Notification gönder
            if(NotificationManager.Instance != null)
            {
                NotificationManager.Instance.Show(
                    $"Lonca Geliri: {workerCount} asker çalıştı, {totalIncome} Akçe kazanıldı.", 
                    NotificationType.Success
                );
            }
        }
    }
}