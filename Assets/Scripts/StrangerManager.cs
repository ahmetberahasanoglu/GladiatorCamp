using UnityEngine;

public class StrangerManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject strangerPrefab; 
    public Transform spawnAndExitPoint; 
    public Transform campWaitPoint;     

    [Header("Zamanlama (Gerçek Zaman)")]
    public float minSpawnTime = 60f; 
    public float maxSpawnTime = 120f; 

    private GameObject currentStranger;
    
    // YENİ: Süreyi artık Update içinde biz kontrol ediyoruz
    private float timer = 0f;
    private float nextSpawnTime = 0f;

    void Start()
    {
        SetNewSpawnTime();
    }

    void Update()
    {
        // --- 1. KİLİT: OYUN DURUMU KONTROLÜ ---
        // Eğer savaştaysak SÜREYİ DONDUR
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle) return;
        
        // Eğer harita açıksa SÜREYİ DONDUR
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;

        // --- 2. KİLİT: ZATEN BİRİ VAR MI? ---
        // Eğer kampta zaten bir yabancı varsa yenisi için süre sayma
        if (currentStranger != null) return;

        // --- 3. SÜREYİ İŞLET ---
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnStranger();
            SetNewSpawnTime();
            timer = 0f; // Süreyi sıfırla
        }
    }

    void SetNewSpawnTime()
    {
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
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