using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StateSupportUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public GameObject loanPanel; 
    public TextMeshProUGUI loanStatusText; 
    
    [Header("Borç Butonları")]
    public Button borrowButton;
    public Button repayButton;

    [Header("Bağış Alanı (Yeni)")]
    public Button donateButton;      // "Bağış Yap" butonu
    public TextMeshProUGUI donateInfoText; // "500 Akçe Bağışla (+5 İtibar)" yazısı
    public int donationCost = 500;   // Kaç para gidecek?
    public int donationReward = 5;   // Kaç itibar gelecek?

    void Start()
    {
        // Borç Butonları
        borrowButton.onClick.AddListener(() => StateLoanManager.Instance.RequestLoan());
        repayButton.onClick.AddListener(() => StateLoanManager.Instance.RepayLoan());

        // --- YENİ: Bağış Butonu ---
        donateButton.onClick.AddListener(OnDonateClicked);
        
        // Butonun üzerindeki yazıyı ayarla (Otomatik)
        if(donateInfoText != null)
            donateInfoText.text = $"{donationCost} Akçe Bağışla\n<size=80%>(+{donationReward} İtibar)</size>";

        // Eventleri Dinle
        if (StateLoanManager.Instance != null)
            StateLoanManager.Instance.OnLoanStateChanged += UpdateLoanPanel;
        
        // Paneli Güncelle
        UpdateLoanPanel();
    }

    void OnDonateClicked()
    {
        // Direkt Manager'daki yeni fonksiyonu çağırıyoruz
        ReputationManager.Instance.DonateToState(donationCost, donationReward);
        
        // Belki bir ses efekti veya popup burada ekleyebilirsin
    }

    public void UpdateLoanPanel()
    {
        if (StateLoanManager.Instance == null) return;
        var manager = StateLoanManager.Instance;

        // Borç Durumu UI Güncellemesi
        if (manager.hasActiveLoan)
        {
            loanStatusText.text = $"BORÇ: <color=red>{manager.loanAmount}</color> Akçe\nSON GÜN: {manager.loanDueDay}. Gün";
            borrowButton.gameObject.SetActive(false); 
            repayButton.gameObject.SetActive(true);   
        }
        else
        {
            loanStatusText.text = "Aktif Borç Yok\n<size=70%>Devlet hazinesi emrinize amadedir.</size>";
            borrowButton.gameObject.SetActive(true);
            repayButton.gameObject.SetActive(false);
        }

        // Bağış butonu kontrolü (Paramız yoksa buton sönük olsun)
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