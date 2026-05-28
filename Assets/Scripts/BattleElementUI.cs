using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Savaş ekranında sol ve sağ kenarda TFT tarzı element göstergesi.
///
/// Hierarchy (öneri):
///   BattleElementUI
///   ├── PlayerSide (sol panel — dikey layout group)
///   │   └── [ElementRowUI prefablar runtime'da spawn edilir]
///   └── EnemySide (sağ panel — dikey layout group)
///       └── [ElementRowUI prefablar runtime'da spawn edilir]
///
/// Her ElementRowUI prefabı içinde:
///   ├── ElementIcon   (Image)
///   ├── ElementName   (TextMeshProUGUI)
///   ├── Count         (TextMeshProUGUI)   "x2"
///   └── MatchArrow    (Image)  → avantaj/dezavantaj oku (isteğe bağlı)
/// </summary>
public class BattleElementUI : MonoBehaviour
{
    public static BattleElementUI Instance;

    [Header("Paneller")]
    public GameObject playerSidePanel;   // Sol — bizim askerler
    public GameObject enemySidePanel;    // Sağ — düşmanlar
    public GameObject rowPrefab;         // ElementRowUI prefabı

    [Header("Element İkonları — Inspector'dan ata")]
    public Sprite iconNormal;
    public Sprite iconInanc;
    public Sprite iconAtes;
    public Sprite iconZehir;

    [Header("Element Renkleri")]
    public Color colorNormal = new Color(0.7f, 0.7f, 0.7f);
    public Color colorInanc  = new Color(0.95f, 0.85f, 0.2f);   // Altın
    public Color colorAtes   = new Color(1f,   0.35f, 0.1f);    // Ateş kırmızısı
    public Color colorZehir  = new Color(0.35f, 0.85f, 0.2f);   // Zehir yeşili

    [Header("Avantaj/Dezavantaj Renkleri")]
    public Color advantageColor    = new Color(0.2f, 1f, 0.4f);  // Yeşil ok — avantaj
    public Color disadvantageColor = new Color(1f, 0.25f, 0.2f); // Kırmızı ok — dezavantaj

    [Header("Güncelleme Aralığı")]
    public float refreshInterval = 1.5f;

    private float _timer;

    // ─────────────────────────────────────────────────────────────────────

    void Awake() => Instance = this;

    void Start()
    {
        _timer = 0f;
        Refresh();
    }

