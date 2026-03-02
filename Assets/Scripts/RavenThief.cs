using UnityEngine;
using UnityEngine.EventSystems;

public class RavenThief : MonoBehaviour
{
    [Header("Uçuş Ayarları")]
    public float speed = 5f;
    private Vector3 destination;

    [Header("Ödül")]
    public int goldReward = 20;

    [Header("Efektler")]
public GameObject featherPoofEffect;
public AudioClip coinClip; // AudioSource yerine AudioClip



    // Spawner bu kuşu yarattığında hedefini belirlemek için bu fonksiyonu çağıracak
    public void SetupFlight(Vector3 startPoint, Vector3 endPoint)
    {
        transform.position = startPoint;
        destination = endPoint;
        
        // Kuşun yönünü hedefe doğru çevir
        transform.LookAt(destination);
    }

    void Update()
    {
        // Hedefe doğru sürekli uç
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        // Hedefe vardıysa (Ekrandan çıktıysa) kendini yok et
        if (Vector3.Distance(transform.position, destination) < 0.5f)
        {
            Destroy(gameObject);
        }
    }

    
    void OnMouseDown()
{
    if (EventSystem.current.IsPointerOverGameObject()) return;

    if (MoneyManager.Instance != null)
    {
        MoneyManager.Instance.Add(goldReward);
    }

    if (NotificationManager.Instance != null)
    {
        NotificationManager.Instance.Show(
            $"Kuzgunu indirdin! Gagasından {goldReward} akçe düştü.", 
            NotificationType.Success
        );
    }

    // 🔊 SESİ OBJEYİ YOK ETMEDEN BAĞIMSIZ ÇAL
    if (coinClip != null)
    {
        AudioSource.PlayClipAtPoint(coinClip, transform.position);
    }

    if (featherPoofEffect != null)
    {
        GameObject poof = Instantiate(featherPoofEffect, transform.position, Quaternion.identity);
        Destroy(poof, 2f);
    }

    Destroy(gameObject);
}
}