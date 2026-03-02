using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Ses Kaynakları (Audio Sources)")]
    [Tooltip("Arka plan müziği için (Sürekli çalar)")]
    public AudioSource musicSource; 
    [Tooltip("Arayüz, bildirim ve genel efektler için (2D)")]
    public AudioSource sfxSource;   

    [Header("Arayüz (UI) Sesleri")]
    public AudioClip buttonClick;
    public AudioClip errorSound;
    public AudioClip goldCoins;
    public AudioClip notificationSuccess;
    public AudioClip notificationWarning;

    [Header("Kamp & Olay Sesleri")]
    public AudioClip upgradeBuilding;
    public AudioClip paperRustle; // Harita açılırken/Görev seçerken
    public AudioClip dayPassed;   // NextDay butonuna basınca
    public AudioClip ravenPoof;   // Kuzgun öldüğünde

    [Header("Müzikler")]
    public AudioClip campMusic;
    public AudioClip battleMusic;

    void Awake()
    {
        // Klasik Singleton koruması (Sahneler arası silinmesin diye)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler değişse de müzik kesilmez!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMusic(campMusic); // Oyun başlar başlamaz kamp müziğini ver
    }

    // --- GENEL EFEKT ÇALICI ---
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        // PlayOneShot: Aynı anda birden fazla sesin (Örn: 3 kere altın sesi) üst üste çalmasına izin verir, birbirini kesmez.
        sfxSource.PlayOneShot(clip); 
    }

    // --- MÜZİK ÇALICI ---
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null || musicSource.clip == musicClip) return;

        musicSource.clip = musicClip;
        musicSource.Play();
        // İleride buraya müziği yavaşça kısıp (FadeOut) diğerini açan (FadeIn) bir Coroutine de ekleyebiliriz!
    }

    // --- HAZIR ÇAĞIRICI FONKSİYONLAR (Diğer kodlar bunları kullanacak) ---
    public void PlayClick() => PlaySFX(buttonClick);
    public void PlayError() => PlaySFX(errorSound);
    public void PlayGold() => PlaySFX(goldCoins);
    public void PlayPaper() => PlaySFX(paperRustle);
}