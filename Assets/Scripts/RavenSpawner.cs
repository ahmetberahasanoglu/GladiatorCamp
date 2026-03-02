using UnityEngine;

public class RavenSpawner : MonoBehaviour
{
    [Header("Kuzgun Ayarları")]
    public GameObject ravenPrefab;
    
    [Header("Zamanlama")]
    public float minSpawnTime = 30f; // En az kaç saniyede bir çıksın
    public float maxSpawnTime = 90f; // En çok kaç saniyede bir çıksın

    [Header("Uçuş Noktaları")]
    public Transform[] startPoints; // Kuşun çıkacağı noktalar (Örn: Haritanın solu)
    public Transform[] endPoints;   // Kuşun gideceği noktalar (Örn: Haritanın sağı)

    // --- YENİ EKLENEN KISIM: Kendi zamanlayıcımız ---
    private float timer = 0f;
    private float nextSpawnTime = 0f;

    void Start()
    {
        if (startPoints.Length == 0 || endPoints.Length == 0)
        {
            Debug.LogWarning("RavenSpawner için Start veya End noktaları eksik!");
            return;
        }

        // İlk doğma süresini belirle
        SetNewSpawnTime();
    }

    void Update()
    {
        // --- 1. ZAMAN DONDURMA KİLİTLERİ (OPTIMİZASYON) ---
        // Savaş açıksa veya harita açıksa süreyi dondur! Arkada boşuna kuş üretilmesin.
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;
        // --------------------------------------------------

        // Süreyi işlet
        timer += Time.deltaTime;

        // Hedef süreye ulaşıldıysa kuşu üret
        if (timer >= nextSpawnTime)
        {
            SpawnRaven();
            SetNewSpawnTime();
            timer = 0f; // Süreyi sıfırla
        }
    }

    void SetNewSpawnTime()
    {
        // Bir sonraki kuşun ne zaman geleceğini rastgele belirle
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }

    void SpawnRaven()
    {
        if (ravenPrefab == null) return;

        // Rastgele bir başlangıç ve bitiş noktası seç
        Transform randomStart = startPoints[Random.Range(0, startPoints.Length)];
        Transform randomEnd = endPoints[Random.Range(0, endPoints.Length)];

        // Kuşu yarat
        GameObject newRaven = Instantiate(ravenPrefab, randomStart.position, Quaternion.identity);
        
        // Kuşun içindeki scripti bul ve rotasını ver
        RavenThief thiefScript = newRaven.GetComponent<RavenThief>();
        if (thiefScript != null)
        {
            thiefScript.SetupFlight(randomStart.position, randomEnd.position);
        }
    }
}