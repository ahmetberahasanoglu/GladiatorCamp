using UnityEngine;
using System.Collections.Generic;
using System; // Action için

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    public int currentDay = 1;

    [Header("Olay Sistemi")]
    public List<GameEvent> possibleEvents; // Olabilecek tüm olaylar
    [Range(0, 100)] public int eventChance = 10; // Her gün % kaç ihtimalle olay olsun?

    // Diğer sistemlerin (SupplyManager, RecruitManager) dinlediği event
    public event Action OnNewDay; 
    
    // UI'ın dinlediği eventler (Parametreli)
    public event Action<int> OnDayChanged;
    public event Action<GameEvent> OnEventTriggered;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Başlangıçta günü bildir
        OnDayChanged?.Invoke(currentDay);
    }

    // "Günü Bitir" butonuna basınca bu çalışacak
    public void NextDay()
    {
        currentDay++;
        
        // 1. Genel sisteme haber ver (Yemek yensin, askerler iyileşsin vs.)
        OnNewDay?.Invoke();

        // 2. UI'a haber ver (Gün sayısını güncelle)
        OnDayChanged?.Invoke(currentDay);

        Debug.Log($"Gün bitti. Yeni Gün: {currentDay}");

        // 3. ZAR AT: Bugün bir olay olacak mı?
        CheckForRandomEvent();
        
        // 4. ÖZEL GÜN KONTROLÜ (Ulufe)
        CheckForUlufe();
    }

    void CheckForRandomEvent()
    {
        if (possibleEvents.Count == 0) return;

        // 100 üzerinden zar at, şanstan düşükse olay olur
        if (UnityEngine.Random.Range(0, 100) < eventChance)
        {
            // Rastgele bir olay seç
            GameEvent randomEvent = possibleEvents[UnityEngine.Random.Range(0, possibleEvents.Count)];
            TriggerEvent(randomEvent);
        }
    }

    void TriggerEvent(GameEvent e)
    {
        // Etkileri uygula
        if (e.moneyChange != 0) MoneyManager.Instance.Add(e.moneyChange); // Eksi gelirse düşer
        if (e.foodChange != 0) SupplyManager.Instance.BuyFood(e.foodChange); // (Mevcut fonksiyonu kullanıyoruz, gerekirse ModifyFood yazarız)
        
        // Moral etkisi için askerleri gezmemiz lazım
        if (e.moraleChange != 0)
        {
            foreach(var s in FindObjectsOfType<Gladiator>())
            {
                if(e.moraleChange < 0) s.DecreaseMorale(Mathf.Abs(e.moraleChange));
                else s.data.morale += e.moraleChange; // Artırma fonksiyonun yoksa direkt data
            }
        }

        // UI'a haber ver (Popup aç)
        OnEventTriggered?.Invoke(e);
        Debug.Log($"OLAY VAR: {e.title}");
    }

    void CheckForUlufe()
    {
        // Osmanlı'da Ulufe 3 ayda bir (yaklaşık 90 gün) dağıtılırdı.
        if (currentDay % 90 == 0)
        {
            Debug.Log("ULUFE GÜNÜ GELDİ!");
            // Buraya özel bir Ulufe Eventi tetikletebilirsin
        }
    }
}