using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StateSupportUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public GameObject loanPanel; // Borç panelinin kendisi
    public TextMeshProUGUI loanStatusText; 
    public Button borrowButton;
    public Button repayButton;
    
    // (İtibar referanslarını zaten ReputationManager'dan hallediyoruz)

    void Start()
    {
        // Butonları bağla
        borrowButton.onClick.AddListener(() => StateLoanManager.Instance.RequestLoan());
        repayButton.onClick.AddListener(() => StateLoanManager.Instance.RepayLoan());

        // --- DİKKAT: YENİ EVENT BAĞLANTISI ---
        if (StateLoanManager.Instance != null)
        {
            StateLoanManager.Instance.OnLoanStateChanged += UpdateLoanPanel;
        }

        // Paneli ilk açılışta güncelle
        UpdateLoanPanel();
    }

    void OnDestroy()
    {
        // Abonelikten çıkmayı unutma
        if (StateLoanManager.Instance != null)
        {
            StateLoanManager.Instance.OnLoanStateChanged -= UpdateLoanPanel;
        }
    }

    public void UpdateLoanPanel()
    {
        // Eğer manager yoksa hata vermesin diye kontrol
        if (StateLoanManager.Instance == null) return;
        
        var manager = StateLoanManager.Instance;

        if (manager.hasActiveLoan)
        {
            // Borç VARSA
            loanStatusText.text = $"BORÇ: {manager.loanAmount} Akçe\nSON GÜN: {manager.loanDueDay}. Gün";
            loanStatusText.color = Color.red;
            
            borrowButton.gameObject.SetActive(false); 
            repayButton.gameObject.SetActive(true);   
        }
        else
        {
            // Borç YOKSA
            loanStatusText.text = "Aktif Borç Yok";
            loanStatusText.color = Color.green;

            borrowButton.gameObject.SetActive(true);
            repayButton.gameObject.SetActive(false);
        }
    }
    
    // Panel açılıp kapandığında da güncel kalsın
    void OnEnable()
    {
        UpdateLoanPanel();
    }
}