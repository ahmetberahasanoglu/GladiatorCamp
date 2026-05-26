using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Geliştirilmiş Güreş Mini Oyunu:
///   - 3 Faz: Her faz hız artar, hedef küçülür
///   - Iskalama toleransı: 1 ıskalama hakkı, 2'de kaybedilir
///   - Animasyonlu slider rengi (bölge yaklaştıkça sarıya döner)
///   - Faz göstergesi (HazırlanAşama 1/3)
///   - Sallantı efekti: Başarılı vuruşta slider kısa süre hızlanır
/// </summary>
public class WrestlingMiniGame : MonoBehaviour
{
    public static WrestlingMiniGame Instance;

    [Header("UI Elementleri")]
    public GameObject miniGamePanel;
    public Slider timingSlider;
    public RectTransform sweetSpotRect;
    public Button actionButton;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI phaseText;       // "Faz 1 / 3" yazısı
    public TextMeshProUGUI missText;        // "Dikkat — 1 hata hakkın kaldı" gibi
    public Image sliderFillImage;           // Slider'ın fill Image'ı (renk için)
    public Image sweetSpotImage;            // Yeşil alan Image'ı

    [Header("Oyun Ayarları")]
    public float baseSliderSpeed = 1.5f;
    [Tooltip("Her fazda slider ne kadar hızlanır")]
    public float speedIncreasePerPhase = 0.6f;
    [Tooltip("Her fazda sweet spot ne kadar küçülür (0-1 çarpanı)")]
    public float spotShrinkPerPhase = 0.75f;
    [Tooltip("Kazanmak için tüm fazları geçmesi gerekiyor")]
    public int totalPhases = 3;
    [Tooltip("Toplam kaç ıskalama hakkı var (0 = tek hatalı kaybet)")]
    public int maxMisses = 1;

    [Header("Renkler")]
    public Color safeColor   = new Color(0.2f, 0.8f, 0.2f);   // Yeşil — güvenli bölgede
    public Color dangerColor = new Color(0.9f, 0.3f, 0.1f);   // Kırmızı — uzakta
    public Color hitColor    = Color.white;                    // Flash rengi

    // ── İç Durum ───────────────────────────────────────────────────────
    private int   _currentPhase = 0;
    private int   _missCount    = 0;
    private float _currentSpeed;
    private float _sweetSpotMult;
    private bool  _movingRight  = true;
    private bool  _isGameActive = false;
    private float _minSuccess, _maxSuccess;
    private float _totalSliderWidth;
    private Action<bool> _onGameFinished;
    private Coroutine _flashCoroutine;

    void Awake() => Instance = this;

    void Start()
    {
        actionButton.onClick.AddListener(TryHit);
    }

    // ── Başlat ─────────────────────────────────────────────────────────
    public void StartWrestling(Gladiator soldier, int opponentStrength, Action<bool> callback)
    {
        _onGameFinished = callback;
        _currentPhase   = 0;
        _missCount      = 0;
        _movingRight    = true;
        timingSlider.value = 0.5f;

        // Güç oranına göre sweet spot başlangıç büyüklüğü
        float powerRatio = (float)soldier.data.strength / Mathf.Max(1, opponentStrength);
        _sweetSpotMult   = Mathf.Clamp(powerRatio * 0.35f, 0.12f, 0.75f);

        _totalSliderWidth = timingSlider.GetComponent<RectTransform>().rect.width;

        miniGamePanel.SetActive(true);
        BeginPhase(_currentPhase);
        _isGameActive = true;
    }

    void BeginPhase(int phase)
    {
        // Hız ve hedef boyutunu faza göre güncelle
        float phaseMult = Mathf.Pow(spotShrinkPerPhase, phase);
        float currentMult = _sweetSpotMult * phaseMult;
        _currentSpeed = baseSliderSpeed + (speedIncreasePerPhase * phase);

        // Sweet spot genişliği
        sweetSpotRect.sizeDelta = new Vector2(_totalSliderWidth * currentMult, sweetSpotRect.sizeDelta.y);

        float halfSize   = currentMult / 2f;
        _minSuccess      = 0.5f - halfSize;
        _maxSuccess      = 0.5f + halfSize;

        // Faz yazısını güncelle
        if (phaseText != null)
            phaseText.text = $"Hamle {phase + 1} / {totalPhases}";

        // Hata yazısını güncelle
        UpdateMissText();
    }

    // ── Update — Slider hareketi ve renk güncellemesi ──────────────────
    void Update()
    {
        if (!_isGameActive) return;

        // PingPong hareketi
        if (_movingRight)
        {
            timingSlider.value += _currentSpeed * Time.deltaTime;
            if (timingSlider.value >= 1f) _movingRight = false;
        }
        else
        {
            timingSlider.value -= _currentSpeed * Time.deltaTime;
            if (timingSlider.value <= 0f) _movingRight = true;
        }

        // İbre sweet spot'a ne kadar yakın? Buna göre renk değiştir
        UpdateSliderColor();
    }

    void UpdateSliderColor()
    {
        if (sliderFillImage == null) return;

        float val        = timingSlider.value;
        bool  inZone     = val >= _minSuccess && val <= _maxSuccess;
        float proximity  = 0f;

        if (!inZone)
        {
            float distToMin = Mathf.Abs(val - _minSuccess);
            float distToMax = Mathf.Abs(val - _maxSuccess);
            float dist      = Mathf.Min(distToMin, distToMax);
            proximity       = Mathf.Clamp01(1f - (dist / 0.3f));
        }
        else
        {
            proximity = 1f;
        }

        sliderFillImage.color = Color.Lerp(dangerColor, safeColor, proximity);
    }

    // ── Vuruş ──────────────────────────────────────────────────────────
    void TryHit()
    {
        if (!_isGameActive) return;

        float val    = timingSlider.value;
        bool  inZone = val >= _minSuccess && val <= _maxSuccess;

        if (inZone)
        {
            // Başarılı hamle
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashSweetSpot());

            _currentPhase++;

            if (_currentPhase >= totalPhases)
            {
                FinishGame(true);
                return;
            }

            // Sonraki faz — slider ortada başlasın
            timingSlider.value = 0.5f;
            BeginPhase(_currentPhase);
        }
        else
        {
            // Iskaladı
            _missCount++;
            UpdateMissText();

            // Slider'ı biraz salla (feedback)
            StartCoroutine(ShakeSlider());

            if (_missCount > maxMisses)
            {
                FinishGame(false);
            }
        }
    }

    // ── Oyun Sonu ──────────────────────────────────────────────────────
    void FinishGame(bool won)
    {
        _isGameActive = false;
        miniGamePanel.SetActive(false);
        _onGameFinished?.Invoke(won);
    }

    // ── Görsel Efektler ────────────────────────────────────────────────
    IEnumerator FlashSweetSpot()
    {
        if (sweetSpotImage == null) yield break;
        Color original = sweetSpotImage.color;
        sweetSpotImage.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        sweetSpotImage.color = original;
    }

    IEnumerator ShakeSlider()
    {
        if (sliderFillImage == null) yield break;
        sliderFillImage.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        // Renk UpdateSliderColor'a bırakılır
    }

    void UpdateMissText()
    {
        if (missText == null) return;
        int remaining = maxMisses - _missCount + 1;
        if (remaining > 0)
            missText.text = $"<color=orange>Hata hakkı: {remaining}</color>";
        else
            missText.text = "<color=red>Son şansın!</color>";
    }
}
