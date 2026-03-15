using UnityEngine;
using System;

public class CampSurvivalManager : MonoBehaviour
{
    public static CampSurvivalManager Instance;
    public event Action OnTemperatureChanged;

    [Header("Sıcaklık Değerleri")]
    public int minTemperature = -30;
    public int maxTemperature = 30;
    [Range(-30, 30)] public int currentTemperature = 25; 
    
    [Header("Mevsimsel Soğuma (Günlük)")]
    // AAA Dengelemesi: Soğuklar artık daha sert!
    public int summerColdDrop = 3;  
    public int autumnColdDrop = 8;  
    public int winterColdDrop = 15; // Kışın dondurucu soğuk

    [Header("Ateş Etkisi")]
    // AAA Dengelemesi: Odun artık çok değerli! 5 Odun sadece 5 derece ısıtır.
    public int warmthPerWood = 1; 
    public int maxWoodPerClick = 5; 

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

    void ApplyDailyCold()
    {
        int day = DayManager.Instance.currentDay;
        int dropAmount = summerColdDrop;

        if (day >= 10 && day < 20) dropAmount = autumnColdDrop;
        else if (day >= 20) dropAmount = winterColdDrop;

        currentTemperature -= dropAmount;

        bool autoBurned = false;
        int burnedWoodAmount = 0;

        // Gece donma tehlikesinde otomatik odun yakma
        while (currentTemperature <= 0 && ResourceManager.Instance.wood >= maxWoodPerClick)
        {
            ResourceManager.Instance.SpendWood(maxWoodPerClick);
            currentTemperature += (maxWoodPerClick * warmthPerWood);
            burnedWoodAmount += maxWoodPerClick;
            autoBurned = true;
        }

        if (autoBurned && NotificationManager.Instance != null)
        {
            NotificationManager.Instance.Show($"Gece don vurdu! Askerler hayatta kalmak için depodan {burnedWoodAmount} odun yaktı.", NotificationType.Warning);
        }

        // KESİN ÇÖZÜM: Sıcaklığı ne olursa olsun -30 ile 30 arasına hapset!
        currentTemperature = Mathf.Clamp(currentTemperature, minTemperature, maxTemperature);

        OnTemperatureChanged?.Invoke();
        CheckSurvivalStatus();
    }

    public bool StokeFire() 
    {
        // YENİ MANTIK KİLİDİ: Hava zaten maksimum sıcaklıktaysa boşuna odun yakmayı engelle
        if (currentTemperature >= maxTemperature)
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Hava zaten yeterince sıcak, odunlarını israf etme!", NotificationType.Warning);
            return false;
        }

        if (ResourceManager.Instance.SpendWood(maxWoodPerClick))
        {
            int warmthGained = maxWoodPerClick * warmthPerWood;
            currentTemperature += warmthGained;
            
            // Sıcaklığı tekrar kontrol et ve kilitle
            currentTemperature = Mathf.Clamp(currentTemperature, minTemperature, maxTemperature);
            
            OnTemperatureChanged?.Invoke();
            
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

    void CheckSurvivalStatus()
    {
        if (currentTemperature <= -10) 
        {
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-10); 
            if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(-1);
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("<color=blue>İliklerimize kadar donuyoruz!</color> Moraller çöktü.", NotificationType.Error);
        }
        else if (currentTemperature <= 5) 
        {
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-3); 
        }
    }
}