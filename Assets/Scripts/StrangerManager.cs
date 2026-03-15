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
        
        int typeToSpawn = 0; 
        int currentNasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
        int maxNasip = NasipManager.Instance != null ? NasipManager.Instance.maxNasip : 6;

        // (Nasip Full ise %25 ihtimalle gelir)
        if (currentNasip >= maxNasip && Random.Range(0, 100) < 25)
        {
            typeToSpawn = 5; 
        }
        else
        {
            typeToSpawn = Random.Range(0, 5); 
        }

        strangerScript.Setup(campWaitPoint, spawnAndExitPoint, typeToSpawn);
        
        if (NotificationManager.Instance != null)
        {
            if (typeToSpawn == 5)
                NotificationManager.Instance.Show("<color=yellow>Kampa nur yüzlü bir ihtiyar geldi...</color>", NotificationType.Info);
            else
                NotificationManager.Instance.Show("Kampa kukuletalı bir yabancı geldi...", NotificationType.Info);
        }
    }
}