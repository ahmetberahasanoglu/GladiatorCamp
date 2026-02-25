using UnityEngine;
using System.Collections;

public class SeasonManager : MonoBehaviour
{
    public static SeasonManager Instance;

    [Header("Işık Ayarları (Directional Light)")]
    public Light sunLight;
    
    [Header("1-50 Gün: Yaz / Erken Sonbahar")]
    public Color summerColor = new Color(1f, 0.95f, 0.8f); // Açık, sıcak sarı
    public float summerIntensity = 1.2f;

    [Header("50-80 Gün: Geç Sonbahar")]
    public Color autumnColor = new Color(1f, 0.7f, 0.4f); // Turuncumsu, hüzünlü
    public float autumnIntensity = 0.9f;
    public ParticleSystem autumnLeavesParticle; // Dökülen yapraklar

    [Header("80-100 Gün: Kışın Ayak Sesleri")]
    public Color winterColor = new Color(0.7f, 0.8f, 0.9f); // Soğuk, gri-mavi
    public float winterIntensity = 0.6f;
    public ParticleSystem snowParticle; // Kar yağışı

    [Header("Ses Efektleri")]
    public AudioSource windAudioSource;
    public AudioClip autumnWindClip;
    public AudioClip winterBlizzardClip;

    private int currentPhase = 0; // 1: Yaz, 2: Sonbahar, 3: Kış

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // DayManager'ın OnNewDay eventine abone oluyoruz (Eğitim sistemindeki gibi!)
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay += CheckSeason;
            // İlk açılışta atmosferi ayarla (DayManager'da gün değişkeninin adını currentDay varsayıyorum)
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

    // Her gün atladığında bu tetiklenecek
    void CheckSeason()
    {
        UpdateAtmosphere(DayManager.Instance.currentDay);
    }

    public void UpdateAtmosphere(int day)
    {
        if (sunLight == null) return;

        // --- 1. FAZ: GÜNEŞLİ GÜNLER (1-50) ---
        if (day < 50)
        {
            if (currentPhase != 1) // Sadece faza ilk geçildiğinde ayarla
            {
                currentPhase = 1;
                StartCoroutine(TransitionLight(summerColor, summerIntensity, 2f));
                
                if (autumnLeavesParticle != null) autumnLeavesParticle.Stop();
                if (snowParticle != null) snowParticle.Stop();
                
                if (windAudioSource != null) windAudioSource.Stop();
                RenderSettings.fogDensity = 0.01f; // Sis az
                RenderSettings.fogColor = summerColor;
            }
        }
        // --- 2. FAZ: SONBAHAR YAPRAKLARI (50-80) ---
        else if (day >= 50 && day < 80)
        {
            if (currentPhase != 2)
            {
                currentPhase = 2;
                StartCoroutine(TransitionLight(autumnColor, autumnIntensity, 3f));
                
                if (autumnLeavesParticle != null && !autumnLeavesParticle.isPlaying) autumnLeavesParticle.Play();
                if (snowParticle != null) snowParticle.Stop();

                if (windAudioSource != null && autumnWindClip != null)
                {
                    windAudioSource.clip = autumnWindClip;
                    windAudioSource.Play();
                }

                RenderSettings.fogDensity = 0.01f; // Sis biraz artar
                RenderSettings.fogColor = autumnColor;
                
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show("Havalar soğumaya başladı. Kış yaklaşıyor...", NotificationType.Warning);
            }
        }
        // --- 3. FAZ: KIŞ GELİYOR (80-100) ---
        else if (day >= 80)
        {
            if (currentPhase != 3)
            {
                currentPhase = 3;
                StartCoroutine(TransitionLight(winterColor, winterIntensity, 4f));
                
                if (autumnLeavesParticle != null) autumnLeavesParticle.Stop();
                if (snowParticle != null && !snowParticle.isPlaying) snowParticle.Play();

                if (windAudioSource != null && winterBlizzardClip != null)
                {
                    windAudioSource.clip = winterBlizzardClip;
                    windAudioSource.Play();
                }

                RenderSettings.fogDensity = 0.04f; // Yoğun, kasvetli sis
                RenderSettings.fogColor = winterColor;

                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show("İlk kar düştü! Vaktimiz tükeniyor, hemen yola çıkmalıyız!", NotificationType.Error);
            }
        }
    }

    // Işığın rengini ve gücünü aniden değil, yavaşça (sinematik) değiştiren fonksiyon
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