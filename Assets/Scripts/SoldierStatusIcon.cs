using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Askerin üzerinde durum ikonu gösteren World-Space Canvas bileşeni.
/// Her 0.2 saniyede bir durumu polling ile kontrol eder.
/// </summary>
public class SoldierStatusIcon : MonoBehaviour
{
    [Header("Icon Sprites — Inspector'dan ata")]
    public Sprite trainingSprite;   // Kılıç    — Eğitimde
    public Sprite prayingSprite;    // Hilal    — Dua'da
    public Sprite workingSprite;    // Kazma    — Çalışıyor
    public Sprite healingSprite;    // Kalp     — Şifahane
    public Sprite missionSprite;    // Çadır    — Keşifte
    public Sprite injuredSprite;    // Yara     — Can %50 altı

    [Header("UI Bileşenleri")]
    public GameObject backgroundObject; // YENİ: Açıp kapatacağımız yarı saydam arka plan (İkonun Parent'ı olmalı)
    public Image  iconImage;            // İkonu gösteren Image (Background'ın Child'ı)
    public Canvas iconCanvas;           // Sadece kameraya bakması (Billboard) için referans tutuyoruz

    [Header("Polling")]
    [Tooltip("Kaç saniyede bir durum kontrol edilsin")]
    public float pollInterval = 0.2f;

    // ── Bileşen referansları ──────────────────────────────────────────────
    private Gladiator         _gladiator;
    private GladiatorTraining _training;
    private GladiatorHealing  _healing;
    private GladiatorPraying  _praying;
    private Camera            _mainCam;

    // ── Polling ──────────────────────────────────────────────────────────
    private float  _pollTimer;
    private Sprite _lastSprite;        
    private bool   _lastVisible;

    // ─────────────────────────────────────────────────────────────────────

    void Awake()
    {
        _gladiator = GetComponentInParent<Gladiator>();
        _training  = GetComponentInParent<GladiatorTraining>();
        _healing   = GetComponentInParent<GladiatorHealing>();
        _praying   = GetComponentInParent<GladiatorPraying>();

        // ── YENİ: Oyun başlarken Canvas'ı değil, sadece arka plan objesini kapatıyoruz ──
        if (backgroundObject != null) 
        {
            backgroundObject.SetActive(false);
            _lastVisible = false;
        }
    }

    void Start()
    {
        _mainCam   = Camera.main;
        _pollTimer = 0f; 

        // Başlangıç durumunu anında yansıt
        ForceRefresh();
    }

    void Update()
    {
        // ── Billboard: Canvas her zaman kameraya baksın ────────────────────
        // (Görünmezken bile diğer objeler için dönmeye devam etmeli)
        if (_mainCam != null && iconCanvas != null)
        {
            iconCanvas.transform.LookAt(
                iconCanvas.transform.position + _mainCam.transform.rotation * Vector3.forward,
                _mainCam.transform.rotation   * Vector3.up);
        }

        // ── Polling ──────────────────────────────────────────────────────
        _pollTimer -= Time.deltaTime;
        if (_pollTimer <= 0f)
        {
            _pollTimer = pollInterval;
            CheckAndApply();
        }
    }

    // ── Durum Kontrolü ───────────────────────────────────────────────────
    void CheckAndApply()
    {
        if (_gladiator == null || iconImage == null || backgroundObject == null) return;

        Sprite chosen  = DetermineSprite();
        bool   visible = chosen != null;

        // Değişim yoksa UI'ya dokunma
        if (chosen == _lastSprite && visible == _lastVisible) return;

        _lastSprite  = chosen;
        _lastVisible = visible;

        // Tüm Canvas'ı değil, sadece Arka Plan objesini aç/kapat
        backgroundObject.SetActive(visible);
        
        // Eğer görünecekse doğru ikonu (Child) yerleştir
        if (visible) 
        {
            iconImage.sprite = chosen;
        }
    }

    public void ForceRefresh()
    {
        _lastSprite  = null;  
        _lastVisible = false;
        CheckAndApply();
    }

    // ── Hangi sprite gösterilecek? ────────────────────────────────────────
    Sprite DetermineSprite()
    {
        if (_gladiator == null || _gladiator.data == null) return null;

        // Öncelik sırası: Şifa > Eğitim > Dua > Çalışma > Keşif > Yaralı
        if (_healing  != null && _healing.IsHealing)    return healingSprite;
        if (_training != null && _training.IsTraining)  return trainingSprite;
        if (_praying  != null && _praying.isPraying)    return prayingSprite;

        SoldierActivity act = _gladiator.data.currentActivity;
        if (act == SoldierActivity.Working)   return workingSprite;
        if (act == SoldierActivity.OnMission) return missionSprite;
        if (act == SoldierActivity.Praying)   return prayingSprite; 

        // Yaralı: can %50'nin altındaysa ve boştaysa göster
        bool isInjured = _gladiator.currentHealth < _gladiator.maxHealth * 0.5f
                         && _gladiator.currentHealth > 0f;
        if (isInjured) return injuredSprite;

        // Hiçbiri değilse null dön (Arka plan kapanacak)
        return null;   
    }
}