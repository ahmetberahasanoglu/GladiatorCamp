using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DummyInteract : MonoBehaviour
{
    [Header("Görsel ve Ses")]
    public ParticleSystem hitParticles;   // Tahta tozları/kıymıklar
    public AudioSource hitSound;          // Kılıç/odun vurma sesi
    private Animator animator;

    private bool isHit = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        // Eğer şu an zaten sallanmıyorsa vur
        if (!isHit) StartCoroutine(WobbleRoutine());
    }

    IEnumerator WobbleRoutine()
    {
        isHit = true; // Kuklayı kilitledik, başka tıklama almayacak

        // Efektleri ve animasyonu oynat
        if (hitParticles != null) hitParticles.Play();
        if (hitSound != null) hitSound.Play();
        if (animator != null) animator.SetTrigger("hit");
        
        // Animasyonun uzunluğu kadar bekle (Örn: 0.5 saniye)
        // Eğer animasyonun daha uzun veya kısaysa bu süreyi ona göre değiştirebilirsin
        yield return new WaitForSeconds(0.2f); 
        
        isHit = false; // Süre bitti, artık tekrar vurulabilir
    }
}