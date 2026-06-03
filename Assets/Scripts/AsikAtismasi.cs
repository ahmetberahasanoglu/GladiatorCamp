using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Aşık Atışması — Güç Tabanlı Sistem
///
/// Rakip bir mısra söyler. Ekrandaki 3 şıkın HEPSI aynı kafiyeyi taşır
/// (hepsi "doğru") ama güç puanları farklıdır.
///
/// Sonuç:
///   Oyuncu puanı > Rakip puanı  → Tur Kazanıldı
///   Oyuncu puanı = Rakip puanı  → Berabere (can gitmez, tur sayılmaz)
///   Oyuncu puanı < Rakip puanı  → Can kaybı
///
/// Trait Bonusları (GetEffectivePower içinde):
///   Dindar   → isDini mısralarda +2
///   Yetenekli → isLong  mısralarda +1, ayrıca her kazanılan turda +1sn
///   Obur     → baseTime +2sn (tok kafası iyi çalışır)
///   Sıradan  → bonus yok
/// </summary>
public class AsikAtismasi : MonoBehaviour
{
    public static AsikAtismasi Instance;

    // ════════════════════════════════════════════════════════════════════
    //  VERİ YAPILARI
    // ════════════════════════════════════════════════════════════════════

    public enum RivalType { KoyOzani, GezginAsik, PirSultan }

    struct RivalProfile
    {
        public string name;
        public string introLine;
        public float  baseTime;
        public int    lives;
        public int    rivalBasePower;   // Rakibin seçeceği mısranın minimum gücü
        public int    goldReward;
        public int    repReward;
        public int    moraleReward;
        public int    repPenalty;
        public int    moralePenalty;
    }

    static readonly RivalProfile[] Rivals = new RivalProfile[]
    {
        new RivalProfile
        {
            name           = "Köy Ozanı Veli",
            introLine      = "\"Haydi bakalım delikanlı, sazıma karşılık ver!\"",
            baseTime       = 12f,
            lives          = 3,
            rivalBasePower = 4,   // Düşük güç — yenilmesi kolay
            goldReward     = 150,
            repReward      = 10,
            moraleReward   = 20,
            repPenalty     = -10,
            moralePenalty  = -10
        },
        new RivalProfile
        {
            name           = "Gezgin Aşık Karaçalı",
            introLine      = "\"Bin köy gezdim, bin dil bildim. Bakalım seninki nereye kadar!\"",
            baseTime       = 8f,
            lives          = 2,
            rivalBasePower = 6,
            goldReward     = 350,
            repReward      = 25,
            moraleReward   = 30,
            repPenalty     = -20,
            moralePenalty  = -20
        },
        new RivalProfile
        {
            name           = "Pir Sultan",
            introLine      = "\"Dara düştüm dağlar dayanmaz bana... Senin gönlün dayanır mı?\"",
            baseTime       = 5f,
            lives          = 1,
            rivalBasePower = 9,   // Yüksek güç — sadece güçlü mısrayla yenilir
            goldReward     = 700,
            repReward      = 50,
            moraleReward   = 50,
            repPenalty     = -40,
            moralePenalty  = -30
        }
    };

    // ── Mısra Veri Yapısı ─────────────────────────────────────────────────
    struct Misra
    {
        public string text;
        public int    basePower;   // 1-10 arası ham güç
        public bool   isDini;      // Dindar trait bonusu için
        public bool   isLong;      // Uzun mısra = Yetenekli trait bonusu için
    }

    struct KafiyeGrubu
    {
        public string      rhymeKey;
        public string      rhymeEnding;
        public string      theme;
        public List<Misra> misralar;
    }

    List<KafiyeGrubu> _kafiyeHavuzu;

    // ── Oyun Durumu ───────────────────────────────────────────────────────
    int         _currentRound;
    int         _maxRounds;
    int         _playerLives;
    float       _currentTimeLimit;
    float       _timeLeft;
    RivalProfile _rival;
    Gladiator   _soldier;
    int         _rivalMisraPower;    // Bu turda rakibin söylediği mısranın gücü
    Coroutine   _timerCoroutine;
    bool        _nasipUsedThisRound;

    // Berabere turları — üst üste 2 berabere can götürür
    int         _drawStreak;

