using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Oyunun ana döngüsünü yönetir.
/// 
/// Tetikleyiciler:
///   ReputationManager.OnGameOver  → TriggerExile()
///   DayManager.OnWinterArrived    → TriggerFinalBoss() (30. gün)
///   exilePanel'deki "Yeni Sefer" butonu → StartNewRun()
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    // ── SÜRGÜN PANELİ ───────────────────────────────────────────────────────
    [Header("Sürgün Paneli")]
    public GameObject exilePanel;
    public TextMeshProUGUI exileDetailText;
    public Button newRunButton;          // "Yeni Sefer Başlat" butonu
    public Button relicScreenButton;     // "Miras Ekranı" butonu (opsiyonel)

    // ── ZAFER PANELİ ────────────────────────────────────────────────────────
    [Header("Zafer Paneli")]
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryDetailText;
    public Button victoryNewRunButton;   // Zafer sonrası yeni run

    // ── FINAL BOSS ──────────────────────────────────────────────────────────
    [Header("Final Boss")]
    public string finalBossSceneName = "BattleScene"; // Boss savaşı sahnesi
    [Tooltip("BattleManager'a geçilecek boss zorluğu (1-5)")]
    public int finalBossDifficulty = 5;

    // ── BAŞLANGIÇ DEĞERLERİ ─────────────────────────────────────────────────
    [Header("Başlangıç Değerleri")]
    public int startingGold       = 1000;
    public int startingReputation = 80;
    public int startingMorale     = 80;
    public int startingFood       = 50;

    // ── İÇ DURUM ────────────────────────────────────────────────────────────
    private bool _finalBossTriggered = false;
    private bool _exileInProgress    = false;

    // ────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (exilePanel   != null) exilePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    void Start()
    {
        // İtibar 0 → sürgün
        if (ReputationManager.Instance != null)
            ReputationManager.Instance.OnGameOver += TriggerExile;

        // Gün 30 doldu → süre bitti (boss yenilmediyse run kapanır)
        if (DayManager.Instance != null)
            DayManager.Instance.OnWinterArrived += OnTimeExpired;

        // Buton dinleyicileri
        if (newRunButton         != null) newRunButton.onClick.AddListener(StartNewRun);
        if (victoryNewRunButton  != null) victoryNewRunButton.onClick.AddListener(StartNewRun);
        if (relicScreenButton    != null) relicScreenButton.onClick.AddListener(OpenRelicScreen);
    }

    void OnDestroy()
    {
        if (ReputationManager.Instance != null)
            ReputationManager.Instance.OnGameOver -= TriggerExile;
        if (DayManager.Instance != null)
            DayManager.Instance.OnWinterArrived -= OnTimeExpired;
    }

    // ── SÜRGÜN ──────────────────────────────────────────────────────────────
    public void TriggerExile()
    {
        if (_exileInProgress) return;
        _exileInProgress = true;

        Debug.Log("<color=red>[RunManager] SÜRGÜN FERMANI GELDİ!</color>");

        // Aktif seferi iptal et
        if (ExpeditionManager.Instance != null)
            ExpeditionManager.Instance.FailExpedition();

        // Tüm panelleri kapat
        if (MapManager.Instance != null) MapManager.Instance.HideMap();

        // Miras hakkı hesapla ve kaydet (FailExpedition içinde zaten yapılıyor,
        // ama sürgün öncesi son bir kontrol)
        if (MetaProgressionManager.Instance != null)
            MetaProgressionManager.Instance.SaveMetaProgress();

        // Sürgün metnini güncelle
        int totalRelics = MetaProgressionManager.Instance != null
            ? MetaProgressionManager.Instance.currentSaveData.unlockedRelics.Count
            : 0;

        if (exileDetailText != null)
            exileDetailText.text =
                "Padişahın fermanı kampa ulaştı...\n\n" +
                "İtibarın yerle bir oldu. Ordun dağıtıldı, hazinene el konuldu.\n" +
                "Tüm kazandıklarını geride bırakıp sürgüne yollanıyorsun...\n\n" +
                $"<color=#C8A040>Geriye {totalRelics} ata yadigarı kaldı.\n" +
                "Bir sonraki seferde yanında olacaklar.</color>";

        if (exilePanel != null) exilePanel.SetActive(true);

        // Miras ekranı varsa pendingRelics kontrolü
        if (MetaProgressionManager.Instance != null)
            MetaProgressionManager.Instance.CheckPendingRelics();
    }

    // ── FINAL BOSS ──────────────────────────────────────────────────────────
    /// <summary>
    /// Boss'u yenmek için çağrılır (BattleManager'dan manuel çağır).
    /// 30. gün dolduğunda otomatik tetiklenmez — oyuncu boss'a ulaşmak zorunda değil.
    /// </summary>
    public void TriggerFinalBoss()
    {
        if (_finalBossTriggered) return;
        _finalBossTriggered = true;

        Debug.Log("[RunManager] Final Boss tetiklendi!");

        PlayerPrefs.SetInt("FinalBossDifficulty", finalBossDifficulty);
        PlayerPrefs.Save();
    }

    // ── 30. GÜN DOLDU — SÜRE BİTTİ ─────────────────────────────────────────
    /// <summary>
    /// DayManager.OnWinterArrived'a bağlı.
    /// Boss yenilmemişse run biter, miraslar korunur, ana menüye döner.
    /// </summary>
    private void OnTimeExpired()
    {
        if (_finalBossTriggered) return; // Boss zaten yenildi, bu tetiklenmesin

        Debug.Log("[RunManager] 30. gün doldu — süre bitti.");

        if (MetaProgressionManager.Instance != null)
            MetaProgressionManager.Instance.SaveMetaProgress();

        int totalRelics = MetaProgressionManager.Instance != null
            ? MetaProgressionManager.Instance.currentSaveData.unlockedRelics.Count
            : 0;

        if (exileDetailText != null)
            exileDetailText.text =
                "30 gün geçti. Kış geldi, yollar kapandı.\n\n" +
                "Kızıl Kale'ye ulaşamadan vakit tükendi.\n" +
                "Ordun dağıtıldı, kamp söküldü.\n\n" +
                $"<color=#C8A040>Geriye {totalRelics} ata yadigarı kaldı.\n" +
                "Bir sonraki seferde daha hızlı olmalısın.</color>";

        if (exilePanel != null) exilePanel.SetActive(true);
        if (MetaProgressionManager.Instance != null)
            MetaProgressionManager.Instance.CheckPendingRelics();
    }

    // ── ZAFER ───────────────────────────────────────────────────────────────
    /// <summary>BattleManager, final boss savaşı kazanılınca bunu çağırır.</summary>
    public void TriggerVictory()
    {
        Debug.Log("<color=green>[RunManager] ZAFER!</color>");

        if (MetaProgressionManager.Instance != null)
        {
            // Zafer bonusu: 3 ekstra miras hakkı
            MetaProgressionManager.Instance.AddPendingRelics(3);
            MetaProgressionManager.Instance.SaveMetaProgress();
        }

        int totalRelics = MetaProgressionManager.Instance != null
            ? MetaProgressionManager.Instance.currentSaveData.unlockedRelics.Count
            : 0;

        if (victoryDetailText != null)
            victoryDetailText.text =
                "Kızıl Kale düştü!\n\n" +
                "Ordun efsane oldu, adın Anadolu'nun dört bir yanına yayıldı.\n\n" +
                $"<color=#C8A040>Toplam {totalRelics} ata yadigarı kazandın.\n" +
                "Yeni seferde daha da güçlü başlayacaksın.</color>";

        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    // ── YENİ RUN ────────────────────────────────────────────────────────────
    public void StartNewRun()
    {
        Debug.Log("[RunManager] Yeni sefer başlıyor...");

        _exileInProgress    = false;
        _finalBossTriggered = false;

        // Panel kapat
        if (exilePanel   != null) exilePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // 1. Askerleri sil
        var soldiers = GameObject.FindGameObjectsWithTag("MySoldier");
        foreach (var s in soldiers) Destroy(s);

        // 2. Ekonomiyi sıfırla
        ResetEconomy();

        // 3. Günü sıfırla
        if (DayManager.Instance != null)
        {
            DayManager.Instance.currentDay = 1;
            DayManager.Instance.RefreshUI();
        }

        // 4. Haritayı sıfırla ve rastgeleleştir
        if (MapManager.Instance  != null) MapManager.Instance.ResetMapProgress();
        if (MapRandomizer.Instance != null) MapRandomizer.Instance.RandomizeMap();

        // 5. Relic bufflarını uygula (ZenginAta vb.)
        if (MetaProgressionManager.Instance != null)
            MetaProgressionManager.Instance.ApplyStartingRelics();

        // 6. Bildirim
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show(
                "Yeni sefer başladı. Atalarının ruhu seninle!",
                NotificationType.Success);

        Debug.Log("[RunManager] Yeni sefer hazır.");
    }

    // ── EKONOMİ SIFIRLAMA ───────────────────────────────────────────────────
    private void ResetEconomy()
    {
        // Altın
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.Spend(MoneyManager.Instance.gold);
            MoneyManager.Instance.Add(startingGold);
        }

        // İtibar
        if (ReputationManager.Instance != null)
        {
            int diff = startingReputation - ReputationManager.Instance.GetReputation();
            ReputationManager.Instance.ChangeReputation(diff);
        }

        // Moral
        if (CampMoraleManager.Instance != null)
        {
            int diff = startingMorale - CampMoraleManager.Instance.currentMorale;
            CampMoraleManager.Instance.ChangeMorale(diff);
        }

        // Erzak
        if (SupplyManager.Instance != null)
        {
            int diff = startingFood - SupplyManager.Instance.currentFood;
            if (diff > 0) SupplyManager.Instance.AddFood(diff);
            else if (diff < 0) SupplyManager.Instance.SpendFood(-diff);
        }

        // Nasip sıfırla
        if (NasipManager.Instance != null && NasipManager.Instance.currentNasip > 0)
            NasipManager.Instance.SpendNasip(NasipManager.Instance.currentNasip);
    }

    // ── MİRAS EKRANI ────────────────────────────────────────────────────────
    private void OpenRelicScreen()
    {
        if (MetaProgressionManager.Instance != null)
            MetaProgressionManager.Instance.CheckPendingRelics();
    }
}