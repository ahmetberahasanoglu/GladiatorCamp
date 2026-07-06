using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Ses Oynatıcıları (Audio Sources)")]
    public AudioSource musicSource;  // Chill Kamp müzikleri için
    public AudioSource sfxSource;    // Anlık efektler (Tıklama, altın, kılıç)
    public AudioSource ambientSource;// Savaş ambiyansı veya rüzgar sesi için (Döngüsel)
    public AudioSource dialogueSource;// Diyalog ve yazı sesleri için

    [Header("Müzikler (Kulağı Yormayan / Chill)")]
    public AudioClip summerCampMusic; // Normal günlerdeki sakin müzik
    public AudioClip mapMusic;        // Sefer haritası müziği (YENİ)
    public AudioClip winterCampMusic; // Kış geldiğinde çalacak daha soğuk/melankolik müzik
    public AudioClip battleAmbient;   // MÜZİK DEĞİL! Savaş alanı gürültüsü
    
    [Range(0f, 1f)] public float musicVolume = 0.5f; 

    [Header("Arayüz Sesleri (UI)")]
    public AudioClip buttonClick;
    public AudioClip errorSound;
    public AudioClip paperSound;
    public AudioClip mapOpenSound;
    public AudioClip typewriterSound;

    [Range(0f, 1f)] public float uiVolume = 0.8f; 

    [Header("Ekonomi ve Olay Sesleri")]
    public AudioClip goldSound;
    public AudioClip diceRoll;
    public AudioClip warHorn; 
    [Range(0f, 1f)] public float eventVolume = 1f;

    [Header("Kamp İçi Eylem Sesleri (Her Şey İçin)")]
    public AudioClip upgradeBuilding;
    public AudioClip blacksmithOpen;    
    public AudioClip equipSound;
    public AudioClip barkSound;

    [Header("Savaş Sesleri")]
    public AudioClip swordSwoosh; 
    public AudioClip swordHit; 
    public AudioClip hitSound;    
    public AudioClip gruntSound;  
    public AudioClip deathSound;  
    public AudioClip woodSound;
    public AudioClip cheerSound;
    public AudioClip trainHitSound;
    public AudioClip eatFoodSound;
    public AudioClip mapMoveSound;
    public AudioClip bearRoar;  
    public AudioClip beastHit;  
    public AudioClip bearHit;  
    public AudioClip beastDeath;  
    public AudioClip gaziSoundClip;  

    [Range(0f, 1f)] public float actionVolume = 0.9f;

    public bool IsAtCamp { get; private set; } = true;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        musicSource.volume = musicVolume;
        ambientSource.loop = true; 
        PlayCampMusic(false); // Başlangıçta yaz müziğini çal
        
        if (DayManager.Instance != null)
            DayManager.Instance.OnWinterArrived += () => PlayCampMusic(true);
    }

    public void PlayTypewriter() 
    {
        if (typewriterSound != null && sfxSource != null)
        {
            sfxSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(typewriterSound, uiVolume * 0.7f);
            sfxSource.pitch = 1.0f; 
        }
    }

    public void StartTypewriter() 
    {
        if (typewriterSound != null && dialogueSource != null)
        {
            dialogueSource.clip = typewriterSound;
            dialogueSource.loop = true; 
            dialogueSource.volume = uiVolume * 0.5f; 
            dialogueSource.Play();
        }
    }

    public void StopTypewriter()
    {
        if (dialogueSource != null && dialogueSource.isPlaying)
        {
            dialogueSource.Stop(); 
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    // ── MÜZİK VE AMBİYANS GEÇİŞLERİ (Fade In/Out) ───────────────────────────

    public void PlayCampMusic(bool isWinter)
    {
        IsAtCamp = true;
        AudioClip targetMusic = isWinter ? winterCampMusic : summerCampMusic;
        StartCoroutine(FadeMusic(targetMusic, musicVolume));
        
        // Kamptaysak savaş gürültüsünü sustur
        StartCoroutine(FadeAmbient(null, 0f)); 
    }

    /// <summary>
    /// YENİ: Oyuncu sefere çıktığında harita müziğini pürüzsüzce başlatır.
    /// </summary>
    public void PlayMapMusic()
    {
        IsAtCamp = false;
        if (mapMusic != null)
        {
            StartCoroutine(FadeMusic(mapMusic, musicVolume));
            // Haritadayken arka plandaki olası savaş gürültülerini susturur
            StartCoroutine(FadeAmbient(null, 0f));
        }
    }

    public void StartBattleAcoustics()
    {
        // Savaşa girince chill müziği yavaşça sustur
        IsAtCamp = false;
        StartCoroutine(FadeMusic(null, 0f)); 
        
        PlayWarHorn();
        
        ambientSource.clip = battleAmbient;
        ambientSource.Play();
        StartCoroutine(FadeAmbient(battleAmbient, 0.7f)); 
    }

    private IEnumerator FadeMusic(AudioClip newClip, float targetVolume)
    {
        float fadeTime = 1.5f;
        float startVolume = musicSource.volume;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;

        if (newClip != null)
        {
            musicSource.Play();
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, targetVolume, t / fadeTime);
                yield return null;
            }
        }
    }

    private IEnumerator FadeAmbient(AudioClip newClip, float targetVolume)
    {
        float fadeTime = 2f;
        float startVol = ambientSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            ambientSource.volume = Mathf.Lerp(startVol, targetVolume, t / fadeTime);
            yield return null;
        }
        if (targetVolume == 0) ambientSource.Stop();
    }

    // ── KISA YOL ÇAĞIRICILARI ───────────────────────────────────────────────
    public void PlayClick() => PlaySFX(buttonClick, uiVolume);
    public void PlayError() => PlaySFX(errorSound, uiVolume);
    public void PlayPaper() => PlaySFX(paperSound, uiVolume);
    public void PlayMap() => PlaySFX(mapOpenSound, uiVolume);

    public void PlayGold() => PlaySFX(goldSound, eventVolume);
    public void PlayDice() => PlaySFX(diceRoll, eventVolume);
    public void PlayWarHorn() => PlaySFX(warHorn, eventVolume);

    public void PlayUpgrade() => PlaySFX(upgradeBuilding, actionVolume);
    public void PlayBlacksmith() => PlaySFX(blacksmithOpen, actionVolume);
    public void PlayCheer() => PlaySFX(cheerSound, actionVolume);
    public void PlayEquip() => PlaySFX(equipSound, actionVolume);
    public void PlayTrainHit() => PlaySFX(trainHitSound, actionVolume * 0.7f); 
    public void PlayEat() => PlaySFX(eatFoodSound, actionVolume);
    public void PlayBark() => PlaySFX(barkSound, actionVolume);
    public void PlayWood() => PlaySFX(woodSound, actionVolume * 0.1f);
    public void PlaySword() => PlaySFX(swordSwoosh, 0.8f);
    public void PlayMapMove() => PlaySFX(mapMoveSound, actionVolume);
    
    // DÜZELTME: mapMoveSound çalan hata gaziSoundClip olarak güncellendi.
    public void PlayGazi() => PlaySFX(gaziSoundClip, actionVolume); 
}