    void Awake()
    {
        Instance = this;
        BuildKafiyeHavuzu();
    }

    // ════════════════════════════════════════════════════════════════════
    //  BAŞLAT
    // ════════════════════════════════════════════════════════════════════
    public void StartAtisma(Gladiator soldier, RivalType rivalType)
    {
        _soldier    = soldier;
        _rival      = Rivals[(int)rivalType];
        _maxRounds  = rivalType == RivalType.KoyOzani ? 3
                    : rivalType == RivalType.GezginAsik ? 4 : 5;

        _currentRound = 0;
        _playerLives  = _rival.lives;
        _drawStreak   = 0;

        // Trait bazlı süre bonusu
        float speedBonus = Mathf.Min(5f, soldier.data.speed / 5f);
        float traitBonus = soldier.data.trait == SoldierTrait.Obur ? 2f : 0f;
        _currentTimeLimit = _rival.baseTime + speedBonus + traitBonus;

        // Giriş ekranı
        string traitNote = soldier.data.trait switch
        {
            SoldierTrait.Dindar    => "\n Dindar — dini mısralarda +2 güç",
            SoldierTrait.Yetenekli => "\n Yetenekli — uzun mısralarda +1 güç, kazanınca +1sn",
            SoldierTrait.Obur      => "\n Obur — tok kafa iyi düşünür (+2sn)",
            _                      => ""
        };

        MapEventManager.Instance.titleText.text = _rival.name + " ile Atışma";
        MapEventManager.Instance.descText.text  =
            $"{_rival.introLine}\n\n" +
            $"<b>{soldier.data.gladiatorName}</b>  Hız: {soldier.data.speed}  " +
            $"Süre: {_currentTimeLimit:F0}sn{traitNote}\n\n" +
            $"<size=85%>Tur: {_maxRounds}   Can: {_playerLives}   " +
            $"Ödül: {_rival.goldReward} Akçe\n\n" +
            $"Her şık aynı kafiyeyi taşır. En güçlü karşılığı seç!</size>";

        MapEventManager.Instance.ClearAllButtons();
        MapEventManager.Instance.CreateButton("Atışmayı Başlat!", () => PlayRound());
        MapEventManager.Instance.CreateButton("Vazgeç (-" + Mathf.Abs(_rival.repPenalty / 2) + " İtibar)", () =>
        {
            AddReward(0, _rival.repPenalty / 2);
            CampMoraleManager.Instance?.ChangeMorale(_rival.moralePenalty / 2);
            NotificationManager.Instance?.Show("Atışmadan kaçtın.", NotificationType.Warning);
            MapEventManager.Instance.ClosePanel();
        });
    }

