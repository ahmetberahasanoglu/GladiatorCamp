using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class SquadSlotUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI powerText;
    public Image backgroundImage;
    public Image borderImage;        // Dış çerçeve — Inspector'dan bağla
    public Button toggleButton;

    [Header("Renkler")]
    public Color normalBg       = new Color(0.15f, 0.15f, 0.15f, 1f);
    public Color selectedBg     = new Color(0.12f, 0.25f, 0.12f, 1f);
    public Color injuredBg      = new Color(0.25f, 0.08f, 0.08f, 1f);
    public Color normalBorder   = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color selectedBorder = new Color(1f, 0.84f, 0.1f, 1f);  // Altın

    [Header("Animasyon")]
    public float selectScale   = 1.08f;
    public float animDuration  = 0.12f;

    // ── İç Durum ──────────────────────────────────────────────
    public Gladiator gladiatorComponent { get; private set; }
    private bool _isSelected  = false;
    private bool _isInjured   = false;
    private Action<Gladiator, bool> _onToggle;
    private Coroutine _scaleCoroutine;

    // ── Kurulum ───────────────────────────────────────────────
    public void Setup(Gladiator glad, Action<Gladiator, bool> onToggle)
    {
        gladiatorComponent = glad;
        _onToggle          = onToggle;
        _isSelected        = false;

        // Asker ölü/yaralı mı?
        _isInjured = glad.currentHealth < glad.maxHealth * 0.5f;

        nameText.text  = glad.data.gladiatorName;
        powerText.text = "G:"+ glad.data.GetTotalStats();

        // Yaralıysa ek bilgi
        if (_isInjured)
        {
            int pct = Mathf.RoundToInt((glad.currentHealth / glad.maxHealth) * 100f);
            powerText.text += $"  <color=red>({pct}% can)</color>";
        }

        ApplyVisuals(instant: true);

        toggleButton.onClick.RemoveAllListeners();
        toggleButton.onClick.AddListener(OnClicked);
    }

    // ── Tıklama ───────────────────────────────────────────────
    void OnClicked()
    {
        _isSelected = !_isSelected;
        ApplyVisuals(instant: false);
        _onToggle?.Invoke(gladiatorComponent, _isSelected);
    }

    // ── Görsel Güncelleme ─────────────────────────────────────
    void ApplyVisuals(bool instant)
    {
        // Arka plan rengi
        if (backgroundImage != null)
        {
            backgroundImage.color = _isInjured && !_isSelected ? injuredBg
                                  : _isSelected                ? selectedBg
                                  :                              normalBg;
        }

        // Çerçeve rengi
        if (borderImage != null)
            borderImage.color = _isSelected ? selectedBorder : normalBorder;

        // Scale animasyonu
        Vector3 target = _isSelected
            ? Vector3.one * selectScale
            : Vector3.one;

        if (instant)
        {
            transform.localScale = target;
        }
        else
        {
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = StartCoroutine(ScaleTo(target));
        }
    }

    IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, t / animDuration);
            yield return null;
        }
        transform.localScale = target;
    }

    // ── Dışarıdan seçim sıfırlama ─────────────────────────────
    public void ForceDeselect()
    {
        _isSelected = false;
        ApplyVisuals(instant: false);
    }
}
