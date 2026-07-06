using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ── RELIC LİSTESİ ───────────────────────────────────────────────────────────
public enum RelicType
{
    None,

    // ── ALTIN & KAYNAK ──────────────────────────────────────────────────────
    ZenginAta,       // +150 başlangıç altını
    BereketliYol,    // Seferde bulunan altın +%25
    CimriDede,       // Eğitim masrafları -%20
    HazineNefesi,    // Kampa dönüşte tempGold'a +%10 bonus

    // ── ERZAK & HAYATTA KALMA ───────────────────────────────────────────────
    KutluMide,       // Günlük erzak tüketimi -%20
    AvciBaba,        // Başlangıç erzağı +30
    TuzluEt,         // Kış günlük erzak tüketimi -%1 (ek)

    // ── ASKERİ GÜÇ ──────────────────────────────────────────────────────────
    GaziKani,        // Tüm askerler +15 max can ile başlar
    DemirBilek,      // Tüm askerler +3 güç (STR) ile başlar
    CelikZirh,       // Tüm askerler +3 savunma (DEF) ile başlar
    RuzgarAyak,      // Tüm askerler +2 hız (SPD) ile başlar

    // ── EĞİTİM & GELİŞİM ────────────────────────────────────────────────────
    DemirDovucu,     // Eğitim süresi -%1 gün (min 1)
    UstalıkOcağı,    // Eğitimden kazanılan stat +1 bonus
    OldurumUstası,   // İyileştirme maliyeti -%30

    // ── MORAL & İTİBAR ──────────────────────────────────────────────────────
    CesurYurek,      // Savaş kaybedilince moral kaybı -%50
    OnurluKan,       // Run başlangıcında itibar +10
    HalkınGözü,      // Kervansaray komisyonu -%5 (normal %10 → %5)

    // ── SEFER & HARITA ──────────────────────────────────────────────────────
    KaderYolu,       // Her seferin başında 1 fazla node erişilebilir
    NasipliYolcu,    // Run başında nasip +15 bonus
    EfsaneviHikaye,  // Her 5 encounter'da relic hakkı +1 (yani iki relic)
}


// ── KAYIT VERİSİ ────────────────────────────────────────────────────────────
[System.Serializable]
public class MetaSaveData
{
    public List<RelicType> unlockedRelics = new List<RelicType>();
}

// ── MANAGER ─────────────────────────────────────────────────────────────────
public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance;

    private string saveFilePath;
    public MetaSaveData currentSaveData = new MetaSaveData();
    public RelicSelectionUI relicSelectionUI;
    public int pendingRelicPicks = 0;
