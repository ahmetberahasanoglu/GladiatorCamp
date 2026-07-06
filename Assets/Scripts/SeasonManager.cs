using UnityEngine;
using System.Collections;

public class SeasonManager : MonoBehaviour
{
    public static SeasonManager Instance;

    [Header("Işık Ayarları (Directional Light)")]
    public Light sunLight;
    
    [Header("1-5 Gün: Yaz / Erken Sonbahar")]
    public Color summerColor = new Color(1f, 0.95f, 0.8f); 
    public float summerIntensity = 1.2f;
    public ParticleSystem summerParticle; 

    [Header("5-10 Gün: Geç Sonbahar")]
    public Color autumnColor = new Color(1f, 0.7f, 0.4f); 
    public float autumnIntensity = 0.9f;
    public ParticleSystem autumnLeavesParticle; 

    [Header("10+ Gün: Kışın Ayak Sesleri")]
    public Color winterColor = new Color(0.7f, 0.8f, 0.9f); 
    public float winterIntensity = 0.6f;
    public ParticleSystem snowParticle; 

    [Header("Ses Efektleri")]
    public AudioSource windAudioSource;
    public AudioClip summerWindClip;
    public AudioClip autumnWindClip;
    public AudioClip winterBlizzardClip;

    [Header("Mevsim Durumu")]
    public int currentPhase = 0; // 1: Yaz, 2: Sonbahar, 3: Kış

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay += CheckSeason;
            UpdateAtmosphere(DayManager.Instance.currentDay); 
        }
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay -= CheckSeason;
        }
    }

    void CheckSeason()
    {
        if (DayManager.Instance != null)
        {
            UpdateAtmosphere(DayManager.Instance.currentDay);
        }
    }

    /// <summary>
    /// Kampa geri dönüşlerde BattleManager tarafından çağrılır. 
    /// Haritadayken biriken donmuş parçacıkları temizler ve güncel mevsimi sıfırdan pürüzsüz başlatır.
    /// </summary>
    public void ResetAndCleanParticles(bool isWinter)
    {
        StopAllWeatherEffects();
        ApplyPhaseVisualsAndAudio();
        
        Debug.Log($"[Mevsim Motoru] Kampa geri dönüldü. Faz {currentPhase} parçacıkları ve ortam sesleri sıfırdan tertemiz başlatıldı.");
    }

    public void UpdateAtmosphere(int day)
    {
        if (sunLight == null) return;

        int targetPhase = 1;
        if (day >= 5 && day < 10) targetPhase = 2;
        else if (day >= 10) targetPhase = 3;

        if (currentPhase != targetPhase)
        {
            currentPhase = targetPhase;
            
            Color targetColor = summerColor;
            float targetIntensity = summerIntensity;
            float fogDensity = 0.006f;

            if (currentPhase == 2) { targetColor = autumnColor; targetIntensity = autumnIntensity; fogDensity = 0.01f; }
            else if (currentPhase == 3) { targetColor = winterColor; targetIntensity = winterIntensity; fogDensity = 0.02f; }

            // Artık bu coroutine aşağıda var olduğu için hata çözüldü!
            StartCoroutine(TransitionLight(targetColor, targetIntensity, currentPhase == 2 ? 3f : (currentPhase == 3 ? 4f : 2f)));
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogColor = targetColor;

            if (AudioManager.Instance != null && !AudioManager.Instance.IsAtCamp)
            {
                StopAllWeatherEffects();
                Debug.Log($"[Mevsim Motoru] Haritadayken Faz {currentPhase} düzeyine geçildi. Efektler kampa dönüş için askıya aldı.");
                return; 
            }

            TriggerPhaseNotifications();
            ApplyPhaseVisualsAndAudio();
        }
    }

    private void StopAllWeatherEffects()
    {
        if (summerParticle != null) { summerParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); summerParticle.Clear(); }
        if (autumnLeavesParticle != null) { autumnLeavesParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); autumnLeavesParticle.Clear(); }
        if (snowParticle != null) { snowParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); snowParticle.Clear(); }
        if (windAudioSource != null) { windAudioSource.Stop(); }
    }

    private void ApplyPhaseVisualsAndAudio()
    {
        if (currentPhase == 1)
        {
            if (summerParticle != null && !summerParticle.isPlaying) summerParticle.Play();
            if (windAudioSource != null && summerWindClip != null) { windAudioSource.clip = summerWindClip; windAudioSource.Play(); }
        }
        else if (currentPhase == 2)
        {
            if (autumnLeavesParticle != null && !autumnLeavesParticle.isPlaying) autumnLeavesParticle.Play();
            if (windAudioSource != null && autumnWindClip != null) { windAudioSource.clip = autumnWindClip; windAudioSource.Play(); }
        }
        else if (currentPhase == 3)
        {
            if (snowParticle != null && !snowParticle.isPlaying) snowParticle.Play();
            if (windAudioSource != null && winterBlizzardClip != null) { windAudioSource.clip = winterBlizzardClip; windAudioSource.Play(); }
        }
    }

    private void TriggerPhaseNotifications()
    {
        if (NotificationManager.Instance == null) return;

        if (currentPhase == 2)
            NotificationManager.Instance.Show("Havalar soğumaya başladı. Kış yaklaşıyor...", NotificationType.Warning);
        else if (currentPhase == 3)
            NotificationManager.Instance.Show("İlk kar düştü! Vaktimiz tükeniyor, hemen yola çıkmalıyız!", NotificationType.Error);
    }

    public void ForceCampLighting()
    {
        if (sunLight == null) return;
        
        StopAllCoroutines();
        sunLight.shadows = LightShadows.Soft;

        if (currentPhase == 1)
        {
            sunLight.color = summerColor;
            sunLight.intensity = summerIntensity;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.fogColor = summerColor;
        }
        else if (currentPhase == 2)
        {
            sunLight.color = autumnColor;
            sunLight.intensity = autumnIntensity;
            RenderSettings.fogDensity = 0.01f;
            RenderSettings.fogColor = autumnColor;
        }
        else if (currentPhase == 3)
        {
            sunLight.color = winterColor;
            sunLight.intensity = winterIntensity;
            RenderSettings.fogDensity = 0.02f;
            RenderSettings.fogColor = winterColor;
        }
    }

    public void SetWeatherPaused(bool paused)
    {
        if (paused)
        {
            if (snowParticle != null && snowParticle.isPlaying) snowParticle.Pause();
            if (autumnLeavesParticle != null && autumnLeavesParticle.isPlaying) autumnLeavesParticle.Pause();
            if (windAudioSource != null && windAudioSource.isPlaying) windAudioSource.Pause();
        }
        else
        {
            if (currentPhase == 3)
            {
                if (snowParticle != null && !snowParticle.isPlaying) snowParticle.Play();
                if (windAudioSource != null && !windAudioSource.isPlaying) windAudioSource.UnPause();
            }
            else if (currentPhase == 2)
            {
                if (autumnLeavesParticle != null && !autumnLeavesParticle.isPlaying) autumnLeavesParticle.Play();
            }
        }
    }

    // ── GERİ YÜKLENEN SİNEMATİK GEÇİŞ MOTORU ───────────────────────────────
    // Işığın rengini ve gücünü yavaşça değiştiren, senin o efsanevi fonksiyonun.
    IEnumerator TransitionLight(Color targetColor, float targetIntensity, float duration)
    {
        Color startColor = sunLight.color;
        float startIntensity = sunLight.intensity;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            sunLight.color = Color.Lerp(startColor, targetColor, time / duration);
            sunLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, time / duration);
            yield return null;
        }
        
        sunLight.color = targetColor;
        sunLight.intensity = targetIntensity;
    }
}