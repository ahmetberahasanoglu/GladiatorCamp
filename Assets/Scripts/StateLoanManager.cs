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
            NotificationManager.Instance.Show("Zaten ödenmemiş bir borcun var!", NotificationType.Error);
            return;
        }

        if (ReputationManager.Instance.GetReputation() < minRepToBorrow)
        {
            NotificationManager.Instance.Show("Devlet sana güvenmiyor! (İtibar < 50)", NotificationType.Warning);
            return;
        }

        // DİKKAT: MoneyManager'daki fonksiyonun adı AddGold ise burayı düzelt
        MoneyManager.Instance.Add(defaultLoanAmount); 
        
        hasActiveLoan = true;
        loanAmount = defaultLoanAmount;
        loanDueDay = DayManager.Instance.currentDay + paymentPeriod;
        NotificationManager.Instance.Show($"Devletten {loanAmount} Akçe borç alındı. Son ödeme: Gün {loanDueDay}", NotificationType.Info);
        
        // HATA ÇÖZÜLDÜ: UIManager yerine kendi eventimizi tetikliyoruz
        OnLoanStateChanged?.Invoke(); 
    }

    // StateLoanManager.cs içindeki RepayLoan fonksiyonu:

    public void RepayLoan()
    {
        if (!hasActiveLoan) return;

        if (MoneyManager.Instance.gold >= loanAmount)
        {
            MoneyManager.Instance.Spend(loanAmount);
            
      
            // Eğer borcu gününden önce veya gününde ödediysek ödül verelim
            int currentDay = DayManager.Instance.currentDay;
            
            if (currentDay <= loanDueDay) 
            {
                // Örnek: Borç ödenince +10 İtibar kazanılsın
                ReputationManager.Instance.ChangeReputation(10);
                NotificationManager.Instance.Show("Borç zamanında ödendi! Padişah memnun oldu (+10 İtibar).", NotificationType.Success);
            }
            // ----------------------------------------------

            hasActiveLoan = false;
            loanAmount = 0;
            loanDueDay = 0;

            OnLoanStateChanged?.Invoke();
        }
        else
        {
            NotificationManager.Instance.Show("Borcu ödeyecek paran yok!", NotificationType.Warning);
        }
    }

    void CheckLoanStatus(int currentDay)
    {
        if (!hasActiveLoan) return;

        if (currentDay > loanDueDay)
        {
            NotificationManager.Instance.Show("BORÇ GÜNÜ GEÇTİ! İtibar düşürülüyor.", NotificationType.Warning);
            ReputationManager.Instance.ChangeReputation(-latePenaltyRep);

            loanDueDay += 3; // 3 gün ek süre
            
            // HATA ÇÖZÜLDÜ: UI'a haber ver (Güncellensin ki yeni tarihi görsün)
            OnLoanStateChanged?.Invoke();
        }
    }
}