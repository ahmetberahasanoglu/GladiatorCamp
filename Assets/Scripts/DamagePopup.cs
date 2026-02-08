using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public CanvasGroup canvasGroup;
    
    private float moveSpeed = 1.5f;
    private float disappearSpeed = 2f;
    private float lifeTime = 1f; // Ne kadar ekranda kalacak

    public void Setup(int damageAmount, bool isCritical)
    {
        textMesh.text = damageAmount.ToString();

        if (isCritical)
        {
            textMesh.fontSize = 8; // Kritikse büyük
            textMesh.color = Color.yellow; // ve sarı
        }
        else
        {
            textMesh.fontSize = 5; // Normalse küçük
            textMesh.color = Color.white; // ve beyaz
        }

        // Her seferinde görünürlüğünü sıfırla
        canvasGroup.alpha = 1;
        
        // Hareketi başlat
        StartCoroutine(AnimateRoutine());
    }

    IEnumerator AnimateRoutine()
    {
        float timer = 0;

        // İlk yarıda sadece yukarı çıksın
        while (timer < lifeTime * 0.5f)
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        // İkinci yarıda hem yukarı çıksın hem kaybolsun
        while (timer < lifeTime)
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
            canvasGroup.alpha -= disappearSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        // İşimiz bitti, objeyi kapat (Havuza geri dönsün)
        gameObject.SetActive(false);
    }

    // Yazının hep kameraya bakması için (Billboard efekti)
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}