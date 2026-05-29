using UnityEngine;
using System;

public class StateLoanManager : MonoBehaviour
{
    public static StateLoanManager Instance;

    [Header("Karz-ı Hasen (Faizsiz Borç) Ayarları")]
    public bool hasActiveLoan = false; 
    public int loanAmount = 0;         
    public int loanDueDay = 0;         
    public int paymentPeriod = 7;      
    
    [Header("Cezalar ve Kısıtlamalar")]
    public int latePenaltyRep = 15;    
    public int cooldownDays = 3; // YENİ: Borç ödendikten sonra kaç gün beklenmeli?

    public int dayLoanTaken = 0;
    public int nextAvailableLoanDay = 0; // YENİ: Bir sonraki borç alınabilecek gün

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

    public int GetMaxAvailableLoan()
    {
        int rep = ReputationManager.Instance.GetReputation();
        
        if (rep >= 80) return 3000;
        if (rep >= 60) return 2000;
        if (rep >= 40) return 1000;
        
        return 0; 
    }

   public void RequestLoan(int tierAmount)
    {
        if (hasActiveLoan)
        {
            NotificationManager.Instance.Show("Vakıf sandığından alınmış, ödenmemiş bir emanetin var!", NotificationType.Error);
            return;
        }

        // --- BEKLEME SÜRESİ KONTROLÜ ---
        int currentDay = DayManager.Instance.currentDay;
        if (currentDay < nextAvailableLoanDay)
        {
            int remainingDays = nextAvailableLoanDay - currentDay;
            NotificationManager.Instance.Show($"Vakıf sandığı defterleri düzenliyor. Yeni bir emanet için {remainingDays} gün sonra gel.", NotificationType.Warning);
            return;
        }

        // --- HATA BURADAYDI ÇÖZÜLDÜ ---
        // Artık "Maksimum ne alabilir" diye sormuyoruz. "İstediği miktara (tierAmount) itibarı yetiyor mu?" diye soruyoruz.
        int requiredRep = 40;
        if (tierAmount == 2000) requiredRep = 60;
        else if (tierAmount == 3000) requiredRep = 80;

        if (ReputationManager.Instance.GetReputation() < requiredRep)
        {
            NotificationManager.Instance.Show($"Ahiler sana o kadar büyük bir meblağ güvenmiyor! (İtibar {requiredRep}+ olmalı)", NotificationType.Warning);
            return;
        }

        // availableLoan yerine doğrudan UI'dan gelen tierAmount'u kullanıyoruz!
        MoneyManager.Instance.Add(tierAmount); 
        
        hasActiveLoan = true;
        loanAmount = tierAmount; // Borcumuz da tam olarak istediğimiz miktar kadar oluyor
        
        loanDueDay = currentDay + paymentPeriod;
        dayLoanTaken = currentDay; 

        NotificationManager.Instance.Show($"İtibarına güvenilerek Vakıf sandığından {tierAmount} Akçe emanet alındı.", NotificationType.Info);
        
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
                ReputationManager.Instance.ChangeReputation(10);
                if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(1); 

                if (currentDay > dayLoanTaken)
                    NotificationManager.Instance.Show("Vakfın emaneti zamanında ödendi! (+10 İtibar, +1 Nasip)", NotificationType.Success);
                else
                    NotificationManager.Instance.Show("Emaneti erkenden teslim ettin. Ahiler sadakatini takdir ediyor!", NotificationType.Success);
            }

            // --- YENİ: BEKLEME SÜRESİNİ BAŞLAT ---
            // Borç ödendiği an, şu anki güne 3 gün ekleyip "bir sonraki alınabilecek tarihi" belirliyoruz.
            nextAvailableLoanDay = currentDay + cooldownDays;

            hasActiveLoan = false;
            loanAmount = 0;
            loanDueDay = 0;
            dayLoanTaken = 0;

            OnLoanStateChanged?.Invoke();
        }
        else
        {
            NotificationManager.Instance.Show("Emaneti tamamen teslim edecek kadar Akçen yok!", NotificationType.Warning);
        }
    }


   void CheckLoanStatus(int currentDay)
    {
        if (!hasActiveLoan) return;

        // Vade Günü GEÇTİYSE (Örn: 8 günlük sürenin 9. gününe girildiyse)
        if (currentDay > loanDueDay)
        {
            // 1. Durum: Kasada yeterli para varsa sessizce tahsil et
            if (MoneyManager.Instance.gold >= loanAmount)
            {
                MoneyManager.Instance.Spend(loanAmount);
                
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show($"Vade doldu! Vakıf {loanAmount} Akçeyi kasandan otomatik tahsil etti.", NotificationType.Warning);

                // Normal bekleme süresi (3 gün)
                nextAvailableLoanDay = currentDay + cooldownDays;
            }
            // 2. Durum: Para yetmiyorsa tüm parasını haczet + Ağır İtibar/Nasip Cezası kes!
            else
            {
                int missingAmount = loanAmount - MoneyManager.Instance.gold;
                MoneyManager.Instance.Spend(MoneyManager.Instance.gold); // Kasayı sıfırla
                
                // Her 100 akçe eksik için 1 itibar düş (Minimum 5 ceza)
                int repPenalty = missingAmount / 100; 
                if (repPenalty < 5) repPenalty = 5; 

                ReputationManager.Instance.ChangeReputation(-repPenalty);

                if (NasipManager.Instance != null) 
                    NasipManager.Instance.SpendNasip(2);

                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show($"Vakfa borcunu ödeyemedin! Tüm paran haczedildi ve {repPenalty} İtibar kaybettin.", NotificationType.Error);

                // Haciz cezası olarak Cooldown süresini normalden uzun yap (5 gün ceza)
                nextAvailableLoanDay = currentDay + 5; 
            }

            // Borcu sıfırla (Tahsil edildi veya haczedildi)
            hasActiveLoan = false;
            loanAmount = 0;
            dayLoanTaken = 0;
            loanDueDay = 0;
            
            OnLoanStateChanged?.Invoke();
        }
    }
}