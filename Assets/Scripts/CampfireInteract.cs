using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CampfireInteract : MonoBehaviour
{
    [Header("Ateş Ayarları")]
    public ParticleSystem sparksParticle;

    [Header("Sesler")]
    public AudioSource flareSound;          // Ateş yakma sesi (tek seferlik)
    public AudioClip extinguishSound;       // Söndürme sesi (Clip olarak alıyoruz)
    
    [Header("Döngü (Loop) Sesleri")]
    public AudioSource campfireLoopA;       // 1. Crossfade Kanalı
    public AudioSource campfireLoopB;       // 2. Crossfade Kanalı
    public float crossfadeTime = 0.3f;      // Geçiş süresi

    private AudioSource activeLoop;
    private AudioSource nextLoop;
    private Coroutine loopCoroutine;      

    private bool isSparking = true;
    private bool isOnCooldown = false;

    void Start()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay += DimFire;

        if (isSparking)
        {
            if (sparksParticle != null) sparksParticle.Play();
            StartLoop(); 
        }
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay -= DimFire;
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject() || isOnCooldown) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;

        StartCoroutine(ToggleFireRoutine());
    }

    IEnumerator ToggleFireRoutine()
    {
        isOnCooldown = true;

        if (isSparking)
        {
            isSparking = false;

            if (sparksParticle != null) sparksParticle.Stop();

            StopFireLoops(); 

            if (extinguishSound != null && flareSound != null)
                flareSound.PlayOneShot(extinguishSound);

            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Ateşi söndürdün. Kamp soğumaya başlayacak.", NotificationType.Warning);
        }
        else
        {
           
            if (CampSurvivalManager.Instance != null)
            {
                bool isSuccess = CampSurvivalManager.Instance.StokeFire();

                if (isSuccess)
                {
                    isSparking = true;

                    if (sparksParticle != null) sparksParticle.Play();

                    if (flareSound != null) flareSound.Play(); 

                    StartLoop(); 
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        isOnCooldown = false;
    }

    void DimFire()
    {
        isSparking = false;

        if (sparksParticle != null) sparksParticle.Stop();
        
        StopFireLoops(); 
    }



    void StartLoop()
    {
        if (campfireLoopA == null || campfireLoopB == null) return;
        if (loopCoroutine != null) StopCoroutine(loopCoroutine);

        activeLoop = campfireLoopA;
        nextLoop = campfireLoopB;

        activeLoop.volume = 1f;
        nextLoop.volume = 0f;
        
        activeLoop.Play();
        nextLoop.Stop(); 

        loopCoroutine = StartCoroutine(LoopRoutine());
    }

    void StopFireLoops()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
        if (campfireLoopA != null) campfireLoopA.Stop();
        if (campfireLoopB != null) campfireLoopB.Stop();
    }

    IEnumerator LoopRoutine()
    {
        while (isSparking)
        {

            if (activeLoop.clip == null) yield break;

            // Sesin bitimine crossfadeTime kadar süre kalana dek bekle
            float waitTime = activeLoop.clip.length - crossfadeTime;
            if (waitTime > 0) yield return new WaitForSeconds(waitTime);

            // Eğer bekleme sırasında ateş söndürüldüyse döngüden çık
            if (!isSparking) yield break;

            // Sonraki sesi kısık sesle başlat
            nextLoop.volume = 0f;
            nextLoop.Play();

            float t = 0f;
            // Crossfade süresince sesi yavaşça kıs/aç
            while (t < crossfadeTime && isSparking) 
            {
                t += Time.deltaTime;
                float normalized = t / crossfadeTime;

                activeLoop.volume = 1f - normalized;
                nextLoop.volume = normalized;

                yield return null;
            }

            activeLoop.Stop();

            // Swap (Kanalları değiştir)
            AudioSource temp = activeLoop;
            activeLoop = nextLoop;
            nextLoop = temp;
        }
    }
}