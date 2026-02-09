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

   // type: 0 = Normal, 1 = Kritik, 2 = Heal
    public void Setup(int amount, int type) 
    {
        textMesh.text = amount.ToString();

        if (type == 2) // HEAL
        {
            textMesh.text = "+" + amount; // Başına artı koy
            textMesh.fontSize = 7;
            textMesh.color = Color.green;
        }
        else if (type == 1) // CRITICAL
        {
            textMesh.fontSize = 8;
            textMesh.color = Color.yellow;
        }
        else // NORMAL
        {
            textMesh.fontSize = 5;
            textMesh.color = Color.white;
        }

        canvasGroup.alpha = 1;
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