    // ════════════════════════════════════════════════════════════════════
    //  TUR DÖNGÜSÜ
    // ════════════════════════════════════════════════════════════════════
    void PlayRound()
    {
        if (_currentRound >= _maxRounds) { WinGame(); return; }
        if (_playerLives  <= 0)          { LoseGame("Sözün bitti, nefesin tükendi."); return; }

        _nasipUsedThisRound = false;

        bool preferDini = _soldier.data.trait == SoldierTrait.Dindar;
        KafiyeGrubu group = PickGroup(preferDini);

        // ── Rakip mısrasını seç ──────────────────────────────────────────
        // Rakip kendi güç seviyesine yakın bir mısra seçer
        List<Misra> shuffled = new List<Misra>(group.misralar);
        Shuffle(shuffled);

        // Rakibin gücüne yakın mısra bul (±2 tolerans)
        Misra rakipMisra = shuffled[0];
        foreach (var m in shuffled)
        {
            if (Mathf.Abs(m.basePower - _rival.rivalBasePower) <
                Mathf.Abs(rakipMisra.basePower - _rival.rivalBasePower))
                rakipMisra = m;
        }
        _rivalMisraPower = rakipMisra.basePower;

        // ── Oyuncu seçenekleri — aynı kafiyeden, farklı güçler ───────────
        List<Misra> pool = new List<Misra>(group.misralar);
        pool.RemoveAll(m => m.text == rakipMisra.text);
        Shuffle(pool);

        // 3 farklı güç seviyesi: rakipten güçlü, eşit, zayıf
        List<Misra> options = new List<Misra>();

        // Güçlü seçenek (mutlaka olsun)
        Misra? strongOption  = null;
        Misra? equalOption   = null;
        Misra? weakOption    = null;

        foreach (var m in pool)
        {
            int ep = GetEffectivePower(m);
            if (ep > _rivalMisraPower && strongOption == null)  strongOption = m;
            else if (ep == _rivalMisraPower && equalOption == null) equalOption = m;
            else if (ep < _rivalMisraPower && weakOption == null)   weakOption  = m;
        }

        // Havuz yeterli değilse kalan mısralarla doldur
        foreach (var m in pool)
        {
            if (options.Count >= 3) break;
            if (!options.Exists(o => o.text == m.text)) options.Add(m);
        }

        // Önce belirlenen tier'ları ekle, yoksa pool'dan al
        options.Clear();
        if (strongOption.HasValue) options.Add(strongOption.Value);
        if (equalOption.HasValue  && options.Count < 3) options.Add(equalOption.Value);
        if (weakOption.HasValue   && options.Count < 3) options.Add(weakOption.Value);

        // Hâlâ 3'e ulaşamadıysak havuzdan ekle
        foreach (var m in pool)
        {
            if (options.Count >= 3) break;
            if (!options.Exists(o => o.text == m.text)) options.Add(m);
        }

        Shuffle(options);

        // ── Ekran metni ───────────────────────────────────────────────────
        string hearts    = new string('♥', _playerLives) +
                           new string('♡', _rival.lives - _playerLives);
        string timeColor = _timeLeft <= 3f ? "red" : "white";

        MapEventManager.Instance.titleText.text =
            $"{_rival.name}  |  Tur {_currentRound + 1}/{_maxRounds}  |  {hearts}";

        MapEventManager.Instance.descText.text =
            $"{_rival.name} dedi:\n\n" +
            $"<i>\"{rakipMisra.text}\"</i>\n\n" +
            $"Kafiye: -{group.rhymeEnding}   " +
            $"Tema: {group.theme}\n\n" +
            $"<size=85%>Rakip Gücü: {_rivalMisraPower}   " +
            $"Tüm şıklar bu kafiyeyi taşıyor — en güçlü karşılığı seç!</size>";

        MapEventManager.Instance.ClearAllButtons();

        foreach (var opt in options)
        {
            Misra captured = opt;
            int effectivePower = GetEffectivePower(captured);

            // Güç göstergesi: oyuncuya ipucu ver ama sayı gösterme
            string powerHint = effectivePower > _rivalMisraPower + 2 ? " (Güçlü)"
                             : effectivePower > _rivalMisraPower     ? " (Orta)"
                             : effectivePower == _rivalMisraPower    ? " (Zayıf)"
                             :                                          " (Çok Zayıf)";

            MapEventManager.Instance.CreateButton(
                $"\"{captured.text}\"{powerHint}",
                () => OnAnswerSelected(captured));
        }

        // ── Nasip Butonu ───────────────────────────────────────────────────
        int nasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
        if (nasip >= 2 && !_nasipUsedThisRound)
        {
            MapEventManager.Instance.CreateButton($"Nasibine Sığın! (-2 Nasip = +3sn)", () =>
            {
                _nasipUsedThisRound = true;
                _timeLeft += 3f;
                NasipManager.Instance?.SpendNasip(2);
                NotificationManager.Instance?.Show("Allah yardımcın olsun! +3sn kazandın.", NotificationType.Success);
            });
        }

        // ── Zamanlayıcı ────────────────────────────────────────────────────
        _timeLeft = _currentTimeLimit;
        if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
        _timerCoroutine = StartCoroutine(TimerRoutine());
    }

    // ════════════════════════════════════════════════════════════════════
    //  TRAIT BAZLI GÜÇ HESABI
    // ════════════════════════════════════════════════════════════════════
    int GetEffectivePower(Misra m)
    {
        int power = m.basePower;

        if (_soldier == null || _soldier.data == null) return power;

        switch (_soldier.data.trait)
        {
            case SoldierTrait.Dindar:
                if (m.isDini) power += 2;
                break;

            case SoldierTrait.Yetenekli:
                if (m.isLong) power += 1;
                break;

            // Obur ve Sıradan: mısra gücünü değiştirmez
            // (Obur bonusu zaten süre üzerinde uygulandı)
        }

        return power;
    }

