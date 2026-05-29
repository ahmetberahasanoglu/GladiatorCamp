using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public struct LoanTierCard
{
    public GameObject cardObject; 
    public TextMeshProUGUI loanAmountText; 
    public TextMeshProUGUI requiredRepText; 
    public Button actionButton; 
    public TextMeshProUGUI buttonText; 
    public GameObject lockedOverlay; 
}

public class StateSupportUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public GameObject loanPanel;
    public TextMeshProUGUI loanStatusText; 
    public TextMeshProUGUI cooldownText; 

    [Header("Kademeli Kartlar")]
    public List<LoanTierCard> loanCards; 

    [Header("Bağış Alanı")]
    public Button donateButton;      
    public TextMeshProUGUI donateInfoText; 
    public int donationCost = 500;   
    public int donationReward = 5;   

    void Start()
    {
        donateButton.onClick.AddListener(OnDonateClicked);
        
        if(donateInfoText != null)
            donateInfoText.text = $"{donationCost} Akçe Bağışla\n<size=80%>(+{donationReward} İtibar)</size>";

        if (StateLoanManager.Instance != null)
            StateLoanManager.Instance.OnLoanStateChanged += UpdateLoanPanel;
        
        UpdateLoanPanel();
    }

    public void OpenLoanPanel()
    {
        if (PanelManager.Instance != null)
            PanelManager.Instance.OpenPanel(loanPanel, "Vakıf");
        else
            loanPanel.SetActive(!loanPanel.activeSelf);
    }

    public void CloseLoanPanel()
    {
        if (PanelManager.Instance != null)
            PanelManager.Instance.ClosePanel(loanPanel);
        else
            loanPanel.SetActive(false);
    }

    void OnDonateClicked()
    {
        ReputationManager.Instance.DonateToState(donationCost, donationReward);
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show("Vakfa bağış yapıldı. Dualar seninle.", NotificationType.Success);
    }

  public void UpdateLoanPanel()
    {
        if (StateLoanManager.Instance == null || DayManager.Instance == null) return;
        var manager = StateLoanManager.Instance;
        int currentDay = DayManager.Instance.currentDay;
        
        // ── Kış (Son 10 Gün) Kısıtlaması ──
        int maxDays = DayManager.Instance.maxDays;
        bool isWinterRestriction = currentDay >= (maxDays - 10);

        // 1. Durum Metni ve Cooldown
        if (manager.hasActiveLoan)
        {
            // YENİ: Borç günlerini hesapla
           // KESİN ÇÖZÜM: Borç günlerini şaşmaz matematik ile hesapla
int daysElapsed = currentDay - manager.dayLoanTaken; // Alındığı günden beri tam olarak kaç gün geçti
int remainingDays = manager.paymentPeriod - daysElapsed; // Toplam vadeden, geçen günü çıkar

            loanStatusText.text = $"Aktif Emanet: <color=#AF2C3E>{manager.loanAmount} Akçe</color>";
            
            if (remainingDays > 0)
            {
                cooldownText.text = $"Vade bitimine <color=#AF2C3E>{remainingDays} gün</color> kaldı.\n<size=80%>(Emaneti alalı {daysElapsed} gün oldu)</size>";
            }
            else if (remainingDays == 0)
            {
                cooldownText.text = $"<color=red>DİKKAT: Emaneti teslim etmenin son günü!</color>";
            }
            else
            {
                cooldownText.text = $"<color=red>Ödeme {Mathf.Abs(remainingDays)} gün gecikti! İtibarın düşüyor!</color>";
            }
            cooldownText.gameObject.SetActive(true);
        }
        else if (isWinterRestriction)
        {
            loanStatusText.text = "Kış Hazırlıkları Başladı";
            cooldownText.text = "<color=#AF2C3E>Seferin son 10 gününde vakıf borç vermez.</color>";
            cooldownText.gameObject.SetActive(true);
        }
        else if (currentDay < manager.nextAvailableLoanDay)
        {
            int remainingDays = manager.nextAvailableLoanDay - currentDay;
            loanStatusText.text = "Vakıf Defterleri Düzenliyor...";
            cooldownText.text = $"Yeni bir emanet için <color=#AF2C3E>{remainingDays} gün</color> bekleyin.";
            cooldownText.gameObject.SetActive(true);
        }
        else
        {
            loanStatusText.text = "Aktif Emanet Yok\n<size=70%>Vakıf sandığı dar gününde yanındadır.</size>";
            cooldownText.gameObject.SetActive(false);
        }

        // 2. Kademeli Kartları Yönet
        for (int i = 0; i < loanCards.Count; i++)
        {
            LoanTierCard card = loanCards[i];
            
            int tierAmount = (i + 1) * 1000;
            card.loanAmountText.text = $"{tierAmount} Akçe";
            
            int requiredRep = 40 + (i * 20);
            card.requiredRepText.text = $"Gerekli İtibar: <color=#AF2C3E>{requiredRep}+</color>";

            // Kendi borcumuz olan kart
            if (manager.hasActiveLoan && manager.loanAmount == tierAmount)
            {
                card.actionButton.gameObject.SetActive(true);
                card.buttonText.text = "Borcu öde";
                card.actionButton.onClick.RemoveAllListeners();
                card.actionButton.onClick.AddListener(() => manager.RepayLoan());
                
                card.lockedOverlay.SetActive(false);
                card.actionButton.interactable = true;
                
                card.cardObject.SetActive(true); 
            }
            // Diğer kartları gizle
            else if (manager.hasActiveLoan && manager.loanAmount != tierAmount)
            {
                card.cardObject.SetActive(false); 
            }
            // Borç yoksa normal alma ekranı
            else
            {
                card.cardObject.SetActive(true); 
                card.actionButton.gameObject.SetActive(true);
                card.buttonText.text = "Borç Al";
                card.actionButton.onClick.RemoveAllListeners();
                card.actionButton.onClick.AddListener(() => manager.RequestLoan(tierAmount)); 

                int currentRep = ReputationManager.Instance.GetReputation();
                bool tooLowRep = currentRep < requiredRep;
                bool cooldownActive = currentDay < manager.nextAvailableLoanDay;

                // Kış kısıtlaması veya yetersiz itibar varsa kartı da kilitle
                if (tooLowRep || cooldownActive || isWinterRestriction)
                {
                    card.lockedOverlay.SetActive(true);
                    card.actionButton.interactable = false;
                }
                else
                {
                    card.lockedOverlay.SetActive(false);
                    card.actionButton.interactable = true;
                }
            }
        }

        if (donateButton != null && MoneyManager.Instance != null)
        {
            donateButton.interactable = MoneyManager.Instance.gold >= donationCost;
        }
    }

    void OnEnable()
    {
        UpdateLoanPanel();
    }
}