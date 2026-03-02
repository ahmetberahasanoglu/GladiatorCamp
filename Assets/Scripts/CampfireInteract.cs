using UnityEngine;
using UnityEngine.EventSystems;

public class CampfireInteract : MonoBehaviour
{
    [Header("Ateş Ayarları")]
    public ParticleSystem sparksParticle; 
    public AudioSource flareSound;        

    private bool isSparking = true; // İlk açılışta yanıyor kabul edelim

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        // Eğer zaten çok gür yanıyorsa oyuncu boşuna odun israf etmesin
        if (isSparking)
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Ateş zaten gür yanıyor. Daha fazla harlamaya gerek yok.", NotificationType.Info);
            return;
        }

        // Ateşi harlamayı dene (CampSurvivalManager içinde odun kesilecek)
        if (CampSurvivalManager.Instance != null)
        {
            // Eğer odun yoksa `StokeFire` zaten hata mesajı verecek ve aşağısı çalışmayacak.
            // Ama biz sonucu bilmek için küçük bir numara yapacağız.
            int oldTemp = CampSurvivalManager.Instance.currentTemperature;
            CampSurvivalManager.Instance.StokeFire();
            
            // Sıcaklık arttıysa demek ki odun başarıyla harcanmış
            if (CampSurvivalManager.Instance.currentTemperature > oldTemp)
            {
                FlareRoutine();
            }
        }
    }

    void FlareRoutine()
    {
        // Ateşi canlandır
        sparksParticle.Play();
        isSparking = true;
        if (flareSound != null) flareSound.Play();
    }

    void Start()
    {
        // DayManager'a abone olup, her gün ateşi SÖNDÜRECEĞİZ ki oyuncu ertesi gün tekrar yaksın
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay += DimFire;
        }
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay -= DimFire;
        }
    }

    void DimFire()
    {
        // Gün dönünce ateş görsel olarak cılızlaşsın
        sparksParticle.Stop();
        isSparking = false;
    }
}