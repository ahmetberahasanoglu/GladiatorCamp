using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Ses Oynatıcıları (Audio Sources)")]
    public AudioSource musicSource;  // Chill Kamp müzikleri için
    public AudioSource sfxSource;    // Anlık efektler (Tıklama, altın, kılıç)
    public AudioSource ambientSource;// Savaş ambiyansı veya rüzgar sesi için (Döngüsel)
    public AudioSource dialogueSource;// Savaş ambiyansı veya rüzgar sesi için (Döngüsel)

    [Header("Müzikler (Kulağı Yormayan / Chill)")]
    public AudioClip summerCampMusic; // Normal günlerdeki sakin müzik
    public AudioClip winterCampMusic; // Kış geldiğinde çalacak daha soğuk/melankolik müzik
    public AudioClip battleAmbient;   // MÜZİK DEĞİL! Kılıç çarpışmaları, bağrışmalar, savaş alanı gürültüsü
    [Range(0f, 1f)] public float musicVolume = 0.5f; // Müziğin genel sesi (Çok bağırıp kulağı yormasın)

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
    public AudioClip warHorn; // Savaş Başlangıç Borusu
    [Range(0f, 1f)] public float eventVolume = 1f;

    [Header("Kamp İçi Eylem Sesleri (Her Şey İçin)")]
    public AudioClip upgradeBuilding;
    public AudioClip blacksmithOpen;    
    public AudioClip equipSound;
    public AudioClip barkSound;
    [Header("Savaş Sesleri")]
    public AudioClip swordSwoosh; // Bu zaten vardı
    public AudioClip swordHit; // Bu zaten vardı
    public AudioClip hitSound;    // Kılıcın ete/zırha çarpma sesi
    public AudioClip gruntSound;  // Askerin acı çekme/inleme sesi
    public AudioClip deathSound;  // Ölüm çığlığı/yere yığılma sesi
    public AudioClip woodSound;
    public AudioClip cheerSound;
    public AudioClip trainHitSound; // Talimhanede kılıç/odun vurma sesi
    public AudioClip eatFoodSound;  // Gün sonu erzak yendiğinde tok bir ses
    public AudioClip mapMoveSound;  // Haritada ikon hareket ederken (At toynağı vs.)
    [Range(0f, 1f)] public float actionVolume = 0.9f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           // DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        musicSource.volume = musicVolume;
        ambientSource.loop = true; // Ambiyans hep dönsün
        PlayCampMusic(false); // Başlangıçta yaz müziğini çal
        
        // Eğer kış sistemi tetiklenirse kış müziğine geçmek için evente abone olabiliriz
        if (DayManager.Instance != null)
            DayManager.Instance.OnWinterArrived += () => PlayCampMusic(true);
    }
public void PlayTypewriter() 
    {
        if (typewriterSound != null && sfxSource != null)
        {
            // Sese organik bir his vermek için pitch'i rastgele değiştir (0.9 ile 1.1 arası idealdır)
            sfxSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            
            // Sesi çal (uiVolume'un biraz daha düşüğünü kullanıyoruz ki kulak tırmalamasın)
            sfxSource.PlayOneShot(typewriterSound, uiVolume * 0.3f);
            
            // Pitch ayarını normal (1.0) haline geri döndür ki diğer oyun sesleri bozulmasın!
            sfxSource.pitch = 1.0f; 
        }
    }

    public void StartTypewriter() 
    {
        if (typewriterSound != null && dialogueSource != null)
        {
            dialogueSource.clip = typewriterSound;
            dialogueSource.loop = true; // 13 saniye yetmezse diye başa sarsın
            dialogueSource.volume = uiVolume * 0.5f; // Arka planda tatlı tatlı çalsın
            dialogueSource.Play();
        }
    }

    public void StopTypewriter()
    {
        if (dialogueSource != null && dialogueSource.isPlaying)
        {
            dialogueSource.Stop(); // Yazı bittiği an sesi bıçak gibi keser
        }
    }
    // --- GELİŞMİŞ SFX ÇALICI (VOLUME DESTEKLİ) ---
    // clip: Çalınacak ses | volumeScale: Sesin yüksekliği (0 ile 1 arası)
    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    // --- MÜZİK VE AMBİYANS GEÇİŞLERİ (Fade In/Out ile Kulağı Yormaz) ---
    public void PlayCampMusic(bool isWinter)
    {
        AudioClip targetMusic = isWinter ? winterCampMusic : summerCampMusic;
        StartCoroutine(FadeMusic(targetMusic, musicVolume));
        
        // Kamptaysak savaş gürültüsünü sustur
        StartCoroutine(FadeAmbient(null, 0f)); 
    }

    public void StartBattleAcoustics()
    {
        // Savaşa girince chill müziği yavaşça sustur
        StartCoroutine(FadeMusic(null, 0f)); 
        
        // Savaş borusunu öttür!
        PlayWarHorn();
        
        // Savaş alanı gürültüsünü (Kılıçlar, naralar) yavaşça yükselt
        ambientSource.clip = battleAmbient;
        ambientSource.Play();
        StartCoroutine(FadeAmbient(battleAmbient, 0.7f)); 
    }

    // Müziklerin aniden kesilip kulağı tırmalamaması için yumuşak geçiş (AAA Kalitesi)
    private IEnumerator FadeMusic(AudioClip newClip, float targetVolume)
    {
        float fadeTime = 1.5f;
        float startVolume = musicSource.volume;

        // Önce mevcut müziği yavaşça kıs
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
            // Yeni müziği yavaşça aç
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

    // --- KISA YOL ÇAĞIRICILARI (Volume ayarlarıyla birlikte) ---
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
    public void PlayTrainHit() => PlaySFX(trainHitSound, actionVolume * 0.7f); // Talim sesi biraz daha kısık olsun
    public void PlayEat() => PlaySFX(eatFoodSound, actionVolume);
    public void PlayBark() => PlaySFX(barkSound, actionVolume);
    public void PlayWood() => PlaySFX(woodSound, actionVolume*0.1f);
    public void PlaySword() => PlaySFX(swordSwoosh, 0.8f);
    public void PlayMapMove() => PlaySFX(mapMoveSound, actionVolume);
}