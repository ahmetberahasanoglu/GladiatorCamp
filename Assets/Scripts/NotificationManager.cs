using UnityEngine;

// Bildirim türleri (Renk kodlaması için)
public enum NotificationType { Info, Success, Warning, Error }

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("Ayarlar")]
    public GameObject notificationPrefab; // Az önce yazdığımız item'ın prefabı
    public Transform notificationContainer; // Mesajların dizileceği kutu (Content)

    [Header("Renk Temaları")]
    public Color infoColor = Color.gray;       // Normal bilgi (Gri/Mavi)
    public Color successColor = Color.green;   // Para kazanma, asker alma (Yeşil)
    public Color warningColor = new Color(1f, 0.5f, 0f); // Turuncu
    public Color errorColor = Color.red;       // Para yetmedi, itibar düştü (Kırmızı)

    void Awake()
    {
        Instance = this;
    }

    // Dışarıdan çağrılacak ana fonksiyon
    public void Show(string message, NotificationType type = NotificationType.Info)
    {
        // 1. Prefab'ı oluştur
        GameObject newItem = Instantiate(notificationPrefab, notificationContainer);
        
        // 2. Hiyerarşide en alta koy (Böylece Layout grubunda en son eklenen olur)
        // Eğer mesajların yukarıdan aşağı değil, aşağıdan yukarı birikmesini istersen bunu değiştirebiliriz.
        newItem.transform.SetAsLastSibling();

        // 3. Rengini seç
        Color targetColor = infoColor;
        switch (type)
        {
            case NotificationType.Success: targetColor = successColor; break;
            case NotificationType.Warning: targetColor = warningColor; break;
            case NotificationType.Error: targetColor = errorColor; break;
        }

        // 4. İçeriği doldur
        var itemScript = newItem.GetComponent<NotificationItem>();
        if (itemScript != null)
        {
            itemScript.Setup(message, targetColor);
        }
    }
}