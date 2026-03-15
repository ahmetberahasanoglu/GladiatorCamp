using UnityEngine;

public class StrangerManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject strangerPrefab; 
    public Transform spawnAndExitPoint; 
    public Transform campWaitPoint;     

    [Header("Oyun İçi Gün Ayarları (YENİ)")]
    [Tooltip("Yabancıların kampa gelmeye başlayacağı ilk gün (Tutorial sonrasına ayarla)")]
    public int startingDayForStrangers = 3; 
    [Tooltip("Belirlenen günden sonra her gün kampa gelme ihtimali (% olarak)")]
    [Range(0, 100)] public int dailySpawnChance = 25; 

    private GameObject currentStranger;

    void Start()
    {
        // Gün atlama eventine abone ol
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay += CheckAndSpawnStranger;
        }
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay -= CheckAndSpawnStranger;
        }
    }

    // UPDATE FONKSİYONU TAMAMEN SİLİNDİ!
    // Artık sadece gün değiştiğinde 1 kere çalışacak. Çok daha performanslı.
    void CheckAndSpawnStranger()
    {
        int currentDay = DayManager.Instance.currentDay;

        // 1. Kilit: Tutorial bitmediyse veya gün çok erkense GELDİREMEZ
        if (currentDay < startingDayForStrangers) return;

        // 2. Kilit: Zaten kampta bekleyen biri varsa YENİSİ GELEMEZ
        if (currentStranger != null) return;

        // 3. Zar at: Bugün gelecek mi?
        int roll = Random.Range(0, 100);
        if (roll < dailySpawnChance)
        {
            SpawnStranger();
        }
    }

    void SpawnStranger()
    {
        currentStranger = Instantiate(strangerPrefab, spawnAndExitPoint.position, Quaternion.identity);
        WanderingStranger strangerScript = currentStranger.GetComponent<WanderingStranger>();
        
        int randomType = Random.Range(0, 2); 
        strangerScript.Setup(campWaitPoint, spawnAndExitPoint, randomType);
        
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show("Kampa kukuletalı bir yabancı geldi...", NotificationType.Info);
    }
}