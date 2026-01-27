using UnityEngine;
using System; // Action eventini kullanmak için gerekli

public class StateLoanManager : MonoBehaviour
{
    public static StateLoanManager Instance;

    [Header("Borç Ayarları")]
    public bool hasActiveLoan = false; 
    public int loanAmount = 0;         
    public int loanDueDay = 0;         
    public int paymentPeriod = 7;      
    
    [Header("Limitler ve Cezalar")]
    public int minRepToBorrow = 50;    
    public int defaultLoanAmount = 2000; 
    public int latePenaltyRep = 15;    

    // --- YENİ EKLENEN EVENT ---
    // UI'ın dinlemesi için "Borç Durumu Değişti" sinyali
    public event Action OnLoanStateChanged;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged += CheckLoanStatus;
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayChanged -= CheckLoanStatus;
    }

    public void RequestLoan()
    {
        if (hasActiveLoan)
        {
            Debug.Log("Zaten ödenmemiş bir borcun var!");
            return;
        }

        if (ReputationManager.Instance.GetReputation() < minRepToBorrow)
        {
            Debug.Log("Devlet sana güvenmiyor! (İtibar < 50)");
            return;
        }

        // DİKKAT: MoneyManager'daki fonksiyonun adı AddGold ise burayı düzelt
        MoneyManager.Instance.Add(defaultLoanAmount); 
        
        hasActiveLoan = true;
        loanAmount = defaultLoanAmount;
        loanDueDay = DayManager.Instance.currentDay + paymentPeriod;

        Debug.Log($"Devletten {loanAmount} Akçe borç alındı. Son ödeme: Gün {loanDueDay}");
        
        // HATA ÇÖZÜLDÜ: UIManager yerine kendi eventimizi tetikliyoruz
        OnLoanStateChanged?.Invoke(); 
    }

    public void RepayLoan()
    {
        if (!hasActiveLoan) return;

        // DİKKAT: MoneyManager'daki fonksiyonun adı gold mu, currentGold mu kontrol et
        if (MoneyManager.Instance.gold >= loanAmount)
        {
            MoneyManager.Instance.Spend(loanAmount);
            
            hasActiveLoan = false;
            loanAmount = 0;
            loanDueDay = 0;

            Debug.Log("Borç ödendi! İtibarın güvende.");
            
            // HATA ÇÖZÜLDÜ: UI'a haber ver
            OnLoanStateChanged?.Invoke();
        }
        else
        {
            Debug.Log("Borcu ödeyecek paran yok!");
        }
    }

    void CheckLoanStatus(int currentDay)
    {
        if (!hasActiveLoan) return;

        if (currentDay > loanDueDay)
        {
            Debug.Log("BORÇ GÜNÜ GEÇTİ! İtibar düşürülüyor.");

            ReputationManager.Instance.ChangeReputation(-latePenaltyRep);

            loanDueDay += 3; // 3 gün ek süre
            
            // HATA ÇÖZÜLDÜ: UI'a haber ver (Güncellensin ki yeni tarihi görsün)
            OnLoanStateChanged?.Invoke();
        }
    }
}