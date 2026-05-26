using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MosqueArea : MonoBehaviour
{
    [Header("İbadet Noktaları")]
    public PrayingSpot[] spots;

    // ── Hover / Click Geri Bildirimi ─────────────────────────────────────
    [Header("Hover Ayarları")]
    public float hoverScaleAdd = 0.06f;
    public float scaleAnimTime = 0.12f;

    [Header("Parıltı Efekti (isteğe bağlı)")]
    [Tooltip("Caminin Renderer'ı — emission rengi değiştirilir. Yoksa boş bırak.")]
    public Renderer mosqueRenderer;
    [Tooltip("Hover'da emission rengi")]
    public Color emissionHover  = new Color(1f, 0.9f, 0.4f) * 0.6f;   // Sıcak altın
    [Tooltip("Tıklanınca emission rengi (kısa flash)")]
    public Color emissionClick  = new Color(1f, 1f, 0.6f) * 1.2f;     // Parlak beyaz-altın
    private Color _emissionOff  = Color.black;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    [Header("Ses")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip assignedSound;   // Asker camiye gönderildiğinde
    private AudioSource _audio;

    [Header("Parçacık (isteğe bağlı)")]
    [Tooltip("Hover'da aktifleşecek dua/duman parçacığı")]
    public ParticleSystem hoverParticle;

    // ── İç durum ─────────────────────────────────────────────────────────
    private Vector3   _baseScale;
    private Coroutine _scaleCoroutine;
    private Coroutine _emissionCoroutine;
    private bool      _isHovered = false;

    // ─────────────────────────────────────────────────────────────────────

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _audio.spatialBlend = 1f;
        _audio.playOnAwake  = false;
    }

    void Start()
    {
        _baseScale = transform.localScale;

        // Emission başlangıçta kapalı
        if (mosqueRenderer != null)
        {
            mosqueRenderer.material.EnableKeyword("_EMISSION");
            mosqueRenderer.material.SetColor(EmissionColor, _emissionOff);
        }
    }

    // ── HOVER ─────────────────────────────────────────────────────────────
    void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;

        _isHovered = true;
        AnimateScale(_baseScale + Vector3.one * hoverScaleAdd);
        SetEmission(emissionHover);

        if (hoverParticle != null && !hoverParticle.isPlaying) hoverParticle.Play();
        if (hoverSound != null) _audio.PlayOneShot(hoverSound);
    }

    void OnMouseExit()
    {
        _isHovered = false;
        AnimateScale(_baseScale);
        SetEmission(_emissionOff);

        if (hoverParticle != null && hoverParticle.isPlaying) hoverParticle.Stop();
    }

    // ── CLICK ─────────────────────────────────────────────────────────────
    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;

        // Click punch + flash
        StartCoroutine(ClickPunch());
        StartCoroutine(EmissionFlash(emissionClick, 0.2f));

        if (clickSound != null) _audio.PlayOneShot(clickSound);

        // Seçili asker kontrolü
        Gladiator selectedSoldier = GladiatorSelector.Instance?.GetSelectedGladiator();

        if (selectedSoldier != null)
        {
            bool assigned = AssignSoldierToPray(selectedSoldier);
            if (assigned && assignedSound != null)
                _audio.PlayOneShot(assignedSound);
        }
        else
        {
            // Seçili asker yoksa bilgi mesajı
            int freeSpots = GetFreeSpotCount();
            if (freeSpots == 0)
                NotificationManager.Instance?.Show("Camide boş yer yok!", NotificationType.Warning);
            else
                NotificationManager.Instance?.Show(
                    $"Bir asker seç, sonra camiye tıkla. ({freeSpots} boş yer var)",
                    NotificationType.Info);
        }
    }

    // ── ASKER ATAMA ───────────────────────────────────────────────────────
    public bool AssignSoldierToPray(Gladiator soldier)
    {
        if (!soldier.IsAvailableForTask())
        {
            NotificationManager.Instance?.Show(
                "Bu asker müsait değil! Önce mevcut işini bitirmeli.",
                NotificationType.Warning);
            GladiatorSelector.Instance?.ClearSelection();
            return false;
        }

        foreach (var spot in spots)
        {
            if (!spot.isOccupied)
            {
                GladiatorPraying prayingScript = soldier.GetComponent<GladiatorPraying>();
                if (prayingScript == null)
                    prayingScript = soldier.gameObject.AddComponent<GladiatorPraying>();

                soldier.SetActivity(SoldierActivity.Praying);
                prayingScript.StartPraying(spot);
                GladiatorSelector.Instance?.ClearSelection();

                NotificationManager.Instance?.Show(
                    $"{soldier.data.gladiatorName} ibadete gönderildi.",
                    NotificationType.Success);
                return true;
            }
        }

        NotificationManager.Instance?.Show("Camide boş yer yok!", NotificationType.Warning);
        return false;
    }

    int GetFreeSpotCount()
    {
        int count = 0;
        foreach (var spot in spots) if (!spot.isOccupied) count++;
        return count;
    }

    // ── ANİMASYONLAR ──────────────────────────────────────────────────────
    void AnimateScale(Vector3 target)
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleRoutine(target));
    }

    IEnumerator ScaleRoutine(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float   t     = 0f;
        while (t < scaleAnimTime)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, t / scaleAnimTime);
            yield return null;
        }
        transform.localScale = target;
    }

    IEnumerator ClickPunch()
    {
        Vector3 hovered  = _baseScale + Vector3.one * hoverScaleAdd;
        Vector3 punched  = _baseScale - Vector3.one * (hoverScaleAdd * 0.5f);
        float   half     = scaleAnimTime * 0.5f;

        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, punched, t / half);
            yield return null;
        }
        t = 0f;
        Vector3 returnTo = _isHovered ? hovered : _baseScale;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(punched, returnTo, t / half);
            yield return null;
        }
        transform.localScale = returnTo;
    }

    void SetEmission(Color c)
    {
        if (mosqueRenderer == null) return;
        mosqueRenderer.material.SetColor(EmissionColor, c);
    }

    IEnumerator EmissionFlash(Color flashColor, float duration)
    {
        if (mosqueRenderer == null) yield break;
        if (_emissionCoroutine != null) StopCoroutine(_emissionCoroutine);

        SetEmission(flashColor);
        yield return new WaitForSecondsRealtime(duration);

        // Hover'daysa hover rengine dön, yoksa sönük
        SetEmission(_isHovered ? emissionHover : _emissionOff);
    }
}
