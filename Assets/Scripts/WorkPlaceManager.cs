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
            // --- DÜZELTİLDİ: Artık sadece TAMAMEN BOŞTA olanlar çalışma listesinde görünür ---
            if (soldier.IsAvailableForTask())
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
                totalIncome += (soldier.data.dailyWage * daysPassed);
                workerCount++;
                soldier.SetActivity(SoldierActivity.Idling);
            }
        }


       if (totalIncome > 0)
        {
           
            if (NasipManager.Instance != null && NasipManager.Instance.currentNasip >= 5) 
            {
                if (Random.Range(0, 100) < 20) 
                {
                    totalIncome *= 2;
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayCheer();
                    if (NotificationManager.Instance != null)
                        NotificationManager.Instance.Show($"<color=#FFD700>BEREKET!</color> Yüce gönüllülüğün sayesinde bugün işler bereketli geçti. Kazanç İKİYE KATLANDI! (+{totalIncome} Akçe)", NotificationType.Success);
                }
            }
          

            MoneyManager.Instance.Add(totalIncome);
            }

        if (workPanel.activeSelf)
        {
            RefreshList(); 
        }
    }
}