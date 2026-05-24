using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Ok atma sahnesine rüzgar ekler.
/// - Belirli aralıklarla yön ve güç değişir
/// - UI'da rüzgar göstergesi (ok ikonu + text) gösterir
/// - Arrow.cs'teki Rigidbody'ye sürekli kuvvet uygular
/// </summary>
public class WindManager : MonoBehaviour
{
    public static WindManager Instance;

    [Header("Rüzgar Ayarları")]
    [Tooltip("Rüzgarın maksimum gücü (N)")]
    public float maxWindForce   = 8f;
    [Tooltip("Rüzgar yön değişim aralığı (saniye)")]
    public float minChangeTime  = 4f;
    public float maxChangeTime  = 9f;
    [Tooltip("Yön değişirken ne kadar pürüzsüz geçiş olsun")]
    public float transitionSpeed = 1.5f;

    [Header("Askerin Stat Etkisi")]
    [Tooltip("Bu stat değerinde rüzgar etkisi sıfırlanır (tam usta)")]
    public int masterWindStatThreshold = 80;

    [Header("UI — Rüzgar Göstergesi")]
    public RectTransform windArrowIcon;   // Dönen ok ikonu
    public TextMeshProUGUI windText;      // "Rüzgar: Kuzey — Güçlü"
    public TextMeshProUGUI windWarning;   // "Rüzgar değişiyor!" uyarısı
    public Image windStrengthBar;         // Dolu bar = güçlü rüzgar (opsiyonel)

    // ── İç durum ──────────────────────────────────────────────────────────
    private Vector3 _currentWind;     // Aktif rüzgar vektörü
    private Vector3 _targetWind;      // Geçişin hedef değeri
    private float   _windReduction   = 1f; // Asker statından gelen azaltma (0-1)
    private bool    _isChanging      = false;

    // ── AWAKE / START ──────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // İlk rüzgarı belirle
        _targetWind  = RandomWindVector();
        _currentWind = _targetWind;

        UpdateWindUI();
        StartCoroutine(WindChangeLoop());
    }

    // ── ASKER STATINI UYGULA ───────────────────────────────────────────────
    /// <summary>
    /// ArcheryGameManager, oyuncu asker seçince bunu çağırır.
    /// SPD ne kadar yüksekse rüzgar etkisi o kadar az.
    /// </summary>
    public void SetSoldierStat(int speedStat)
    {
        // 0 stat → tam etki, masterThreshold → sıfır etki
        float ratio     = Mathf.Clamp01((float)speedStat / masterWindStatThreshold);
        _windReduction  = 1f - (ratio * 0.7f); // En iyi asker bile %30 etki alır
        Debug.Log($"[Wind] Asker hızı {speedStat} → Rüzgar azaltma çarpanı: {_windReduction:F2}");
    }

    // ── OKHA KUVVET UYGULA ────────────────────────────────────────────────
    /// <summary>
    /// Arrow.cs FixedUpdate'ten bu metodu çağırır.
    /// </summary>
    public Vector3 GetWindForce()
    {
        return _currentWind * _windReduction;
    }

    // ── RÜZGAR DEĞİŞİM DÖNGÜSÜ ───────────────────────────────────────────
    private IEnumerator WindChangeLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minChangeTime, maxChangeTime);
            yield return new WaitForSeconds(waitTime - 1.5f);

            // 1.5 saniye önceden uyar
            if (windWarning != null)
            {
                windWarning.text    = "Rüzgar değişiyor!";
                windWarning.enabled = true;
                _isChanging         = true;
            }

            yield return new WaitForSeconds(1.5f);

            _targetWind = RandomWindVector();
            _isChanging = false;
            if (windWarning != null) windWarning.enabled = false;
        }
    }

    // ── UPDATE — Yumuşak geçiş ─────────────────────────────────────────────
    void Update()
    {
        // Rüzgarı hedef değere doğru lerp et
        _currentWind = Vector3.Lerp(_currentWind, _targetWind,
                                    transitionSpeed * Time.deltaTime);
        UpdateWindUI();
    }

    // ── UI GÜNCELLEME ─────────────────────────────────────────────────────
    private void UpdateWindUI()
    {
        float magnitude = _currentWind.magnitude;

        // Ok ikonunu rüzgar yönüne döndür
        if (windArrowIcon != null && magnitude > 0.1f)
        {
            Vector2 dir2D       = new Vector2(_currentWind.x, _currentWind.z);
            float   angle       = Mathf.Atan2(dir2D.y, dir2D.x) * Mathf.Rad2Deg;
            windArrowIcon.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Güç metni
        if (windText != null)
        {
            string direction = GetWindDirectionName(_currentWind);
            string strength  = GetWindStrengthName(magnitude);
            windText.text    = $"{direction} — {strength}";

            // Renk: zayıf=beyaz, orta=sarı, güçlü=kırmızı
            windText.color = magnitude < maxWindForce * 0.33f ? Color.white
                           : magnitude < maxWindForce * 0.66f ? Color.yellow
                           : new Color(1f, 0.4f, 0.4f);
        }

        // Güç barı
        if (windStrengthBar != null)
            windStrengthBar.fillAmount = Mathf.Clamp01(magnitude / maxWindForce);
    }

    // ── YARDIMCILAR ───────────────────────────────────────────────────────
    private Vector3 RandomWindVector()
    {
        // X ve Z ekseninde rüzgar (Y'yi etkilemez — sadece yatay kayış)
        float force = Random.Range(0f, maxWindForce);
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle) * force, 0f, Mathf.Sin(angle) * force);
    }

    private string GetWindDirectionName(Vector3 wind)
    {
        if (wind.magnitude < 0.5f) return "Sakin";
        float angle = Mathf.Atan2(wind.x, wind.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        if (angle < 22.5f || angle >= 337.5f) return "Kuzey";
        if (angle < 67.5f)  return "Kuzeydoğu";
        if (angle < 112.5f) return "Doğu";
        if (angle < 157.5f) return "Güneydoğu";
        if (angle < 202.5f) return "Güney";
        if (angle < 247.5f) return "Güneybatı";
        if (angle < 292.5f) return "Batı";
        return "Kuzeybatı";
    }

    private string GetWindStrengthName(float magnitude)
    {
        if (magnitude < maxWindForce * 0.2f) return "Esinti";
        if (magnitude < maxWindForce * 0.5f) return "Orta";
        if (magnitude < maxWindForce * 0.8f) return "Güçlü";
        return "Fırtına";
    }
}