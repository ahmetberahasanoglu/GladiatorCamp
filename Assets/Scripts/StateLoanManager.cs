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
    public int repaymentAmount = 2400; 
    public int latePenaltyRep = 15;    


    private int dayLoanTaken = 0;

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

        MoneyManager.Instance.Add(defaultLoanAmount); 
        
        hasActiveLoan = true;
        loanAmount = repaymentAmount; 
        
        loanDueDay = DayManager.Instance.currentDay + paymentPeriod;
        dayLoanTaken = DayManager.Instance.currentDay; 

        NotificationManager.Instance.Show($"Devletten {defaultLoanAmount} Akçe alındı. Geri Ödeme: {loanAmount} Akçe", NotificationType.Info);
        
        OnLoanStateChanged?.Invoke(); 
    }

    public void RepayLoan()
    {
        if (!hasActiveLoan) return;

        if (MoneyManager.Instance.gold >= loanAmount)
        {
            MoneyManager.Instance.Spend(loanAmount);
            
            int currentDay = DayManager.Instance.currentDay;

            if (currentDay <= loanDueDay) 
            {

                if (currentDay > dayLoanTaken)
                {

                    ReputationManager.Instance.ChangeReputation(10);

                    if (NasipManager.Instance != null)
                    {
                        NasipManager.Instance.AddNasip(1); 
                    }

                    NotificationManager.Instance.Show("Borç zamanında ödendi! (+10 İtibar, +1 Nasip)", NotificationType.Success);
                }
                else
                {
                    NotificationManager.Instance.Show("Borç erkenden kapatıldı. Devlet masrafı kesti.", NotificationType.Info);
                }
            }

            // Borcu sıfırla
            hasActiveLoan = false;
            loanAmount = 0;
            loanDueDay = 0;
            dayLoanTaken = 0;

            OnLoanStateChanged?.Invoke();
        }
        else
        {
            NotificationManager.Instance.Show("Borcu tamamen kapatacak paran yok!", NotificationType.Warning);
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