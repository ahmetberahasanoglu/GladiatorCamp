using UnityEngine;
using System.Collections;

public class RavenSpawner : MonoBehaviour
{
    [Header("Kuzgun Ayarları")]
    public GameObject ravenPrefab;
    
    [Header("Zamanlama")]
    public float minSpawnTime = 30f; // En az kaç saniyede bir çıksın
    public float maxSpawnTime = 90f; // En çok kaç saniyede bir çıksın

    [Header("Uçuş Notaları")]
    public Transform[] startPoints; // Kuşun çıkacağı noktalar (Örn: Haritanın solu)
    public Transform[] endPoints;   // Kuşun gideceği noktalar (Örn: Haritanın sağı)

    void Start()
    {
        if (startPoints.Length == 0 || endPoints.Length == 0)
        {
            Debug.LogWarning("RavenSpawner için Start veya End noktaları eksik!");
            return;
        }

        // Üretim döngüsünü başlat
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Rastgele bir bekleme süresi seç
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            SpawnRaven();
        }
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