[Header("Tüm Relic ScriptableObject Veritabanı")]
    [Tooltip("Oluşturduğunuz tüm RelicData ScriptableObject'lerini buraya sürükleyin.")]
    public List<RelicData> allRelicDatabase = new List<RelicData>();

    /// <summary>
    /// Enum tipine göre veritabanından ilgili RelicData'yı bulur.
    /// </summary>
    public RelicData GetRelicData(RelicType type)
    {
        // RelicData içindeki relicID değeri ile Enum adının eşleştiğini varsayıyoruz (Örn: "ZenginAta")
        return allRelicDatabase.Find(r => r != null && r.relicID == type.ToString());
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        saveFilePath = Path.Combine(Application.persistentDataPath, "AtaYadigarlari.json");
        LoadMetaProgress();
    }

    // ── KAYIT / YÜKLEME ─────────────────────────────────────────────────────
    public void SaveMetaProgress()
    {
        string json = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("[Meta] Kaydedildi: " + saveFilePath);
    }

    public void LoadMetaProgress()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentSaveData = JsonUtility.FromJson<MetaSaveData>(json);
            Debug.Log($"[Meta] Yüklendi. Yadigâr sayısı: {currentSaveData.unlockedRelics.Count}");
        }
        else
        {
            currentSaveData = new MetaSaveData();
            Debug.Log("[Meta] Yeni miras defteri açıldı.");
        }
    }

    public bool HasRelic(RelicType type)
        => currentSaveData.unlockedRelics.Contains(type);

    // ── SEÇİM KUYRUĞU ───────────────────────────────────────────────────────
    public void AddPendingRelics(int count)
    {
        pendingRelicPicks += count;
        CheckPendingRelics();
    }

    public void CheckPendingRelics()
    {
        if (pendingRelicPicks > 0) TriggerRelicChoice();
        else if (relicSelectionUI != null) relicSelectionUI.ClosePanel();
    }

    private void TriggerRelicChoice()
    {
        var all = System.Enum.GetValues(typeof(RelicType))
                             .Cast<RelicType>()
                             .Where(r => r != RelicType.None && !HasRelic(r))
                             .ToList();

        if (all.Count == 0)
        {
            Debug.Log("[Meta] Tüm yadigarlar açıldı!");
            pendingRelicPicks = 0;
            return;
        }

        var options = new List<RelicType>();
        int count   = Mathf.Min(3, all.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, all.Count);
            options.Add(all[idx]);
            all.RemoveAt(idx);
        }

        if (relicSelectionUI != null) relicSelectionUI.ShowOptions(options);
    }

    public void UnlockRelic(RelicType chosen)
    {
        if (HasRelic(chosen)) return;

        currentSaveData.unlockedRelics.Add(chosen);
        SaveMetaProgress();

        // ── YENİ: SEÇİLEN YADİGARI COMMANDER STORAGE'A EKLE ──
        if (CommanderStorage.Instance != null)
        {
            RelicData rData = GetRelicData(chosen);
            if (rData != null)
            {
                CommanderStorage.Instance.AddRelic(rData);
                
                // Üst bar ve Defter UI açık ise anlık olarak yenilensin
                if (TopInfoBarUI.Instance != null) TopInfoBarUI.Instance.RefreshRelics();
            }
            else
            {
                Debug.LogWarning($"[Meta] Veritabanında {chosen} türüne ait eşleşen bir RelicData bulunamadı! 'relicID' kontrol edin.");
            }
        }
        // ─────────────────────────────────────────────────────

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show(
                "Ata Yadigarı: " + GetRelicDisplayName(chosen),
                NotificationType.Success);

        pendingRelicPicks--;
        CheckPendingRelics();
    }

    // ── BUFFLAR — RUN BAŞINDA UYGULA ────────────────────────────────────────
    /// <summary>
    /// RunManager.StartNewRun() içinden çağrıl.
    /// Her relic sadece bir kez, run başında efektini uygular.
    /// </summary>
    public void ApplyStartingRelics()
    {
       if (CommanderStorage.Instance != null)
        {
            foreach (RelicType unlocked in currentSaveData.unlockedRelics)
            {
                RelicData rData = GetRelicData(unlocked);
                if (rData != null) 
                    CommanderStorage.Instance.AddRelic(rData);
            }
            
            // Üst barı tetikle
            if (TopInfoBarUI.Instance != null) TopInfoBarUI.Instance.RefreshRelics();
        }
        // ────────────────────────────────────────────────────────────────

        // ── ALTIN & KAYNAK (Mevcut kodların aynen devam ediyor...) ──
        if (HasRelic(RelicType.ZenginAta) && MoneyManager.Instance != null)
        {
            MoneyManager.Instance.Add(150);
            Debug.Log("[Relic] ZenginAta: +150 altın");
        }

        if (HasRelic(RelicType.AvciBaba) && SupplyManager.Instance != null)
        {
            SupplyManager.Instance.AddFood(30);
            Debug.Log("[Relic] AvciBaba: +30 erzak");
        }

        // ── MORAL & İTİBAR ──────────────────────────────────────────────────
        if (HasRelic(RelicType.OnurluKan) && ReputationManager.Instance != null)
        {
            ReputationManager.Instance.ChangeReputation(10);
            Debug.Log("[Relic] OnurluKan: +10 itibar");
        }

        if (HasRelic(RelicType.NasipliYolcu) && NasipManager.Instance != null)
        {
            NasipManager.Instance.AddNasip(3);
            Debug.Log("[Relic] NasipliYolcu: +3 nasip");
        }

        // ── ASKERİ GÜÇ — Mevcut tüm askerlere uygula ───────────────────────
        ApplySoldierStartingBuffs();

        Debug.Log("[Meta] Tüm başlangıç relic buffları uygulandı.");
    }

    private void ApplySoldierStartingBuffs()
    {
        // FindObjectsByType yerine CampManager'dan referans al (optimizasyon)
        Gladiator[] soldiers = GetAllSoldiers();
        if (soldiers == null || soldiers.Length == 0) return;

        foreach (var g in soldiers)
        {
            if (g == null || g.data == null) continue;

            if (HasRelic(RelicType.GaziKani))
            {
                g.data.maxHealth += 15;
                g.data.currentHealth = Mathf.Min(g.data.currentHealth + 15, g.data.maxHealth);
                Debug.Log($"[Relic] GaziKani: {g.data.gladiatorName} +15 max can");
            }

            if (HasRelic(RelicType.DemirBilek))
            {
                g.data.strength += 3;
                Debug.Log($"[Relic] DemirBilek: {g.data.gladiatorName} +3 STR");
            }

            if (HasRelic(RelicType.CelikZirh))
            {
                g.data.defense += 3;
                Debug.Log($"[Relic] CelikZirh: {g.data.gladiatorName} +3 DEF");
            }

            if (HasRelic(RelicType.RuzgarAyak))
            {
                g.data.speed += 2;
                Debug.Log($"[Relic] RuzgarAyak: {g.data.gladiatorName} +2 SPD");
            }
        }
    }

    // ── DEVAMLI ETKİLİ BUFFLAR ──────────────────────────────────────────────
    // Bu metodlar ilgili Manager'lardan her hesaplamada çağrılacak.

    /// <summary>SupplyManager.ConsumeDailyFood() içinde kullan: dailyCost *= GetFoodCostMultiplier()</summary>
    public float GetFoodCostMultiplier()
    {
        float mult = 1f;
        if (HasRelic(RelicType.KutluMide)) mult *= 0.80f; // -%20
        if (HasRelic(RelicType.TuzluEt) &&
            SeasonManager.Instance != null &&
            SeasonManager.Instance.currentPhase == 3)
            mult -= 0.05f; // kışta ek -%5
        return Mathf.Max(0.1f, mult);
    }

    /// <summary>TrainingArea3D.cost'u hesaplarken çarp: finalCost = base * GetTrainingCostMultiplier()</summary>
    public float GetTrainingCostMultiplier()
    {
        float mult = 1f;
        if (HasRelic(RelicType.CimriDede))  mult *= 0.80f; // -%20
        return mult;
    }

    /// <summary>TrainingArea3D.timeCost'u hesaplarken: finalDays = Max(1, base + GetTrainingDayBonus())</summary>
    public int GetTrainingDayBonus()
    {
        int bonus = 0;
        if (HasRelic(RelicType.DemirDovucu)) bonus -= 1; // 1 gün az
        return bonus;
    }

    /// <summary>GladiatorTraining finalAmount += GetTrainingStatBonus()</summary>
    public int GetTrainingStatBonus()
    {
        int bonus = 0;
        if (HasRelic(RelicType.UstalıkOcağı)) bonus += 1;
        return bonus;
    }

    /// <summary>HealingSpot.cost'u hesaplarken çarp</summary>
    public float GetHealingCostMultiplier()
    {
        float mult = 1f;
        if (HasRelic(RelicType.OldurumUstası)) mult *= 0.70f; // -%30
        return mult;
    }

    /// <summary>ExpeditionManager.AddLoot() içinde altın ödülüne çarp</summary>
    public float GetLootGoldMultiplier()
    {
        float mult = 1f;
        if (HasRelic(RelicType.BereketliYol))  mult *= 1.25f; // +%25
        if (HasRelic(RelicType.HazineNefesi))  mult *= 1.10f; // +%10
        return mult;
    }

    /// <summary>ExpeditionManager.SendLootViaCaravan() komisyonu hesaplarken kullan</summary>
    public float GetCaravanCommissionRate()
    {
        float rate = 0.10f; // normal %10
        if (HasRelic(RelicType.HalkınGözü)) rate = 0.05f; // %5'e düşer
        return rate;
    }

    /// <summary>BattleManager'da savaş kaybı moral düşüşüne çarp</summary>
    public float GetMoraleLossMultiplier()
    {
        float mult = 1f;
        if (HasRelic(RelicType.CesurYurek)) mult *= 0.50f; // -%50
        return mult;
    }

    /// <summary>Her 5 encounter relic hakkı: EfsaneviHikaye varsa 2 hak ver</summary>
    public int GetRelicPicksPerMilestone()
    {
        return HasRelic(RelicType.EfsaneviHikaye) ? 2 : 1;
    }

    // ── GÖRÜNEN İSİMLER ─────────────────────────────────────────────────────
    public string GetRelicDisplayName(RelicType type) => type switch
    {
        RelicType.ZenginAta       => "Zengin Ata",
        RelicType.BereketliYol    => "Bereketli Yol",
        RelicType.CimriDede       => "Cimri Dede",
        RelicType.HazineNefesi    => "Hazine Nefesi)",
        RelicType.KutluMide       => "Kutlu Mide)",
        RelicType.AvciBaba        => "Avcı Baba",
        RelicType.TuzluEt         => "Tuzlu Et",
        RelicType.GaziKani        => "Gazi Kanı",
        RelicType.DemirBilek      => "Demir Bilek",
        RelicType.CelikZirh       => "Çelik Zırh (Askerler +3 Savunma)",
        RelicType.RuzgarAyak      => "Rüzgar Ayak (Askerler +2 Hız)",
        RelicType.DemirDovucu     => "Demircinin Mirası (Eğitim -1 Gün)",
        RelicType.UstalıkOcağı    => "Ustalık Ocağı (Eğitim +1 Stat Bonus)",
        RelicType.OldurumUstası   => "Lokman Hekim",
        RelicType.CesurYurek      => "Cesur Yürek (Savaş Kaybı Moral -%50)",
        RelicType.OnurluKan       => "Onurlu Kan (+10 Başlangıç İtibarı)",
        RelicType.HalkınGözü      => "Yol Arkadaşı",
        RelicType.NasipliYolcu    => "Nasipli Yolcu",
        RelicType.EfsaneviHikaye  => "Efsanevi Hikaye (Her mirastanda 2 hak)",
        _                          => type.ToString()
    };

    public string GetRelicDescription(RelicType type) => type switch
    {
        RelicType.ZenginAta       => "Dedenden kalan altın kese, her seferin başında kesenin ağzını açar.",
        RelicType.BereketliYol    => "Atanın bastığı yerde altın biter derler. Boş söz değilmiş.",
        RelicType.CimriDede       => "Dede akçesi olmasına rağmen paspal giyinir eksik ödeme yapardı.",
        RelicType.HazineNefesi    => "Ganimetlerin üstüne yatmadan önce bir nefes üfle. Bereketlenir.",
        RelicType.KutluMide       => "Atanın mirasından gelen mide — azla tok, çokla şükür.",
        RelicType.AvciBaba        => "Babanın sadakası: yola çıkmadan önce ambarı dolu bırakmış.",
        RelicType.TuzluEt         => "Kışa hazırlık sırrı: tuzla, bekle, hayatta kal.",
        RelicType.GaziKani         => "Gazi soyundan gelenlerin kanı daha kolay dinmez.",
        RelicType.DemirBilek      => "Atanın örsü, neslinden gelenlerin kollarına işlemiş.",
        RelicType.CelikZirh       => "Askerlerine çelikten bir zırh verir",
        RelicType.RuzgarAyak      => "Rüzgarın hızı bacaklara geçer, soydan soya.",
        RelicType.DemirDovucu     => "Ustanın eli çabuk — şakirtleri de öyle olur.",
        RelicType.UstalıkOcağı    => "Ateşte dövülen demir erimez, sertleşir.",
        RelicType.OldurumUstası   => "Şifacı babanın formülü: az ilaç, çok dua, doğru el.",
        RelicType.CesurYurek      => "Yenilgi yüzü kızartır, ama cesur yürek çabuk iyileşir.",
        RelicType.OnurluKan       => "Soyunun adı söylendiğinde başlar dik tutulur.",
        RelicType.HalkınGözü      => "Ata tüccar değil, dosttu — kervanlar ona saygıyla baktı.",
        RelicType.KaderYolu       => "Kader yolları açar — sadece bakmayı bilenler için.",
        RelicType.NasipliYolcu    => "Nasibin soyuna da bağlıdır. Atanınki seninle geliyor.",
        RelicType.EfsaneviHikaye  => "Destanın yolları arasında iki miras saklıdır.",
        _                          => ""
    };

    // ── YARDIMCI ────────────────────────────────────────────────────────────
    private Gladiator[] GetAllSoldiers()
    {
        return FindObjectsByType<Gladiator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }
}