using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CampfireInteract : MonoBehaviour
{
    [Header("Ateş Ayarları")]
    public ParticleSystem sparksParticle; 
    public AudioSource flareSound;        
    
    // YENİ: İstersen ateşi söndürürken "tıss" diye çıkan bir ses ekleyebilirsin
   public AudioClip extinguishSound; 

    private bool isSparking = true; // Oyun başladığında ateş yanıyor kabul ediyoruz
    private bool isOnCooldown = false; // Spam tıklamayı engellemek için

    void Start()
    {
        // Gün dönümünde ateşi söndüren event
        if (DayManager.Instance != null) DayManager.Instance.OnNewDay += DimFire;

        // Oyun başlarken ateşin görselini durumuna göre ayarla
        if (isSparking && sparksParticle != null) sparksParticle.Play();
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null) DayManager.Instance.OnNewDay -= DimFire;
    }

    void OnMouseDown()
    {
        // UI'a tıklanıyorsa veya tıklama bekleme süresindeyse (cooldown) işlem yapma
        if (EventSystem.current.IsPointerOverGameObject() || isOnCooldown) return;

        StartCoroutine(ToggleFireRoutine());
    }

    IEnumerator ToggleFireRoutine()
    {
        isOnCooldown = true;

        if (isSparking)
        {
            // --- ATEŞ YANIYORDU, OYUNCU TIKLAYIP SÖNDÜRDÜ ---
            isSparking = false;
            
            if (sparksParticle != null) sparksParticle.Stop();
            if (extinguishSound != null) AudioSource.PlayClipAtPoint(extinguishSound, transform.position);

            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Ateşi söndürdün. Kamp soğumaya başlayacak.", NotificationType.Warning);
        }
        else
        {
            // --- ATEŞ SÖNÜKTÜ, OYUNCU YAKMAYA ÇALIŞIYOR ---
            if (CampSurvivalManager.Instance != null)
            {
                // StokeFire fonksiyonu zaten odun harcama, sıcaklık artırma ve moral verme işini yapıyor
                bool isSuccess = CampSurvivalManager.Instance.StokeFire(); 
                
                if (isSuccess)
                {
                    isSparking = true;
                    if (sparksParticle != null) sparksParticle.Play();
                    if (flareSound != null) flareSound.Play();
                }
            }
        }

        // Tıklamalar arasına yarım saniyelik çok kısa bir bekleme koyuyoruz ki bug oluşmasın
        yield return new WaitForSeconds(0.5f); 
        isOnCooldown = false;
    }

    void DimFire()
    {
        if (sparksParticle != null)
        {
            sparksParticle.Stop();
            isSparking = false;
        }
    }
}