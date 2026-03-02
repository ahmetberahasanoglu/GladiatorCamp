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
    public GameObject featherPoofEffect; // Vurulunca çıkacak tüy/altın efekti
    public AudioSource coinSound; 

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

    // Fareyle üzerine tıklandığında...
    void OnMouseDown()
    {
        // Eğer UI (Menü vs) açıksa tıklamayı yoksay
        if (EventSystem.current.IsPointerOverGameObject()) return;
          
        // 1. Ödülü Ver
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.Add(goldReward);
        }

        // 2. Bildirim Göster
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.Show($"Kuzgunu indirdin! Gagasından {goldReward} akçe düştü.", NotificationType.Success);
        }
         coinSound.Play();
        // 3. Efekt Yarat (Eğer atadıysan)
        if (featherPoofEffect != null)
        {
            GameObject poof = Instantiate(featherPoofEffect, transform.position, Quaternion.identity);
            Destroy(poof, 2f); // Oyunu kastırmasın diye 2 sn sonra sil
        }

        // Ses eklemek istersen buraya bir AudioSource.PlayClipAtPoint yazabilirsin

        // 4. Kuşu Yok Et
        Destroy(gameObject);
    }
}