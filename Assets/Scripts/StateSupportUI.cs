using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

// --- YENİ VERİ YAPISI: Kart Verilerini Tutar ---
[System.Serializable]
public struct LoanTierCard
{
    public GameObject cardObject; // Kartın UI objesi (Tüm kartı açıp kapatmak için)
    public TextMeshProUGUI loanAmountText; // "1000 Akçe" yazısı
    public TextMeshProUGUI requiredRepText; // "40'tan Fazla İtibar" yazısı
    public Button actionButton; // "Borç İste" veya "Öde" butonu
    public TextMeshProUGUI buttonText; // Butonun üzerindeki yazı
    public GameObject lockedOverlay; // Kilit simgesi ve "İtibar Yetersiz" yazısı (Eğer itibar < X ise açılacak)
}

public class StateSupportUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public GameObject loanPanel;
    public TextMeshProUGUI loanStatusText; 
    public TextMeshProUGUI cooldownText; // "Yeni borç için 3 gün bekleyin" yazısı

    [Header("Kademeli Kartlar (YENİ)")]
    public List<LoanTierCard> loanCards; // 1000, 2000, 3000 kartlarını buraya sürükle

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

    /// <summary>
    /// Inspector'daki butona bu metodu bağla (loanPanel.SetActive yerine).
    /// Açılmadan önce PanelManager üzerinden diğer paneller kapatılır.
    /// </summary>
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
        if (StateLoanManager.Instance == null) return;
        var manager = StateLoanManager.Instance;
        int currentDay = DayManager.Instance.currentDay;

        // 1. Durum Metni ve Cooldown
        if (currentDay < manager.nextAvailableLoanDay)
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

        // 2. Kademeli Kartları Yönet (En Kritik Kısım)
        for (int i = 0; i < loanCards.Count; i++)
        {
            LoanTierCard card = loanCards[i];
            
            // Kartın borç miktarını kodla (Örn: 1000, 2000, 3000)
            int tierAmount = (i + 1) * 1000;
            card.loanAmountText.text = $"{tierAmount} Akçe";
            
            // Gerekli itibarı kodla (i=0 için 40, i=1 için 60, i=2 için 80)
            int requiredRep = 40 + (i * 20);
            card.requiredRepText.text = $"Gerekli İtibar: <color=#AF2C3E>{requiredRep}+</color>";

            // Aktif bir borç varsa, o kartı "Öde" kartına dönüştür, diğerlerini kapat.
            if (manager.hasActiveLoan && manager.loanAmount == tierAmount)
            {
                card.actionButton.gameObject.SetActive(true);
                card.buttonText.text = "Borcu öde";
                card.actionButton.onClick.RemoveAllListeners();
                card.actionButton.onClick.AddListener(() => manager.RepayLoan());
                
                // Kilit simgesini kapat (Kendi borcumuzu ödüyoruz, itibar yetersiz olamaz)
                card.lockedOverlay.SetActive(false);
                card.actionButton.interactable = true;
                
                card.cardObject.SetActive(true); // Bizim borcumuz olan kart açık kalır
            }
            // Başka bir borç aktifse, bu kartı kapat.
            else if (manager.hasActiveLoan && manager.loanAmount != tierAmount)
            {
                card.cardObject.SetActive(false); 
            }
            // Aktif borç yoksa, normal kilitli/açık borç alma moduna geç.
            else
            {
                card.cardObject.SetActive(true); // Tüm kartları aç
                card.actionButton.gameObject.SetActive(true);
                card.buttonText.text = "Borç Al";
                card.actionButton.onClick.RemoveAllListeners();
                card.actionButton.onClick.AddListener(() => manager.RequestLoan(tierAmount)); // YENİ: Miktarı fonksiyona yolla

                // Kilitli mi? (İtibar yetersizse veya Cooldown aktifse kilit aç)
                int currentRep = ReputationManager.Instance.GetReputation();
                bool tooLowRep = currentRep < requiredRep;
                bool cooldownActive = currentDay < manager.nextAvailableLoanDay;

                if (tooLowRep || cooldownActive)
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

        // Bağış butonu kontrolü
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