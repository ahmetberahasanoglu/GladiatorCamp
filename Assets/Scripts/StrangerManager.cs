using UnityEngine;
using System.Collections;

public class StrangerManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject strangerPrefab; // Gelen kişinin modeli
    public Transform spawnAndExitPoint; // Kampın kapısı (Giriş/Çıkış)
    public Transform campWaitPoint;     // Gelip ortada duracağı nokta

    [Header("Zamanlama (Gerçek Zaman)")]
    public float minSpawnTime = 60f; // En az 1 dakikada bir
    public float maxSpawnTime = 120f; // En çok 2 dakikada bir

    private GameObject currentStranger;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // Sadece savaştayken değil, kamptayken ve içeride başka yabancı yokken gelsin
            if (currentStranger == null && BattleManager.Instance != null && BattleManager.Instance.state == BattleState.Idle)
            {
                SpawnStranger();
            }
        }
    }

    void SpawnStranger()
    {
        currentStranger = Instantiate(strangerPrefab, spawnAndExitPoint.position, Quaternion.identity);
        
        WanderingStranger strangerScript = currentStranger.GetComponent<WanderingStranger>();
        
        // 0 (Tüccar) ile 1 (Asker) arasında rastgele birini seç
        int randomType = Random.Range(0, 2); 
        
        strangerScript.Setup(campWaitPoint, spawnAndExitPoint, randomType);
        
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show("Kampa kukuletalı bir yabancı geldi...", NotificationType.Info);
    }
}