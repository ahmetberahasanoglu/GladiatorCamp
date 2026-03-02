using UnityEngine;
using System;

public class CampSurvivalManager : MonoBehaviour
{
    public static CampSurvivalManager Instance;
    public event Action OnTemperatureChanged;

    [Header("Sıcaklık Değerleri")]
    [Range(0, 100)] public int currentTemperature = 100; // 100 en sıcak, 0 donma noktası
    
    [Header("Mevsimsel Soğuma (Günlük)")]
    public int summerColdDrop = 5;  // Yazın geceleri az soğur
    public int autumnColdDrop = 15; // Sonbahar serttir
    public int winterColdDrop = 30; // Kışın ateş yakmamak ölümcüldür

    [Header("Ateş Etkisi")]
    public int warmthPerWood = 10; // 1 birim odun kaç sıcaklık verir?
    public int maxWoodPerClick = 5; // Ateşe tek tıklamada kaç odun atılır?

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay += ApplyDailyCold;
        }
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay -= ApplyDailyCold;
        }
    }

    // Her yeni günde havayı soğut ve sonuçlarına katlan
    void ApplyDailyCold()
    {
        int day = DayManager.Instance.currentDay;
        int dropAmount = summerColdDrop;

        if (day >= 50 && day < 80) dropAmount = autumnColdDrop;
        else if (day >= 80) dropAmount = winterColdDrop;

        currentTemperature -= dropAmount;

        // --- KRİTİK EKLENTİ: ZEKİ ASKERLER (TIME-SKIP KORUMASI) ---
        // Eğer sıcaklık kritik seviyeye (Örn: 30) düştüyse ve depoda odun varsa,
        // oyuncu tıklamasa bile askerler donmamak için depodan gizlice odun yakar!
        bool autoBurned = false;
        int burnedWoodAmount = 0;

        while (currentTemperature <= 30 && ResourceManager.Instance.wood >= maxWoodPerClick)
        {
            ResourceManager.Instance.SpendWood(maxWoodPerClick);
            currentTemperature += (maxWoodPerClick * warmthPerWood);
            burnedWoodAmount += maxWoodPerClick;
            autoBurned = true;
        }

        // Eğer zaman atlamasında odun yakıldıysa oyuncuya rapor ver
        if (autoBurned && NotificationManager.Instance != null)
        {
            NotificationManager.Instance.Show($"Sen yokken kamp çok soğudu. Askerler donmamak için depodan {burnedWoodAmount} odun yaktı.", NotificationType.Warning);
        }
        // -----------------------------------------------------------

        if (currentTemperature < 0) currentTemperature = 0; 

        OnTemperatureChanged?.Invoke();
        CheckSurvivalStatus();
    }

    // Odun yakarak kampı ısıt
    // Odun yakarak kampı ısıt ve anlık bonus ver
   // Odun yakarak kampı ısıt (CampSurvivalManager.cs içindeki StokeFire fonksiyonu)
    public bool StokeFire() 
    {
        if (ResourceManager.Instance.SpendWood(maxWoodPerClick))
        {
            // Sıcaklığı artır
            int warmthGained = maxWoodPerClick * warmthPerWood;
            currentTemperature += warmthGained;
            if (currentTemperature > 100) currentTemperature = 100; 
            
            OnTemperatureChanged?.Invoke();
            
            // Sadece ufak bir moral ver (Can basmayı CampRestSystem'e bıraktık)
            if (CampMoraleManager.Instance != null) 
                CampMoraleManager.Instance.ChangeMorale(1); 
            
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show($"Ateş harlandı! Kamp ısındı (+{warmthGained}°C).", NotificationType.Success);
            
            return true;
        }
        else
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Ateşe atacak yeterli Odun yok!", NotificationType.Error);
            return false;
        }
    }
    // Sıcaklık çok düşerse ne olacak? (Moral düşüşü / Hastalık)
    void CheckSurvivalStatus()
    {
        if (currentTemperature <= 20) // Donmak üzereler!
        {
            if (CampMoraleManager.Instance != null)
            {
                CampMoraleManager.Instance.ChangeMorale(-10); // Ağır moral kaybı
            }
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("<color=blue>Kamp donuyor!</color> Askerlerin morali dibe vurdu.", NotificationType.Error);
        }
        else if (currentTemperature <= 50) // Üşüyorlar
        {
            if (CampMoraleManager.Instance != null)
            {
                CampMoraleManager.Instance.ChangeMorale(-3); 
            }
        }
    }
}