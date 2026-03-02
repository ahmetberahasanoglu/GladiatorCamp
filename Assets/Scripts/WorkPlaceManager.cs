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

    // Start ve OnDestroy içindeki event aboneliklerini sildik!

    public void OpenPanel()
    {
        workPanel.SetActive(true);
        RefreshList();
    }

    public void ClosePanel()
    {
        workPanel.SetActive(false);
    }

    public void RefreshList()
    {
        foreach (Transform child in contentArea) Destroy(child.gameObject);

        var allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);

        foreach (var soldier in allSoldiers)
        {
            if (soldier.IsAvailable)
            {
                GameObject newSlot = Instantiate(slotPrefab, contentArea);
                newSlot.GetComponent<WorkSlotUI>().Setup(soldier);
            }
        }
        
        UpdateSummary();
    }

    public void UpdateSummary()
    {
        int count = 0;
        int gold = 0;
        
        var allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        
        foreach (var s in allSoldiers)
        {
            if (s.data != null && s.data.currentActivity == SoldierActivity.Working)
            {
                count++;
                gold += s.data.dailyWage;
            }
        }
        if(summaryText) summaryText.text = $"Çalışan: {count} Kişi \nGünlük Beklenen Gelir: {gold} Akçe";
    }

    // YENİ: public yaptık, daysPassed parametresi ekledik ve geliri günle çarptık
    public void EndOfDayPayment(int daysPassed)
    {
        int totalIncome = 0;
        int workerCount = 0;
        
        var allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);

        foreach (var soldier in allSoldiers)
        {
            if (soldier.data != null && soldier.data.currentActivity == SoldierActivity.Working)
            {
                // Günlük maaşını geçen gün sayısıyla çarpıp kasaya ekliyoruz!
                totalIncome += (soldier.data.dailyWage * daysPassed);
                workerCount++;

                // İşleri bittiği için tekrar talime döndür
                soldier.SetActivity(SoldierActivity.Training);
            }
        }

        if (totalIncome > 0)
        {
            MoneyManager.Instance.Add(totalIncome);
            
            if(NotificationManager.Instance != null)
            {
                NotificationManager.Instance.Show(
                    $"{workerCount} asker {daysPassed} günlük çalışmayı tamamladı ve toplam {totalIncome} Akçe kazandırdı.", 
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