using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NotificationItem : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TextMeshProUGUI messageText;
    public Image background; // Rengini değiştirmek istersek (Hata kırmızı, Para yeşil vb.)
    public CanvasGroup canvasGroup; // Saydamlık (Fade out) efekti için şart

    [Header("Ayarlar")]
    public float displayDuration = 3f; // Ekranda kalma süresi
    public float fadeDuration = 0.5f;  // Silinme süresi

    public void Setup(string message, Color typeColor)
    {
        messageText.text = message;
        
        // Arkaplan rengini ayarla ama biraz saydam yap ki şık dursun
        if (background != null)
        {
            background.color = new Color(typeColor.r, typeColor.g, typeColor.b, 0.85f);
        }

        // Animasyonu başlat
        StartCoroutine(LifeCycle());
    }

    IEnumerator LifeCycle()
    {
        // 1. Başlangıçta görünür yap (Fade In - Opsiyonel, şimdilik direkt açıyoruz)
        canvasGroup.alpha = 1;

        // 2. Belirlenen süre kadar bekle
        yield return new WaitForSeconds(displayDuration);

        // 3. Yavaşça yok ol (Fade Out)
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // Alpha değerini 1'den 0'a düşür
            canvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }

        // 4. Nesneyi tamamen yok et
        Destroy(gameObject);
    }
}