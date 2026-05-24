using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Her tier için hangi EnemyLoadout havuzunun kullanılacağını tanımlar.
/// Inspector'da tek bir obje oluştur ve BattleManager'a bağla.
/// </summary>
[CreateAssetMenu(menuName = "Data/EnemyTierConfig")]
public class EnemyTierConfig : ScriptableObject
{
    [Header("Tier 1 — Zayıf Düşmanlar (Derinlik 1-4)")]
    [Tooltip("Bu listeden rastgele seçilir")]
    public List<EnemyLoadout> tier1Loadouts = new List<EnemyLoadout>();

    [Header("Tier 2 — Orta Düşmanlar (Derinlik 5-9)")]
    public List<EnemyLoadout> tier2Loadouts = new List<EnemyLoadout>();

    [Header("Tier 3 — Güçlü Düşmanlar (Derinlik 10+)")]
    public List<EnemyLoadout> tier3Loadouts = new List<EnemyLoadout>();

    [Header("Boss")]
    public EnemyLoadout bossLoadout;

    // ── Yardımcı ────────────────────────────────────────────────────────────
    public EnemyLoadout GetRandomLoadout(int tier)
    {
        List<EnemyLoadout> pool = tier switch
        {
            1 => tier1Loadouts,
            2 => tier2Loadouts,
            _ => tier3Loadouts,
        };

        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning($"[EnemyTierConfig] Tier {tier} havuzu boş!");
            return null;
        }

        return pool[Random.Range(0, pool.Count)];
    }
}