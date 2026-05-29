using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum BuildingState { Locked, Ruined, UnderConstruction, Built }

public class BuildingClickable : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    public string buildingName;
    [TextArea] public string description;
    public BuildingState currentState = BuildingState.Built;
    public int repairCost = 1000;

    [Header("UI Bilgilendirme (3D Yazı)")]
    public TextMeshProUGUI statusText;

    [Header("İnşaat Süresi")]
    [Tooltip("Kaç harita adımı VEYA kaç gün geçince tamamlansın")]
    public int requiredEncounters = 3;
    public int currentRemainingEncounters = 0;

    [Tooltip("Gün geçince de ilerlesin mi? (Her daysPerProgress günde 1 ilerleme)")]
    public bool progressOnDayPass = true;
    [Tooltip("Kaç günde bir inşaat ilerlesin")]
    public int daysPerProgress = 1;
    private int _daysSinceLastProgress = 0;

    [Header("Görseller")]
    public GameObject ruinedModel;
    public GameObject builtModel;
    public GameObject constructionModel;

    [Header("Efektler")]
    public GameObject buildEffectPrefab;
    public Vector3    effectOffset = new Vector3(0, 2f, 0);

    [Header("Olaylar")]
    public UnityEvent OnBuiltClick;
    public UnityEvent<BuildingClickable> OnRepairClick;

    // ── Hover / Click ────────────────────────────────────────────────────
    [Header("Hover Ayarları")]
    public float hoverScaleAdd = 0.06f;
    public float scaleAnimTime = 0.12f;

    [Header("Outline Renkleri")]
    public Color outlineColorBuilt        = new Color(1f,   0.85f, 0.2f,  1f);
    public Color outlineColorConstruction = new Color(0.3f, 0.8f,  1f,   1f);
    public Color outlineColorRuined       = new Color(0.9f, 0.2f,  0.1f, 1f);

    [Header("Ses")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource _audio;

    // ── İç durum ─────────────────────────────────────────────────────────
    private Vector3       _baseScale;
    private Coroutine     _scaleCoroutine;
    private bool          _isHovered = false;

    // Outline component'leri — ruined ve built modelin her ikisinden toplanır
    private List<Outline> _outlines = new List<Outline>();

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

    // Önce tüm modelleri aktif et, outline'ları topla, sonra geri kapat
    if (ruinedModel       != null) ruinedModel.SetActive(true);
    if (builtModel        != null) builtModel.SetActive(true);
    if (constructionModel != null) constructionModel.SetActive(true);

    CollectOutlines();   // Hepsi aktifken topla

    UpdateVisuals();     // Sonra doğru modeli aç, gerisini kapat
}

    void OnEnable()
    {
        ExpeditionManager.OnEncounterAdvanced += OnEncounterAdvanced;

        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay += OnNewDay;
    }

    void OnDisable()
    {
        ExpeditionManager.OnEncounterAdvanced -= OnEncounterAdvanced;

        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay -= OnNewDay;
    }

    // ── Outline Toplama — ruined + built her ikisinden ───────────────────
    void CollectOutlines()
    {
        _outlines.Clear();

        // Ruined modelin outline'ları
        if (ruinedModel != null)
            _outlines.AddRange(ruinedModel.GetComponentsInChildren<Outline>(true));

        // Built modelin outline'ları
        if (builtModel != null)
            _outlines.AddRange(builtModel.GetComponentsInChildren<Outline>(true));

        // Construction modelinin outline'ları (varsa)
        if (constructionModel != null)
            _outlines.AddRange(constructionModel.GetComponentsInChildren<Outline>(true));

        // Başta hepsini kapat
        foreach (var o in _outlines) o.enabled = false;
    }

   // ── HOVER ─────────────────────────────────────────────────────────────
    void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;

        _isHovered = true;
        AnimateScale(_baseScale + Vector3.one * hoverScaleAdd);
        SetOutlines(true);
        if (hoverSound != null) _audio.PlayOneShot(hoverSound);

        if (CursorManager.Instance != null) CursorManager.Instance.SetInteractCursor();
    }

    void OnMouseExit()
    {
        _isHovered = false;
        AnimateScale(_baseScale);
        SetOutlines(false);

        if (CursorManager.Instance != null) CursorManager.Instance.SetNormalCursor();
    }

    // ── CLICK ─────────────────────────────────────────────────────────────
    public void OnMouseDown()
    {
        if (UIBlocker.IsPointerOverUI()) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;

        StartCoroutine(ClickPunch());
        if (clickSound != null) _audio.PlayOneShot(clickSound);

        switch (currentState)
        {
            case BuildingState.Ruined:
                RepairPanelManager.Instance?.OpenPanel(this);
                break;

            case BuildingState.UnderConstruction:
                NotificationManager.Instance.Show(
                    $"{buildingName} inşa ediliyor... {currentRemainingEncounters} adım kaldı.",
                    NotificationType.Warning);
                break;

            case BuildingState.Built:
                NotificationManager.Instance.Show($"{buildingName} binasına girdin.", NotificationType.Info);
                OnBuiltClick?.Invoke();
                break;

            case BuildingState.Locked:
                NotificationManager.Instance.Show("Bu bina henüz kilitli!", NotificationType.Info);
                break;
        }
    }

    // ── İNŞAAT İLERLEMESİ — Harita Adımı ────────────────────────────────
    void OnEncounterAdvanced() => AdvanceConstruction(source: "sefer adımı");

    // ── İNŞAAT İLERLEMESİ — Gün Geçişi ──────────────────────────────────
    void OnNewDay()
    {
        if (!progressOnDayPass) return;
        if (currentState != BuildingState.UnderConstruction) return;

        _daysSinceLastProgress++;
        if (_daysSinceLastProgress >= daysPerProgress)
        {
            _daysSinceLastProgress = 0;
            AdvanceConstruction(source: "gün");
        }
    }

    void AdvanceConstruction(string source)
    {
        if (currentState != BuildingState.UnderConstruction) return;

        currentRemainingEncounters--;

        if (currentRemainingEncounters <= 0)
        {
            currentState = BuildingState.Built;
            _daysSinceLastProgress = 0;

            if (buildEffectPrefab != null)
                Destroy(Instantiate(buildEffectPrefab, transform.position + effectOffset, Quaternion.identity), 3f);

            NotificationManager.Instance.Show(
                $"Müjde! {buildingName} inşası tamamlandı!",
                NotificationType.Success);

            // Outline'ları yeni duruma göre güncelle
            CollectOutlines();
        }
        else
        {
            NotificationManager.Instance.Show(
                $"{buildingName}: {currentRemainingEncounters} adım kaldı. ({source})",
                NotificationType.Info);
        }

        UpdateVisuals();
    }

    // ── VİZUELLER ─────────────────────────────────────────────────────────
    public void UpdateVisuals()
    {
        if (ruinedModel       != null) ruinedModel.SetActive(currentState == BuildingState.Ruined);
        if (builtModel        != null) builtModel.SetActive(currentState == BuildingState.Built);
        if (constructionModel != null) constructionModel.SetActive(currentState == BuildingState.UnderConstruction);

        // Hover aktifse outline'ları yenile (model değişmiş olabilir)
        if (_isHovered) SetOutlines(true);
        else            SetOutlines(false);

        UpdateStatusText();
    }

    void SetOutlines(bool visible)
    {
        Color c = currentState switch
        {
            BuildingState.Built             => outlineColorBuilt,
            BuildingState.UnderConstruction => outlineColorConstruction,
            BuildingState.Ruined            => outlineColorRuined,
            _                               => Color.gray
        };

        foreach (var o in _outlines)
        {
            if (o == null) continue;
            // Sadece aktif modeldeki outline'ı aç
            bool modelActive = o.gameObject.activeInHierarchy;
            o.enabled        = visible && modelActive;
            if (o.enabled) o.OutlineColor = c;
        }
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
        float t = 0f;
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
        Vector3 start    = transform.localScale;
        float t = 0f;
        while (t < half) { t += Time.unscaledDeltaTime; transform.localScale = Vector3.Lerp(start, punched, t / half); yield return null; }
        t = 0f;
        Vector3 returnTo = _isHovered ? hovered : _baseScale;
        while (t < half) { t += Time.unscaledDeltaTime; transform.localScale = Vector3.Lerp(punched, returnTo, t / half); yield return null; }
        transform.localScale = returnTo;
    }

    // ── ONARIM ────────────────────────────────────────────────────────────
    public int GetEffectiveCost()
    {
        if (MetaProgressionManager.Instance != null &&
            MetaProgressionManager.Instance.HasRelic(RelicType.DemirDovucu))
            return Mathf.RoundToInt(repairCost * 0.8f);
        return repairCost;
    }

    public void StartRepair()
    {
        if (MoneyManager.Instance != null && MoneyManager.Instance.gold >= repairCost)
        {
            MoneyManager.Instance.Spend(repairCost);
            currentState = BuildingState.UnderConstruction;
            currentRemainingEncounters = requiredEncounters;
            _daysSinceLastProgress = 0;
            UpdateVisuals();
            CollectOutlines();

            string progressNote = progressOnDayPass
                ? $"Her {daysPerProgress} günde veya her sefer adımında ilerler."
                : "Sefer adımlarında ilerler.";

            NotificationManager.Instance.Show(
                $"{buildingName} inşaatı başladı! {requiredEncounters} adım kaldı. {progressNote}",
                NotificationType.Info);
        }
        else
        {
            NotificationManager.Instance.Show("Yeterli akçe yok!", NotificationType.Error);
        }
    }

    private void UpdateStatusText()
    {
        if (statusText == null) return;
        statusText.text = currentState switch
        {
            BuildingState.Locked            => $"<color=#888888>{buildingName}\n<size=70%>(Kilitli)</size></color>",
            BuildingState.Ruined            => $"{buildingName}\n<size=70%>(Harabe)</size>",
            BuildingState.UnderConstruction => $"<color=yellow>{buildingName}\n<size=70%>({currentRemainingEncounters} Adım Kaldı)</size></color>",
            BuildingState.Built             => $"<color=white>{buildingName}</color>",
            _                               => buildingName
        };
    }
}
