using UnityEngine;
using System.Collections.Generic;

public class CommanderStorage : MonoBehaviour
{
    public static CommanderStorage Instance;

    [Header("Komutanın Aktif Nüshaları (Büyü Fermanları)")]
    public List<ItemData> ownedNushas = new List<ItemData>();

    [Header("Seferlerden Kazanılan Kalıcı Miraslar (Relics)")]
    public List<RelicData> activeRelics = new List<RelicData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Kamp sahnesinde yok olmaması için istersen açabilirsin:
        // DontDestroyOnLoad(gameObject);
    }

    // ── NÜSHA İŞLEMLERİ ──
    public void AddNusha(ItemData nusha)
    {
        if (nusha.type == ItemType.Nusha)
        {
            ownedNushas.Add(nusha);
            Debug.Log($"[Komutan Deposu] Yeni Nüsha Edinildi: {nusha.DisplayName}");
        }
    }

    public void UseNusha(ItemData nusha)
    {
        if (ownedNushas.Contains(nusha))
        {
            ownedNushas.Remove(nusha);
            // Savaşta fermanı yakma/büyüyü tetikleme kodun buraya gelecek
        }
    }

    // ── MİRASSAL (RELIC) İŞLEMLERİ ──
    public void AddRelic(RelicData relic)
    {
        if (relic != null && !activeRelics.Contains(relic))
        {
            activeRelics.Add(relic);
            // Burada küresel stat bonuslarını tetikleyebilirsin
            Debug.Log($"[Komutan Deposu] Kadim Miras Aktifleşti: {relic.relicName}");
        }
    }
}