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
    public AudioClip coinClip;

    public void SetupFlight(Vector3 startPoint, Vector3 endPoint)
    {
        transform.position = startPoint;
        destination = endPoint;
        transform.LookAt(destination);
    }

    void Update()
    {
        // --- 1. ZAMAN DONDURMA KİLİTLERİ ---
        // Savaş açıksa veya harita açıksa kuş havada donup beklesin (uçmasın)
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;
        // -----------------------------------

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
        // Eğer UI (Harita vs) arkasındaysa tıklamayı engelle
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        // --- 2. TIKLAMA KİLİDİ ---
        // Harita açıkken veya savaştayken arkadaki kuşa yanlışlıkla tıklanmasını engelle
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;

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