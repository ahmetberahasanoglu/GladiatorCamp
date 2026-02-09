using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Image fillImage;   // Yeşil olan kısım
    public GameObject canvasObj; // Barın tamamı (Gereksizse kapatmak için)

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    // Can değiştikçe çağıracağımız fonksiyon
    public void UpdateBar(float currentHealth, float maxHealth)
    {
        if (fillImage != null)
        {
            // 0 ile 1 arasında oranla
            float ratio = currentHealth / maxHealth;
            fillImage.fillAmount = ratio;

            // Renk değişimi (Can azaldıkça kırmızıya dönsün - Opsiyonel)
            fillImage.color = Color.Lerp(Color.red, Color.green, ratio);
        }
    }

    void LateUpdate()
    {
        // Bar hep kameraya baksın (Billboard)
        if (cam != null)
        {
            // Canvas'ın parent'ı olan obje (veya bu scriptin olduğu obje) dönmeli
            // Eğer bu script direkt Canvas üzerindeyse:
            transform.rotation = cam.transform.rotation;
        }
    }
}