    // ════════════════════════════════════════════════════════════════════
    //  SEÇIM
    // ════════════════════════════════════════════════════════════════════
    void OnAnswerSelected(Misra chosen)
    {
        if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);

        int effectivePower = GetEffectivePower(chosen);

        if (effectivePower > _rivalMisraPower)
        {
            // ── Kazandın ────────────────────────────────────────────────
            AudioManager.Instance?.PlayClick();
            _currentRound++;
            _drawStreak = 0;

            // Yetenekli: her kazanılan tur +1sn
            if (_soldier.data.trait == SoldierTrait.Yetenekli)
                _currentTimeLimit = Mathf.Min(_currentTimeLimit + 1f, _rival.baseTime + 10f);

            string feedback = effectivePower >= _rivalMisraPower + 3
                ? "MUHTEŞEM! Rakip sustu."
                : "Güzel! Rakibi geride bıraktın.";

            ShowRoundResult(feedback, () => PlayRound());
        }
        else if (effectivePower == _rivalMisraPower)
        {
            // ── Berabere ────────────────────────────────────────────────
            _drawStreak++;

            if (_drawStreak >= 2)
            {
                // Üst üste 2 berabere → can kaybı
                _drawStreak = 0;
                _playerLives--;
                ShowRoundResult(
                    "İki kez berabere! Ahali sıkıldı. Can kaybettin.",
                    () => { if (_playerLives > 0) PlayRound(); else LoseGame("Ahali ilgi kaybetti."); });
            }
            else
            {
                ShowRoundResult(
                    "Berabere! Tur tekrarlanıyor...",
                    () => PlayRound());
            }
        }
        else
        {
            // ── Kaybettin ───────────────────────────────────────────────
            AudioManager.Instance?.PlayError();
            _playerLives--;
            _drawStreak = 0;
            _currentTimeLimit = Mathf.Max(3f, _currentTimeLimit - 0.5f);

            string feedback = $"Rakip daha güçlüydü! ({effectivePower} vs {_rivalMisraPower})";
            ShowRoundResult(feedback, () => { if (_playerLives > 0) PlayRound(); else LoseGame("Sözün tükendi."); });
        }
    }

    // ── Kısa ara ekranı — sonuç göster, devam et ─────────────────────────
    void ShowRoundResult(string message, System.Action onContinue)
    {
        MapEventManager.Instance.ClearAllButtons();

        string hearts = new string('♥', _playerLives) +
                        new string('♡', _rival.lives - _playerLives);

        MapEventManager.Instance.descText.text =
            $"{message}\n\n{hearts}   Tur {_currentRound}/{_maxRounds}";

        MapEventManager.Instance.CreateButton("Devam →", () => onContinue?.Invoke());
    }

    // ════════════════════════════════════════════════════════════════════
    //  ZAMANLAYICI
    // ════════════════════════════════════════════════════════════════════
    IEnumerator TimerRoutine()
    {
        while (_timeLeft > 0)
        {
            string hearts    = new string('♥', _playerLives) +
                               new string('♡', _rival.lives - _playerLives);
            string timeColor = _timeLeft <= 3f ? "red" : _timeLeft <= 5f ? "orange" : "white";

            MapEventManager.Instance.titleText.text =
                $"{_rival.name}  |  Tur {_currentRound + 1}/{_maxRounds}  |  {hearts}  " +
                $"{_timeLeft:F1}sn";

            yield return new WaitForSeconds(0.1f);
            _timeLeft -= 0.1f;
        }

        _playerLives--;
        if (_playerLives > 0)
        {
            _currentTimeLimit = Mathf.Max(3f, _currentTimeLimit - 0.5f);
            ShowRoundResult("Süre bitti! Can kaybettin.", () => PlayRound());
        }
        else LoseGame("Dilin tutuldu, süren doldu.");
    }

    // ════════════════════════════════════════════════════════════════════
    //  KAZANMA / KAYBETME
    // ════════════════════════════════════════════════════════════════════
    void WinGame()
    {
        MapEventManager.Instance.ClearAllButtons();
        MapEventManager.Instance.titleText.text = "ATIŞMA KAZANILDI!";
        MapEventManager.Instance.descText.text  =
            $"Sözlerin kılıç gibi kesti! {_rival.name} önünde eğildi.\n\n" +
            $"+{_rival.goldReward} Akçe (Çantaya)  " +
            $"+{_rival.repReward} İtibar (Çantaya)  " +
            $"+{_rival.moraleReward} Moral";

        AddReward(_rival.goldReward, _rival.repReward);
        CampMoraleManager.Instance?.ChangeMorale(_rival.moraleReward);
        AudioManager.Instance?.PlayCheer();
        MapEventManager.Instance.CreateButton("Altınları Al ve Çekil", () => MapEventManager.Instance.ClosePanel());
    }

    void LoseGame(string reason)
    {
        MapEventManager.Instance.ClearAllButtons();
        MapEventManager.Instance.titleText.text = "ATIŞMA KAYBEDİLDİ";
        MapEventManager.Instance.descText.text  =
            $"HÜSRAN!\n{reason}\n\n" +
            $"{_rival.repPenalty} İtibar   {_rival.moralePenalty} Moral";

        AddReward(0, _rival.repPenalty);
        CampMoraleManager.Instance?.ChangeMorale(_rival.moralePenalty);
        MapEventManager.Instance.CreateButton("Utanç İçinde Ayrıl", () => MapEventManager.Instance.ClosePanel());
    }

    // ════════════════════════════════════════════════════════════════════
    //  YARDIMCILAR
    // ════════════════════════════════════════════════════════════════════
    void AddReward(int gold, int rep)
    {
        if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
            ExpeditionManager.Instance.AddLoot(gold, rep);
        else
        {
            if (gold > 0) MoneyManager.Instance?.Add(gold);
            else if (gold < 0) MoneyManager.Instance?.Spend(Mathf.Abs(gold));
            if (rep != 0) ReputationManager.Instance?.ChangeReputation(rep);
        }
    }

    public static RivalType PickRivalType()
    {
        int encounter = ExpeditionManager.Instance != null
            ? ExpeditionManager.Instance.currentEncounterCount : 0;

        if (encounter >= 8) return Random.value < 0.4f ? RivalType.PirSultan : RivalType.GezginAsik;
        if (encounter >= 4) return Random.value < 0.5f ? RivalType.GezginAsik : RivalType.KoyOzani;
        return RivalType.KoyOzani;
    }

    KafiyeGrubu PickGroup(bool preferDini)
    {
        // Dindar asker: %65 ihtimalle dini gruptan seç
        if (preferDini)
        {
            var dini = _kafiyeHavuzu.FindAll(g =>
                g.misralar.Exists(m => m.isDini));
            if (dini.Count > 0 && Random.value < 0.65f)
                return dini[Random.Range(0, dini.Count)];
        }
        return _kafiyeHavuzu[Random.Range(0, _kafiyeHavuzu.Count)];
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  KAFİYE HAVUZU
    //  Her mısra: text | basePower(1-10) | isDini | isLong
    //  isLong = mısra 8+ kelime içeriyorsa true
    // ════════════════════════════════════════════════════════════════════
    void BuildKafiyeHavuzu()
    {
        _kafiyeHavuzu = new List<KafiyeGrubu>
        {
            // ── -AŞAR kafiyesi ─────────────────────────────────────────
            new KafiyeGrubu { rhymeKey = "ASAR", rhymeEnding = "aşar", theme = "Yiğitlik",
            misralar = new List<Misra> {
                new Misra { text = "At koşar meydanı aşar / Yiğit düşmana taşar",                            basePower = 4, isDini = false, isLong = false },
                new Misra { text = "Atım dağları aşar / Düşman önünde şaşar",                                basePower = 5, isDini = false, isLong = false },
                new Misra { text = "Kartal kayaları aşar / Sesi dağlara taşar",                              basePower = 6, isDini = false, isLong = false },
                new Misra { text = "Yiğit her engeli aşar / Ünü dört bir yana taşar",                        basePower = 7, isDini = false, isLong = false },
                new Misra { text = "Sel gelir bendini aşar / Sular dağı ovayı taşar",                        basePower = 8, isDini = false, isLong = true  },
                new Misra { text = "Güneş her karanlığı aşar / Işığı zulmün üstüne taşar",                   basePower = 9, isDini = false, isLong = true  },
                new Misra { text = "Hak yolundaki er her çileyi aşar / Sevabı arşa dek taşar",               basePower = 10, isDini = true, isLong = true  },
            }},

            // ── -AKAR kafiyesi ─────────────────────────────────────────
            new KafiyeGrubu { rhymeKey = "AKAR", rhymeEnding = "akar/bakar", theme = "Özlem",
            misralar = new List<Misra> {
                new Misra { text = "Çeşme durulmaz akar / Susayan yüzüne bakar",                             basePower = 4, isDini = false, isLong = false },
                new Misra { text = "Dağdan soğuk su akar / Gözlerim yola bakar",                             basePower = 5, isDini = false, isLong = false },
                new Misra { text = "Sular menzile akar / Gönlüm yâre doğru bakar",                          basePower = 6, isDini = false, isLong = false },
                new Misra { text = "Gözyaşı yanaşa akar / Yürek gurbet yoluna bakar",                       basePower = 7, isDini = false, isLong = false },
                new Misra { text = "Zaman sel gibi akar / Ömür geçer gider gözlerim arkasına bakar",         basePower = 8, isDini = false, isLong = true  },
                new Misra { text = "Hak'tan gelen rahmet akar / Mümin o nura gözü dolu bakar",               basePower = 9, isDini = true,  isLong = true  },
                new Misra { text = "Kâinat Hakk'ın emriyle akar / Arif olan o sırrı görür bakar",            basePower = 10, isDini = true, isLong = true  },
            }},

            // ── -İÇTİM / -GEÇTİM kafiyesi ─────────────────────────────
            new KafiyeGrubu { rhymeKey = "ICTIM", rhymeEnding = "içtim/geçtim", theme = "Gurbet",
            misralar = new List<Misra> {
                new Misra { text = "Acı şerbeti içtim / Kaderimden el geçtim",                               basePower = 4, isDini = false, isLong = false },
                new Misra { text = "Dost elinden su içtim / Yollara düşüp geçtim",                           basePower = 5, isDini = false, isLong = false },
                new Misra { text = "Hasret bardağın içtim / Yârsız dağları geçtim",                          basePower = 6, isDini = false, isLong = false },
                new Misra { text = "Gurbet elinin acısın içtim / Bin bir çileyle dağları geçtim",            basePower = 7, isDini = false, isLong = true  },
                new Misra { text = "Ayrılık denen zehri içtim / Sılam için dağ dağ geçtim",                  basePower = 8, isDini = false, isLong = true  },
                new Misra { text = "Sevda ile dolu coşkun nehri içtim / Aşkın ateşiyle dağ taş geçtim",      basePower = 9, isDini = false, isLong = true  },
                new Misra { text = "Hak aşkının şarabından içtim / Nefsi geçtim arındım geçtim",             basePower = 10, isDini = true, isLong = true  },
            }},

            // ── -OLMAZ kafiyesi ────────────────────────────────────────
            new KafiyeGrubu { rhymeKey = "OLMAZ", rhymeEnding = "olmaz/almaz", theme = "Hikmet",
            misralar = new List<Misra> {
                new Misra { text = "Kartal ova uçmaz / Aslan düze inmez",                                    basePower = 4, isDini = false, isLong = false },
                new Misra { text = "Güneş balçıkla sıvanmaz / Mert kötü söze dayanmaz",                     basePower = 5, isDini = false, isLong = false },
                new Misra { text = "Ateş olmadan duman çıkmaz / Dert olmadan göz yaşı akmaz",               basePower = 6, isDini = false, isLong = false },
                new Misra { text = "Doğru söz yıkılmaz / Haksızlığa boyun eğmez mert olan sinmez",          basePower = 7, isDini = false, isLong = true  },
                new Misra { text = "Kılıç dövülmeden kesilmez / Yiğit sınanmadan gerçek merd olduğu bilinmez", basePower = 8, isDini = false, isLong = true },
                new Misra { text = "Ocak yanmadan ev ısınmaz / Yurt sevilmeden hür olunmaz er hür kalmaz",   basePower = 9, isDini = false, isLong = true  },
                new Misra { text = "Hakk'a yönelen kalp kırılmaz / Sabır ile yürüyen yoldan geri kalmaz",    basePower = 10, isDini = true, isLong = true  },
            }},

            // ── -AÇIKTIR kafiyesi (Dini ağırlıklı) ────────────────────
            new KafiyeGrubu { rhymeKey = "ACIKTIR", rhymeEnding = "açıktır", theme = "İman",
            misralar = new List<Misra> {
                new Misra { text = "Dua edenin önü / Her an Allah'a açıktır",                                basePower = 4, isDini = true,  isLong = false },
                new Misra { text = "Hak yolunda yürüyenin / Kapısı daima açıktır",                          basePower = 5, isDini = true,  isLong = false },
                new Misra { text = "Tevbe edenin gönlü / Rahmet'e daima açıktır",                           basePower = 6, isDini = true,  isLong = false },
                new Misra { text = "İman edenin yolu / Cennete doğru her dem açıktır",                      basePower = 7, isDini = true,  isLong = false },
                new Misra { text = "Sabır ile yürüyenin / Sonu nura açık sonsuza açıktır",                  basePower = 8, isDini = true,  isLong = true  },
                new Misra { text = "Zikir ile dolan dilin / Allah'a yolu hiç kapanmaz daim açıktır",        basePower = 9, isDini = true,  isLong = true  },
                new Misra { text = "Hakk'ı bilen aşığın / Hem dünyası hem ahireti her dem açıktır",         basePower = 10, isDini = true, isLong = true  },
            }},

            // ── -ALDANMAZ kafiyesi (Dini ağırlıklı) ───────────────────
            new KafiyeGrubu { rhymeKey = "ALDANMAZ", rhymeEnding = "aldanmaz", theme = "Takva",
            misralar = new List<Misra> {
                new Misra { text = "Cahil dünya malına aldanmaz / Aklı olan bu oyuna aldanmaz",              basePower = 4, isDini = false, isLong = false },
                new Misra { text = "Derviş olan gönül verir / Dünya malına aldanmaz",                       basePower = 5, isDini = true,  isLong = false },
                new Misra { text = "Gerçek mümin sabr eder / Nefsin oyununa aldanmaz",                      basePower = 6, isDini = true,  isLong = false },
                new Misra { text = "Kul hakkına girmeyen / Gösteriş ile süse aldanmaz",                     basePower = 7, isDini = true,  isLong = false },
                new Misra { text = "Hak aşığı yol yürür / Dağ taş onu tutmaz şeytana aldanmaz",             basePower = 8, isDini = true,  isLong = true  },
                new Misra { text = "Ahiret yolcusu er / Fani malına mülküne asla aldanmaz",                 basePower = 9, isDini = true,  isLong = true  },
                new Misra { text = "Hakk'ı bilen can hiç / Batıl yola sapıtmaz dünya süsüne aldanmaz",      basePower = 10, isDini = true, isLong = true  },
            }},

            // ── -GİRER kafiyesi ────────────────────────────────────────
            new KafiyeGrubu { rhymeKey = "GIRER", rhymeEnding = "girer", theme = "Kahramanlık",
            misralar = new List<Misra> {
                new Misra { text = "Davul çalınır köyde / Düğüne herkes girer",                              basePower = 4, isDini = false, isLong = false },
                new Misra { text = "Kale kapısı açılır / Yiğit içine girer",                                basePower = 5, isDini = false, isLong = false },
                new Misra { text = "Meydan kurulur düzlükte / Er alanlara girer",                           basePower = 6, isDini = false, isLong = false },
                new Misra { text = "Savaş meydanı açılır / Kılıç kınından çıkar er içine girer",            basePower = 7, isDini = false, isLong = true  },
                new Misra { text = "Bayrak dalgalanır yüksek / Er gözü dönmüş meydana girer",               basePower = 8, isDini = false, isLong = true  },
                new Misra { text = "Kargı saplandı toprağa er şehadet dileyerek meydana girer",             basePower = 9, isDini = false, isLong = true  },
                new Misra { text = "Hak yolunda şehit olmak için er coşkuyla meydana girer",                 basePower = 10, isDini = true, isLong = true  },
            }},
        };
    }
}
