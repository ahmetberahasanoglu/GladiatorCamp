using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Kampa dönüşte gösterilen sefer özet ekranı.
/// ExpeditionManager.ReturnToCampSafely() kapanmadan önce bunu tetikler.
/// </summary>
public class ExpeditionSummaryUI : MonoBehaviour
{
    public static ExpeditionSummaryUI Instance;

    [Header("Panel")]
    public GameObject summaryPanel;
    public Button closeButton;          // "Kampa Dön" butonu

    [Header("Genel Bilgi")]
    public TextMeshProUGUI titleText;         // "Sefer Tamamlandı"
    public TextMeshProUGUI encounterText;     // "5 Düğüm İlerlendi · 3 Savaş Kazanıldı"
    public TextMeshProUGUI daysText;          // "3 Gün Geçirildi"

    [Header("Kazanımlar")]
    public TextMeshProUGUI goldText;          // "+240 Akçe"
    public TextMeshProUGUI repText;           // "+15 İtibar"
    public TextMeshProUGUI itemsText;         // "+2 Eşya"
    public TextMeshProUGUI relicText;         // "1 Miras Hakkı Kazanıldı!"

    [Header("Asker Durumu")]
    public Transform soldierListParent;       // Scroll view içi
    public GameObject soldierSummaryPrefab;   // Ad + Can + Gazi rozeti prefabı

    [Header("Atmosfer")]
    public TextMeshProUGUI flavorText;        // Sonuca göre değişen cümle

    private static readonly string[] WinLines =
    {
        "Kamp ateşi bu gece daha parlak yanıyor.",
        "Askerler başlarını dik tutarak geri döndü.",
        "Ganimetler arabanın içinde şıkır şıkır.",
        "İtibarın dört bir yana yayılıyor.",
    };
    private static readonly string[] ToughLines =
    {
        "Yaralar derin ama ruh kırılmadı.",
        "Bu sefer ağır geçti. Ama öğrendik.",
        "Kılıç paslanmadan dövülmez.",
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (summaryPanel != null) summaryPanel.SetActive(false);
        if (closeButton  != null) closeButton.onClick.AddListener(CloseSummary);
    }

    // ── GÖSTER ────────────────────────────────────────────────────────────
    public void ShowSummary(
        int goldGained,
        int repGained,
        int itemCount,
        int encountersCompleted,
        int daysSpent,
        int relicsEarned,
        List<Gladiator> soldiers,
        bool hadCasualties)
    {
        summaryPanel.SetActive(true);

        // Başlık
        if (titleText != null)
            titleText.text = hadCasualties ? "Sefer Sona Erdi..." : "Sefer Tamamlandı!";

        // Sayılar
        if (encounterText != null)
            encounterText.text = $"{encountersCompleted} Düğüm İlerlendi";

        if (daysText != null)
            daysText.text = $"{daysSpent} Gün Harcandı";

        // Kazanımlar
        if (goldText != null)
        {
            goldText.text  = goldGained >= 0
                ? $"<color=#FFD700>+{goldGained} Akçe</color>"
                : $"<color=red>{goldGained} Akçe</color>";
        }

        if (repText != null)
        {
            repText.text = repGained >= 0
                ? $"<color=#4CAF50>+{repGained} İtibar</color>"
                : $"<color=red>{repGained} İtibar</color>";
        }

        if (itemsText != null)
            itemsText.text = itemCount > 0
                ? $"<color=#00BCD4>+{itemCount} Eşya</color>"
                : "<color=#888888>Eşya yok</color>";

        if (relicText != null)
        {
            relicText.gameObject.SetActive(relicsEarned > 0);
            relicText.text = $"<color=#FFD700>✦ {relicsEarned} Miras Hakkı Kazanıldı!</color>";
        }

        // Asker listesi
        BuildSoldierList(soldiers);

        // Atmosfer cümlesi
        if (flavorText != null)
        {
            var pool = hadCasualties ? ToughLines : WinLines;
            flavorText.text = $"<i>{pool[Random.Range(0, pool.Length)]}</i>";
        }
    }

    private void BuildSoldierList(List<Gladiator> soldiers)
    {
        if (soldierListParent == null || soldierSummaryPrefab == null || soldiers == null) return;

        // Temizle
        foreach (Transform child in soldierListParent)
            Destroy(child.gameObject);

        foreach (var s in soldiers)
        {
            if (s == null || s.data == null) continue;

            GameObject row = Instantiate(soldierSummaryPrefab, soldierListParent);
            var texts      = row.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length < 1) continue;

            int hp    = Mathf.RoundToInt(s.currentHealth);
            int maxHp = Mathf.RoundToInt(s.maxHealth);

            string hpColor  = hp > maxHp * 0.6f ? "green"
                            : hp > maxHp * 0.3f ? "orange" : "red";

            string gaziTag  = s.data.isGazi ? " <color=#FFD700>[GAZİ]</color>" : "";
            string traitTag = GetTraitShortTag(s.data.trait);
            string dead     = hp <= 0 ? " <color=red>[DÜŞTÜ]</color>" : "";

            texts[0].text = $"{s.data.gladiatorName}{gaziTag}{traitTag}{dead}  " +
                            $"<color={hpColor}>{hp}/{maxHp} Can</color>";
        }
    }

    private string GetTraitShortTag(SoldierTrait trait) => trait switch
    {
        SoldierTrait.Obur    => " <color=#FF9800>[Obur]</color>",
        SoldierTrait.Dindar  => " <color=#64B5F6>[Dindar]</color>",
        SoldierTrait.Yetenekli => " <color=#81C784>[Yetenekli]</color>",
        _                    => ""
    };

    private void CloseSummary()
    {
        summaryPanel.SetActive(false);

        // Miras ekranı bekleyenleri kontrol et
        if (MetaProgressionManager.Instance != null)
            MetaProgressionManager.Instance.CheckPendingRelics();
    }
}