using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// defeatPanel içindeki "Geri Dön" butonuna bu scripti bağla.
/// Inspector'dan doğrudan BattleManager.ReturnToCamp() bağlamak yerine
/// bu script üzerinden geç — doğru sırayı garanti eder:
///
///   1. BattleManager.ReturnToCamp()       (kamp görseli restore)
///   2. ExpeditionManager.FailExpedition() (çanta temizle, miras hesapla)
///   3. ExpeditionSummaryUI.ShowSummary()  (sefer özeti)
///      └─ CloseSummary()                 (özet kapanınca)
///           └─ MetaProgression.CheckPendingRelics()  (miras ekranı)
///
/// Inspector'da: defeatReturnButton → OnClick → DefeatReturnHandler.OnDefeatReturnClicked()
/// </summary>
public class DefeatReturnHandler : MonoBehaviour
{
    [Header("Buton Referansı")]
    [Tooltip("defeatPanel içindeki 'Geri Dön' butonu")]
    public Button defeatReturnButton;

    void Start()
    {
        if (defeatReturnButton != null)
            defeatReturnButton.onClick.AddListener(OnDefeatReturnClicked);
    }

    public void OnDefeatReturnClicked()
    {
        // 1. Kamp görünümünü restore et (kamera, askerler vb.)
        BattleManager.Instance?.ReturnToCamp();

        // 2. Sefer çantasını işle ve miras hesapla
        if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
        {
            // Çantada biriken altın/itibar/item transferini yap
            // FailExpedition zaten ResetExpedition çağırır
            int summaryGold      = ExpeditionManager.Instance.tempGold;
            int summaryRep       = ExpeditionManager.Instance.tempReputation;
            int summaryItems     = ExpeditionManager.Instance.tempItems.Count;
            int encounterCount   = ExpeditionManager.Instance.currentEncounterCount;

            // Kazanılan miras hakları (her 5 adım = 1 miras)
            int earnedRelics = encounterCount / 5;

            // Asker listesi ve hasar kontrolü
            var soldiers      = new System.Collections.Generic.List<Gladiator>();
            bool hadCasualties = false;
            foreach (var g in Object.FindObjectsByType<Gladiator>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!g.CompareTag("MySoldier")) continue;
                soldiers.Add(g);
                if (g.currentHealth <= 0) hadCasualties = true;
            }

            // FailExpedition: sadece negatif itibarı uygular, miras ekler
            ExpeditionManager.Instance.FailExpedition();

            // 3. Sefer özet ekranını göster
            // CloseSummary() içinde CheckPendingRelics() çağrılır → miras ekranı
            if (ExpeditionSummaryUI.Instance != null)
            {
                ExpeditionSummaryUI.Instance.ShowSummary(
                    goldGained:           summaryGold,
                    repGained:            summaryRep,
                    itemCount:            summaryItems,
                    encountersCompleted:  encounterCount,
                    daysSpent:            encounterCount,
                    relicsEarned:         earnedRelics,
                    soldiers:             soldiers,
                    hadCasualties:        hadCasualties
                );
            }
            else
            {
                // ExpeditionSummaryUI yoksa direkt miras kontrolüne geç
                MetaProgressionManager.Instance?.CheckPendingRelics();
            }
        }
        else
        {
            // Seferde değilken yenilgi (olmamalı ama güvenlik için)
            MetaProgressionManager.Instance?.CheckPendingRelics();
        }
    }
}