    void Update()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.state != BattleState.Fighting) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = refreshInterval;
            Refresh();
        }
    }

    // ── Dışarıdan çağrılabilir (savaş başında, birim ölünce vs.) ─────────
    public void Refresh()
    {
        if (playerSidePanel == null || enemySidePanel == null || rowPrefab == null) return;

        var playerCounts = CountElements(isPlayer: true);
        var enemyCounts  = CountElements(isPlayer: false);

        BuildSide(playerSidePanel, playerCounts, enemyCounts, isPlayer: true);
        BuildSide(enemySidePanel,  enemyCounts,  playerCounts, isPlayer: false);
    }

    // ── Element sayımı ───────────────────────────────────────────────────
    Dictionary<ElementType, int> CountElements(bool isPlayer)
    {
        var counts = new Dictionary<ElementType, int>();
        string tag = isPlayer ? "MySoldier" : "EnemySoldier";

        foreach (var unit in FindObjectsByType<GladiatorAI>(FindObjectsSortMode.None))
        {
            if (!unit.CompareTag(tag) || unit.isDead || !unit.isInBattle) continue;

            var glad = unit.GetComponent<Gladiator>();
            if (glad?.data == null) continue;

            ElementType el = glad.data.elementType;
            if (!counts.ContainsKey(el)) counts[el] = 0;
            counts[el]++;
        }

        return counts;
    }

    // ── Tek taraf UI'ını kur ─────────────────────────────────────────────
    void BuildSide(GameObject panel,
                   Dictionary<ElementType, int> ownCounts,
                   Dictionary<ElementType, int> opponentCounts,
                   bool isPlayer)
    {
        // Eski satırları temizle
        foreach (Transform child in panel.transform)
        {
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        if (ownCounts.Count == 0) return;

        // Normal elementi en sona at
        var sortedElements = new List<ElementType>(ownCounts.Keys);
        sortedElements.Sort((a, b) =>
        {
            if (a == ElementType.Normal && b != ElementType.Normal) return 1;
            if (b == ElementType.Normal && a != ElementType.Normal) return -1;
            return ownCounts[b].CompareTo(ownCounts[a]); // Çok olandan aza sırala
        });

        foreach (var element in sortedElements)
        {
            int count = ownCounts[element];
            if (count == 0) continue;

            GameObject row = Instantiate(rowPrefab, panel.transform);
            var rowUI = row.GetComponent<ElementRowUI>();
            if (rowUI == null) continue;

            // İkon ve renk
            rowUI.elementIcon.sprite = GetIcon(element);
            rowUI.elementIcon.color  = GetColor(element);

            // İsim
            rowUI.elementName.text  = GetDisplayName(element);
            rowUI.elementName.color = GetColor(element);

            // Sayı
            rowUI.countText.text  = $"x{count}";
            rowUI.countText.color = Color.white;

            // Avantaj/Dezavantaj oku
            SetMatchArrow(rowUI, element, opponentCounts, isPlayer);
        }
    }

    // ── Avantaj/Dezavantaj oku ───────────────────────────────────────────
    void SetMatchArrow(ElementRowUI rowUI, ElementType myElement,
                       Dictionary<ElementType, int> opponentCounts, bool isPlayer)
    {
        if (rowUI.matchArrow == null) return;

        bool hasAdvantage    = false;
        bool hasDisadvantage = false;

        foreach (var opponentElement in opponentCounts.Keys)
        {
            if (opponentCounts[opponentElement] == 0) continue;

            float mult = CombatElementSystem.GetDamageMultiplier(myElement, opponentElement);

            if (mult >= 2f) hasAdvantage    = true;
            if (mult <= 0.5f) hasDisadvantage = true;
        }

        if (hasAdvantage && !hasDisadvantage)
        {
            rowUI.matchArrow.gameObject.SetActive(true);
            rowUI.matchArrow.color = advantageColor;
            // ▲ oku için sprite'ı Inspector'dan bağlayabilirsin
            // rowUI.matchArrow.sprite = arrowUpSprite;
        }
        else if (hasDisadvantage && !hasAdvantage)
        {
            rowUI.matchArrow.gameObject.SetActive(true);
            rowUI.matchArrow.color = disadvantageColor;
        }
        else if (hasAdvantage && hasDisadvantage)
        {
            // Karışık — sarı göster
            rowUI.matchArrow.gameObject.SetActive(true);
            rowUI.matchArrow.color = Color.yellow;
        }
        else
        {
            rowUI.matchArrow.gameObject.SetActive(false);
        }
    }

    // ── Yardımcılar ──────────────────────────────────────────────────────
    Sprite GetIcon(ElementType el)
    {
        return el switch
        {
            ElementType.Inanc => iconInanc,
            ElementType.Ates  => iconAtes,
            ElementType.Zehir => iconZehir,
            _                 => iconNormal
        };
    }

    Color GetColor(ElementType el)
    {
        return el switch
        {
            ElementType.Inanc => colorInanc,
            ElementType.Ates  => colorAtes,
            ElementType.Zehir => colorZehir,
            _                 => colorNormal
        };
    }

    string GetDisplayName(ElementType el)
    {
        return el switch
        {
            ElementType.Inanc => "İnanç",
            ElementType.Ates  => "Ateş",
            ElementType.Zehir => "Zehir",
            _                 => "Normal"
        };
    }

    // ── Savaş bitti — paneli gizle ────────────────────────────────────────
    public void Hide()
    {
        if (playerSidePanel != null) playerSidePanel.SetActive(false);
        if (enemySidePanel  != null) enemySidePanel.SetActive(false);
    }

    public void Show()
    {
        if (playerSidePanel != null) playerSidePanel.SetActive(true);
        if (enemySidePanel  != null) enemySidePanel.SetActive(true);
    }
}

// ── Her satır için küçük component ──────────────────────────────────────────
/// <summary>
/// ElementRowUI prefabının içindeki bileşenlere referans tutar.
/// Prefabın kök objesine ekle.
/// </summary>
public class ElementRowUI : MonoBehaviour
{
    public Image             elementIcon;
    public TextMeshProUGUI   elementName;
    public TextMeshProUGUI   countText;
    public Image             matchArrow;    // İsteğe bağlı — null olabilir
}