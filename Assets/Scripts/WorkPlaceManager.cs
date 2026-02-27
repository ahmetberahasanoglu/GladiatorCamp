using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class WorkplaceManager : MonoBehaviour
{
    public static WorkplaceManager Instance;

    [Header("UI Ayarları")]
    public GameObject workPanel;       
    public Transform contentArea;      
    public GameObject slotPrefab;      
    public TextMeshProUGUI summaryText;

    void Awake()
    {
        Instance = this;
        workPanel.SetActive(false);
    }

    void Start()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged += EndOfDayPayment;
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= EndOfDayPayment;
    }

    public void OpenPanel()
    {
        workPanel.SetActive(true);
        RefreshList();
    }

    public void ClosePanel()
    {
        workPanel.SetActive(false);
    }

    // Listeyi oluşturma
    public void RefreshList()
    {
        // Temizlik
        foreach (Transform child in contentArea) Destroy(child.gameObject);

        // --- GÜNCEL KOD: FindObjectsByType ve SortMode.None kullanıyoruz ---
        var allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);

        foreach (var soldier in allSoldiers)
        {
            // Sadece yaşıyorsa ve müsaitse listele
            if (soldier.IsAvailable)
            {
                GameObject newSlot = Instantiate(slotPrefab, contentArea);
                newSlot.GetComponent<WorkSlotUI>().Setup(soldier);
            }
        }
        
        UpdateSummary();
    }

    // (Opsiyonel) Altta özet bilgi göstermek için
    public void UpdateSummary()
    {
        int count = 0;
        int gold = 0;
        
        // --- GÜNCEL KOD ---
        var allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        
        foreach (var s in allSoldiers)
        {
            if (s.data != null && s.data.currentActivity == SoldierActivity.Working)
            {
                count++;
                // Eğer dailyWage data içindeyse s.data.dailyWage yapabilirsin
                gold += s.data.dailyWage;
            }
        }
        if(summaryText) summaryText.text = $"Çalışan: {count} Kişi \nBeklenen Gelir: {gold} Akçe";
    }

    // --- GÜN SONU MANTIĞI ---
    void EndOfDayPayment(int day)
    {
        int totalIncome = 0;
        int workerCount = 0;
        
        // --- GÜNCEL KOD ---
        var allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);

        foreach (var soldier in allSoldiers)
        {
            if (soldier.data != null && soldier.data.currentActivity == SoldierActivity.Working)
            {
                totalIncome += soldier.data.dailyWage;
                workerCount++;

                // KRİTİK NOKTA: "Bugünlük" dediğin için, parayı aldıktan sonra
                // askeri tekrar talime döndürüyoruz
                soldier.SetActivity(SoldierActivity.Training);
            }
        }

        if (totalIncome > 0)
        {
            MoneyManager.Instance.Add(totalIncome);
            
            if(NotificationManager.Instance != null)
            {
                NotificationManager.Instance.Show(
                    $"{workerCount} asker çalışmayı tamamladı ve {totalIncome} Akçe kazandırdı.", 
                    NotificationType.Success
                );
            }
        }
        
        if (workPanel.activeSelf)
        {
            RefreshList(); 
        }
    }
}