using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public enum BattleState { Idle, Fighting, Won, Lost }
public enum BattleEnvironment { Forest, Cave, Tower, Winter }

[System.Serializable]
public class EnvironmentProfile
{
    public string profileName;
    public BattleEnvironment envType;

    [Header("Objeler")]
    public GameObject environmentProps;
    public GameObject volumeObject;

    [Header("Işıklandırma")]
    public Color sunColor = Color.white;
    public float sunIntensity = 1.0f;
    public bool enableShadows = true;
}

public class BattleManager : MonoBehaviour
{
    [Header("Arena Ortamları")]
    public GameObject forestEnv;
    public GameObject caveEnv;
    public GameObject towerEnv;
    public GameObject winterEnv;

    [Header("Atmosfer ve Ortam")]
    public Light mainDirectionalLight;
    public List<EnvironmentProfile> environments;

    public static BattleManager Instance;

    [Header("Kamp Dizilimi")]
    public Transform campStandingPoint;

    [Header("Gerekli Referanslar")]
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;
    public Camera mainCamera;
    public GameObject enemyPrefab;

    [Header("Boss Savaşı")]
    public GameObject bossPrefab;

    [Header("Kamera Pozisyonları")]
    public Transform campCameraPos;
    public Transform battleCameraPos;

    [Header("Durum")]
    public BattleState state = BattleState.Idle;

    [Header("UI Panelleri")]
    public GameObject lootPanel;
    public GameObject defeatPanel;
    public GameObject bgPanel;
    public GameObject topPanel;
    public TextMeshProUGUI lootText;
    public TextMeshProUGUI defeatText;
    public GameObject skillPanel;

    [Header("Geri Çekilme")]
    public Button retreatButton;
    public int baseRetreatPenalty = 30;

    // ── GERİ ÇEKİLME ONAY POPUP ──────────────────────────────────────────
    [Header("Geri Çekilme Onay Popup")]
    public GameObject retreatConfirmPanel;       // Popup'ın kendisi
    public TextMeshProUGUI retreatConfirmText;   // "X itibar kaybedersin" bilgisi
    public Button retreatConfirmYesBtn;          // "Evet, çekil"
    public Button retreatConfirmNoBtn;           // "Hayır, savaşmaya devam"
    // ─────────────────────────────────────────────────────────────────────

    private int _currentDifficulty = 1;
    private int _currentEnemyCount = 1;

    [Header("Düşman Tier Sistemi")]
    public EnemyTierConfig enemyTierConfig;
    private int _currentTier       = 1;
    private EnemyLoadout _currentLoadout = null;

    [Header("Sinematik Geçiş")]
    public CanvasGroup fadeGroup;

    [Header("Taktik Ekranı UI")]
    public SquadSelectionUIManager squadSelectionUI;

    private BattleEnvironment pendingEnv;

    [Header("Hedef İşaretleme")]
    public Transform currentFocusTarget;
    public GameObject focusMarkerPrefab;
    private GameObject _activeFocusMarker;

    [Header("Vahşi Hayvan")]
    public GameObject bearPrefab;


    [Header("RTS Seçim Sistemi")]
    public GladiatorAI currentlySelectedSoldier; // O an tıkladığımız asker
  
    private bool _isBearBattle = false;

    void Awake() => Instance = this;

    void Start()
    {
        if (skillPanel != null) skillPanel.SetActive(false);

        if (retreatButton != null)
            retreatButton.onClick.AddListener(OnRetreatButtonClicked);

        // Onay popup butonları
        if (retreatConfirmYesBtn != null)
            retreatConfirmYesBtn.onClick.AddListener(ExecuteRetreat);
        if (retreatConfirmNoBtn != null)
            retreatConfirmNoBtn.onClick.AddListener(CancelRetreat);

        if (retreatConfirmPanel != null)
            retreatConfirmPanel.SetActive(false);
    }

    void Update()
    {
        if (state == BattleState.Fighting) HandleMouseClick();
    }

    // ── GERİ ÇEKİLME — 1. ADIM: Zaman durdur, onay sor ─────────────────
    void OnRetreatButtonClicked()
    {
        if (state != BattleState.Fighting) return;

        // Önce kaç askerin sağ olduğunu hesapla ve cezayı göster
        int aliveSoldiers = CountAliveMySoldiers();
        int penalty       = CalculateRetreatPenalty(aliveSoldiers);

        // Popup metni
        if (retreatConfirmText != null)
        {
            retreatConfirmText.text =
                $"Geri çekilmek istiyor musun?\n" +
                $"Sağ asker sayısı: {aliveSoldiers}\n" +
                $"İtibar cezası: <color=red>-{penalty}</color>\n" +
                $"Geri çekilmek bu savaşı kaybettirir.";
        }

        // Zamanı dondur
        Time.timeScale = 0f;

        if (retreatConfirmPanel != null) retreatConfirmPanel.SetActive(true);
    }

    // ── GERİ ÇEKİLME — 2. ADIM: Onaylandı, yürüt ───────────────────────
    void ExecuteRetreat()
    {
        Time.timeScale = 1f;
        if (retreatConfirmPanel != null) retreatConfirmPanel.SetActive(false);

        if (state != BattleState.Fighting) return;

        int aliveSoldiers = CountAliveMySoldiers();
        int penalty       = CalculateRetreatPenalty(aliveSoldiers);

        state = BattleState.Lost;

        // Tüm birimleri durdur
        var allUnits = FindObjectsByType<GladiatorAI>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit.agent != null && unit.agent.isActiveAndEnabled) unit.agent.isStopped = true;
            if (unit.animator != null) unit.animator.SetBool("isRunning", false);
            unit.StopAllCoroutines();
        }

        if (skillPanel != null) skillPanel.SetActive(false);
        topPanel.SetActive(true);

        if (defeatText != null)
            defeatText.text =
                $"GERİ ÇEKİLME!\n\n" +
                $"Askerlerini son anda sahadan çektin.\n" +
                $"<color=red>-{penalty} İtibar (Korkaklık Bedeli)</color>";

        // Cezayı çantaya yansıt
        if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
            ExpeditionManager.Instance.AddLoot(0, -penalty);
        else
            ReputationManager.Instance?.ChangeReputation(-penalty);

        defeatPanel.SetActive(true);
    }

    // ── GERİ ÇEKİLME — İptal ────────────────────────────────────────────
    void CancelRetreat()
    {
        Time.timeScale = 1f;
        if (retreatConfirmPanel != null) retreatConfirmPanel.SetActive(false);
        // Savaş devam ediyor
    }

    // ── YARDIMCI: Ceza Hesabı ────────────────────────────────────────────
    // Az asker kaldıkça ceza azalır (çaresiz kaçış = daha az ayıp)
    // 3 asker sağken → tam ceza, 1 asker sağken → 1/3 ceza
    int CalculateRetreatPenalty(int aliveSoldiers)
    {
        float penaltyMultiplier = (float)aliveSoldiers / 3f;
        return Mathf.RoundToInt(baseRetreatPenalty * penaltyMultiplier);
    }

    int CountAliveMySoldiers()
    {
        int count = 0;
        var allUnits = FindObjectsByType<GladiatorAI>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
            if (unit.CompareTag("MySoldier") && !unit.isDead && unit.isInBattle)
                count++;
        return count;
    }

    // ── HEDEF İŞARETLEME ─────────────────────────────────────────────────
   // ════════════════════════════════════════════════════════════════════
    //  DİNAMİK RTS SEÇİM VE AKILLI FOCUS MARKER MOTORU
    // ════════════════════════════════════════════════════════════════════
    void HandleMouseClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // ── 1. KENDİ ASKERİMİZİ SEÇME ─────────────────────────────────
            if (hit.collider.CompareTag("MySoldier"))
            {
                GladiatorAI friendlyAI = hit.collider.GetComponentInParent<GladiatorAI>();
                if (friendlyAI != null && !friendlyAI.isDead)
                {
                    SelectSoldier(friendlyAI);
                }
                return;
            }

            // ── 2. DÜŞMANA EMİR VERME VEYA KÜRESEL ODAKLANMA ───────────────
            if (hit.collider.CompareTag("EnemySoldier"))
            {
                GladiatorAI enemyAI = hit.collider.GetComponentInParent<GladiatorAI>();
                if (enemyAI != null && !enemyAI.isDead)
                {
                    if (currentlySelectedSoldier != null)
                    {
                        // Tek bir askeri düşmana kilitliyoruz
                        CommandSingleSoldierToAttack(currentlySelectedSoldier, enemyAI.transform);
                    }
                    else
                    {
                        // Seçili asker yoksa tüm orduyu o düşmana odakla (Küresel Odak)
                        SetFocusTarget(hit.collider.transform);
                    }
                }
                return;
            }

            // ── 3. BOŞLUĞA TIKLANDIĞINDA SEÇİMLERİ TEMIZLE ────────────────
            if (!hit.collider.CompareTag("UI")) 
            {
                DeselectSoldier();
            }
        }
    }

    void SelectSoldier(GladiatorAI soldier)
    {
        currentlySelectedSoldier = soldier;

        // ── GÖRSEL İPTAL EDİLDİ ──
        // Başka bir sistem zaten halkayı yaktığı için buradaki Instantiate kodlarını tamamen temizledik.

        // Seçtiğimiz askerin halihazırda saldırdığı biri varsa marker'ı ona taşı
        if (soldier.target != null)
        {
            ApplyFocusMarkerVisual(soldier.target);
        }
        else if (currentFocusTarget == null) 
        {
            HideFocusMarkerVisual();
        }
        
        Debug.Log($"[RTS Sistem] {soldier.gameObject.name} mekanik olarak seçildi. Komut bekliyor.");
    }

    
    void CommandSingleSoldierToAttack(GladiatorAI soldier, Transform enemyTransform)
    {
        if (soldier == null || soldier.isDead || enemyTransform == null) return;

        soldier.forcedTarget = enemyTransform;
        soldier.target = enemyTransform;

        var agent = soldier.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            agent.SetDestination(enemyTransform.position);
        }

        if (DamageTextManager.Instance != null)
            DamageTextManager.Instance.ShowCustomText(soldier.transform.position, "HÜCUM!", Color.green);

        // ── YENİ: Tekil komut verildiğinde Focus Marker'ı anında o düşmanın tepesine dik! ──
        ApplyFocusMarkerVisual(enemyTransform);
    }

    void DeselectSoldier()
    {
        currentlySelectedSoldier = null;

        // ── GÖRSEL İPTAL EDİLDİ ──
        // Halka gizleme kodları silindi.

        if (currentFocusTarget == null)
        {
            HideFocusMarkerVisual();
        }
        else
        {
            ApplyFocusMarkerVisual(currentFocusTarget);
        }
    }

    public void SetFocusTarget(Transform enemyTransform)
    {
        // Tüm ordu tek bir düşmana odaklanıyor (Küresel Odak)
        currentFocusTarget = enemyTransform;

        // ── YENİ: Ortak fonksiyon ile marker'ı genel hedefe yerleştir ──
        ApplyFocusMarkerVisual(enemyTransform);

        GladiatorAI[] allUnits = FindObjectsByType<GladiatorAI>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit.CompareTag("MySoldier") && !unit.isDead)
            {
                unit.target = enemyTransform;
                var agent = unit.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null && agent.isActiveAndEnabled)
                {
                    agent.isStopped = false;
                    agent.SetDestination(enemyTransform.position);
                }
            }
        }
    }

    public void ClearFocusTarget()
    {
        currentFocusTarget = null;
        // Eğer o sırada seçili tekil bir askerimiz yoksa marker'ı tamamen söndür
        if (currentlySelectedSoldier == null)
        {
            HideFocusMarkerVisual();
        }
    }

    // ── GÖRSEL MOTOR: FOCUS MARKER YÖNETİMİ (MÜKERRER KOD ENGELLEYİCİ) ────
    private void ApplyFocusMarkerVisual(Transform targetEnemy)
    {
        if (focusMarkerPrefab == null || targetEnemy == null) return;

        if (_activeFocusMarker == null)
            _activeFocusMarker = Instantiate(focusMarkerPrefab);

        _activeFocusMarker.SetActive(true);
        _activeFocusMarker.transform.SetParent(targetEnemy);
        _activeFocusMarker.transform.localPosition = new Vector3(0, 0.1f, 0); // Karakterin duruşuna göre yükseklik ayarı
    }

    private void HideFocusMarkerVisual()
    {
        if (_activeFocusMarker != null)
        {
            _activeFocusMarker.SetActive(false);
            _activeFocusMarker.transform.SetParent(null);
        }
    }

    // ── SAVAŞ BAŞLATMA ───────────────────────────────────────────────────
   public void StartBattle(int enemyCount, int difficulty, BattleEnvironment envType, int tier = 1)
    {
        _currentEnemyCount = enemyCount;
        _currentDifficulty = difficulty;
        _currentTier       = tier;
        // _currentLoadout    = enemyTierConfig != null ? enemyTierConfig.GetRandomLoadout(tier) : null;
        
        pendingEnv         = envType;
        _isBearBattle      = false;

        MapManager.Instance?.HideMap();
        squadSelectionUI?.OpenPanel(false, enemyCount, difficulty);
    }

    public void StartBearBattle(int bearCount, int difficulty, BattleEnvironment envType)
    {
        _currentEnemyCount = bearCount;
        _currentDifficulty = difficulty;
        pendingEnv         = envType;
        _isBearBattle      = true;

        MapManager.Instance?.HideMap();
        squadSelectionUI?.OpenPanel(false, bearCount, difficulty);
    }

    public void StartBossBattle(int difficulty, BattleEnvironment envType)
    {
        _currentDifficulty = difficulty;
        _currentEnemyCount = 5;
        pendingEnv         = envType;

        MapManager.Instance?.HideMap();
        squadSelectionUI?.OpenPanel(true, 1, difficulty);
    }

    public void ExecuteBattleWithSquad(List<Gladiator> selectedSquad, bool isBossBattle)
    {
        ChangeEnvironment(pendingEnv);
        AudioManager.Instance.StartBattleAcoustics();
        StartCoroutine(CinematicTransitionRoutine(battleCameraPos));

        SpawnPlayerArmy(selectedSquad);

        if (isBossBattle)            SpawnBoss();
        else if (_isBearBattle)      SpawnGenericEnemyArray(bearPrefab, _currentEnemyCount);
        else                         SpawnGenericEnemyArray(enemyPrefab, _currentEnemyCount);

        state = BattleState.Fighting;
        if (skillPanel != null) skillPanel.SetActive(true);
        topPanel.SetActive(false);
        bgPanel.SetActive(false);
        if (BattleElementUI.Instance != null)
        {
            BattleElementUI.Instance.Show();
            BattleElementUI.Instance.Refresh(); // Askerler doğdu, hemen say!
        }
    }

    // ── SAVAŞ KONTROLÜ ───────────────────────────────────────────────────
    public void CheckBattleStatus()
    {
        if (state != BattleState.Fighting) return;

        var allUnits = FindObjectsOfType<GladiatorAI>();
        int livingMy = 0, livingEnemy = 0;
        foreach (var unit in allUnits)
        {
            if (!unit.isDead && unit.isInBattle)
            {
                if (unit.CompareTag("MySoldier"))    livingMy++;
                else if (unit.CompareTag("EnemySoldier")) livingEnemy++;
            }
        }

        if (livingEnemy <= 0)                               EndBattle(true);
        else if (livingMy <= 0)
        {
            EndBattle(false);
            MapManager.Instance?.RetreatToPreviousNode();
        }
    }

    // ── SAVAŞ SONU — RELİC ÇARPANLARI BURADA UYGULANIR ──────────────────
    public void EndBattle(bool isVictory)
    {
        state = isVictory ? BattleState.Won : BattleState.Lost;

        if (skillPanel != null) skillPanel.SetActive(false);
        topPanel.SetActive(true);


        if (isVictory)
        {
            // --- Relic çarpanını ödüle uygula ---
            float lootMult = MetaProgressionManager.Instance != null
                ? MetaProgressionManager.Instance.GetLootGoldMultiplier()
                : 1f;

            int goldReward   = Mathf.RoundToInt(_currentEnemyCount * 50 * _currentDifficulty * lootMult);
            int foodReward   = _currentEnemyCount * 10;
            int moraleReward = 10 + (_currentDifficulty * 5);
            int repReward    = 5 * _currentDifficulty;
            string nushaMetni = "";
           // ── BATTLEMANAGER.CS - ENDBATTLE FONKSİYONU İÇİ ──
            if ((pendingEnv == BattleEnvironment.Cave || _currentDifficulty >= 5) && MapEventManager.Instance != null && MapEventManager.Instance.nadirFermanlar.Count > 0)
            {
                ItemData dusenFerman = MapEventManager.Instance.nadirFermanlar[Random.Range(0, MapEventManager.Instance.nadirFermanlar.Count)];
                
                // ── DEĞİŞİKLİK: Çantaya değil, doğrudan Komutan Deposuna ekle! ──
                if (CommanderStorage.Instance != null)
                    CommanderStorage.Instance.AddNusha(dusenFerman);
                else
                    ExpeditionManager.Instance.tempItems.Add(dusenFerman); // Fallback
                
                nushaMetni = $"\nDüşman Komutanından düştü: {dusenFerman.itemName}";
            }
            // Normal savaşlarda çok düşük ihtimalle (%10) yaygın nüsha düşsün
            else if (Random.Range(0, 100) < 10 && MapEventManager.Instance != null && MapEventManager.Instance.yayginNushalar.Count > 0)
            {
                ItemData dusenNusha = MapEventManager.Instance.yayginNushalar[Random.Range(0, MapEventManager.Instance.yayginNushalar.Count)];
                
                // ── DEĞİŞİKLİK: Çantaya değil, doğrudan Komutan Deposuna ekle! ──
                if (CommanderStorage.Instance != null)
                    CommanderStorage.Instance.AddNusha(dusenNusha);
                else
                    ExpeditionManager.Instance.tempItems.Add(dusenNusha); // Fallback
                
                nushaMetni = $"\nCesetlerin arasında bulundu: {dusenNusha.itemName}";
            }

            if (lootText != null)
                lootText.text =
                    $"ZAFER!\n\n" +
                    $"<color=yellow>+{goldReward} Akçe (Çantaya)</color>\n" +
                    $"<color=green>+{repReward} İtibar (Çantaya)</color>\n" +
                    $"+{foodReward} Erzak\n+{moraleReward} Moral" + nushaMetni+
                    (lootMult > 1f ? $"\n<size=75%> Bereketli Yol bonusu aktif</size>" : "");

            if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
                ExpeditionManager.Instance.AddLoot(goldReward, repReward);
            else
            {
                MoneyManager.Instance?.Add(goldReward);
                ReputationManager.Instance?.ChangeReputation(repReward);
            }

            CampMoraleManager.Instance?.ChangeMorale(moraleReward);
            SupplyManager.Instance?.BuyFood(foodReward);
            lootPanel.SetActive(true);
        }
        else
        {
            // --- Relic çarpanını moral cezasına uygula ---
            float moraleMult = MetaProgressionManager.Instance != null
                ? MetaProgressionManager.Instance.GetMoraleLossMultiplier()
                : 1f;

            int moralePenalty = Mathf.RoundToInt((-20 - (_currentDifficulty * 5)) * moraleMult);
            int repPenalty    = -10;

            if (defeatText != null)
                defeatText.text =
                    $"AĞIR YENİLGİ...\n\nOtağ yasa boğuldu.\n" +
                    $"<color=red>{moralePenalty} Moral</color>\n" +
                    $"<color=red>{repPenalty} İtibar (Çantaya)</color>" +
                    (moraleMult < 1f ? "\n<size=75%> Cesur Yürek relic'i moral kaybını azalttı</size>" : "");

            CampMoraleManager.Instance?.ChangeMorale(moralePenalty);

            if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
                ExpeditionManager.Instance.AddLoot(0, repPenalty);
            else
                ReputationManager.Instance?.ChangeReputation(repPenalty);

            defeatPanel.SetActive(true);
        }
    }

    // ── KAMPA DÖN ────────────────────────────────────────────────────────
    public void ReturnToCamp()
    {
        lootPanel.SetActive(false);
        defeatPanel.SetActive(false);
        bgPanel.SetActive(false);
    if (BattleElementUI.Instance != null) BattleElementUI.Instance.Hide();
        bool isWinter = DayManager.Instance.currentDay >= DayManager.Instance.maxDays;
        AudioManager.Instance?.PlayCampMusic(isWinter);

        if (skillPanel != null) skillPanel.SetActive(false);
        topPanel.SetActive(true);
        MapManager.Instance?.ShowMap();

        bool wasVictorious = (state == BattleState.Won);

        var allUnits = FindObjectsOfType<GladiatorAI>();
        int row = 0, col = 0;
        float spacing = 1.5f;

        foreach (var unit in allUnits)
        {
            if (unit.CompareTag("EnemySoldier"))
            {
                Destroy(unit.gameObject);
            }
            else if (unit.CompareTag("MySoldier"))
            {
                unit.ReviveForCamp();
                unit.isInBattle = false;
                if (wasVictorious) unit.MakeGazi();

                if (campStandingPoint != null)
                {
                    Vector3 campPos = campStandingPoint.position;
                    campPos.x += col * spacing;
                    campPos.z -= row * spacing;

                    var agent = unit.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null) agent.Warp(campPos);
                    else unit.transform.position = campPos;

                    unit.transform.rotation = campStandingPoint.rotation;
                    col++;
                    if (col > 5) { col = 0; row++; }
                }
            }
        }

        foreach (var env in environments)
        {
            if (env.environmentProps != null) env.environmentProps.SetActive(false);
            if (env.volumeObject     != null) env.volumeObject.SetActive(false);
        }

        SeasonManager.Instance?.ForceCampLighting();
        if (SeasonManager.Instance != null)
    {
 
        SeasonManager.Instance.ResetAndCleanParticles(isWinter); 
    }
        StartCoroutine(CinematicTransitionRoutine(campCameraPos));

        state = BattleState.Idle;
        FindFirstObjectByType<BattlePowerUI>()?.ResetGameSpeed();
        TopInfoBarUI.Instance?.ForceUpdateAll();
    }


   void SpawnBoss()
    {
        if (bossPrefab == null) { Debug.LogError("Boss Prefab atanmamış!"); return; }
        Vector3 spawnPos = enemySpawnPoint.position;
        GameObject boss = Instantiate(bossPrefab, spawnPos, enemySpawnPoint.rotation);
        var agent = boss.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) { agent.Warp(spawnPos); agent.isStopped = true; }
        
        // ── YENİ: Boss İçin Yapay Zeka Menzil Senkronizasyonu ──
        GladiatorAI bossAI = boss.GetComponent<GladiatorAI>();
        Gladiator bossGlad = boss.GetComponent<Gladiator>();
        
        if (bossAI != null)
        {
            bossAI.isInBattle = true;
            
            if (bossGlad != null && bossGlad.data != null)
            {
                // Boss'un kendi data değerlerini lokal olarak AI'ına işle ki menzil sorunu yaşamasın
                bossAI.ForceSetupWeaponStats(bossGlad.data.attackRange, bossGlad.data.isRanged, bossGlad.data.weaponClass);
            }
        }
    }
    public void SpawnPlayerArmy(List<Gladiator> selectedSquad)
    {
        var allAI = FindObjectsOfType<GladiatorAI>();
        foreach (var ai in allAI)
            if (ai.CompareTag("MySoldier"))
                ai.isInBattle = selectedSquad.Contains(ai.GetComponent<Gladiator>());

        int row = 0;
        float spacing = 4.0f;
        foreach (var soldier in selectedSquad)
        {
            if (soldier == null) continue;

            soldier.GetComponent<GladiatorTraining>()?.StopTraining();
            var praying = soldier.GetComponent<GladiatorPraying>();
            if (praying != null && praying.isPraying) praying.StopPraying();
            if (soldier.data.currentActivity == SoldierActivity.Working)
                soldier.SetActivity(SoldierActivity.Idling);

            soldier.SetIdle();

            Vector3 targetPos = playerSpawnPoint.position;
            targetPos.z -= row * spacing;

            var agent = soldier.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) { agent.Warp(targetPos); agent.ResetPath(); agent.isStopped = true; }
            else soldier.transform.position = targetPos;

            soldier.transform.rotation = playerSpawnPoint.rotation;
            row++;
        }
    }

  void SpawnGenericEnemyArray(GameObject prefabToSpawn, int count)
    {
        if (prefabToSpawn == null) return;
        int row = 0, col = 0;
        float spacing = 2.0f;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = enemySpawnPoint.position;
            spawnPos.x += col * spacing;
            spawnPos.z += row * spacing;

            GameObject newEnemy = Instantiate(prefabToSpawn, spawnPos, enemySpawnPoint.rotation);
            var agent = newEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) { agent.Warp(spawnPos); agent.isStopped = true; }

            var ai = newEnemy.GetComponent<GladiatorAI>();
            if (ai != null) ai.isInBattle = true;

            var gladiator = newEnemy.GetComponent<Gladiator>();
            if (gladiator != null && gladiator.data != null)
            {
                // ── 1. DURUM: HEYVAN / AYI İSE ──
                if (ai != null && ai.isBeast)
                {
                    gladiator.data.gladiatorName = "Vahşi Ayı";
                    gladiator.data.weaponClass = WeaponClass.Unarmed;
                    gladiator.data.isRanged = false;
                    gladiator.data.attackRange = 2.5f; 

                    float tierMult = 1f + (_currentTier - 1) * 0.4f;
                    gladiator.data.strength = Mathf.RoundToInt(gladiator.data.strength * tierMult);
                    gladiator.data.defense  = Mathf.RoundToInt(gladiator.data.defense  * tierMult);
                    gladiator.data.stamina  = Mathf.RoundToInt(gladiator.data.stamina  * tierMult);
                    gladiator.data.level    = _currentTier;

                    // ── YENİ: Ayının Yapay Zeka Menzilini Zorla Kilitle! ──
                    ai.ForceSetupWeaponStats(2.5f, false, WeaponClass.Unarmed);
                }
                // ── 2. DURUM: İNSAN / DÜŞMAN ASKERİ İSE ──
                else
                {
                    EnemyLoadout randomEnemyType = null;
                    if (enemyTierConfig != null)
                    {
                        randomEnemyType = enemyTierConfig.GetRandomLoadout(_currentTier);
                    }

                    if (randomEnemyType != null)
                    {
                        gladiator.data.strength    = randomEnemyType.baseStrength + randomEnemyType.weaponBonus;
                        gladiator.data.defense     = randomEnemyType.baseDefense  + randomEnemyType.armorBonus;
                        gladiator.data.speed       = randomEnemyType.baseSpeed;
                        gladiator.data.stamina     = randomEnemyType.baseStamina;
                        gladiator.data.level       = randomEnemyType.baseLevel;
                        gladiator.data.elementType = randomEnemyType.elementType;
                        gladiator.data.weaponClass = randomEnemyType.weaponClass;
                        gladiator.data.attackRange = randomEnemyType.weaponRange;
                        gladiator.data.isRanged    = randomEnemyType.isRanged;

                        if (!string.IsNullOrEmpty(randomEnemyType.displayName))
                            gladiator.data.gladiatorName = randomEnemyType.displayName;

                        // ── YENİ: Düşmanın Yapay Zeka Menzilini Çekilen Tipe Göre Kilitle! ──
                        if (ai != null)
                        {
                            ai.ForceSetupWeaponStats(randomEnemyType.weaponRange, randomEnemyType.isRanged, randomEnemyType.weaponClass);
                        }

                        // Mesh'leri açma döngüsü...
                        if (randomEnemyType.activeMeshNames != null)
                        {
                            foreach (var meshName in randomEnemyType.activeMeshNames)
                            {
                                Transform meshObj = FindDeepChild(newEnemy.transform, meshName);
                                if (meshObj != null) meshObj.gameObject.SetActive(true);
                            }
                        }
                    }
                    else
                    {
                        // Fallback (ScriptableObject yüklenemezse eski koruma)
                        float tierMult = 1f + (_currentTier - 1) * 0.4f;
                        gladiator.data.strength = Mathf.RoundToInt(gladiator.data.strength * tierMult);
                        gladiator.data.defense  = Mathf.RoundToInt(gladiator.data.defense  * tierMult);
                        gladiator.data.stamina  = Mathf.RoundToInt(gladiator.data.stamina  * tierMult);
                        gladiator.data.level    = _currentTier;
                        gladiator.data.weaponClass = WeaponClass.Sword; 
                        gladiator.data.attackRange = 2.0f;

                        // ── YENİ: Yedek olarak kılıç menzilini kilitle ──
                        if (ai != null) ai.ForceSetupWeaponStats(2.0f, false, WeaponClass.Sword);
                    }
                }

                gladiator.RecalculateMaxHealth();
            }

            col++;
            if (col > 5) { col = 0; row++; }
        }
    }
    Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
            
            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

    // ── ORTAM ────────────────────────────────────────────────────────────
    void ChangeEnvironment(BattleEnvironment envType)
    {
        foreach (var env in environments)
        {
            if (env.environmentProps != null) env.environmentProps.SetActive(false);
            if (env.volumeObject     != null) env.volumeObject.SetActive(false);
        }

        EnvironmentProfile activeEnv = environments.Find(x => x.envType == envType);
        if (activeEnv != null)
        {
            if (activeEnv.environmentProps != null) activeEnv.environmentProps.SetActive(true);
            if (activeEnv.volumeObject     != null) activeEnv.volumeObject.SetActive(true);

            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.color     = activeEnv.sunColor;
                mainDirectionalLight.intensity = activeEnv.sunIntensity;
                mainDirectionalLight.shadows   = activeEnv.enableShadows ? LightShadows.Soft : LightShadows.None;
            }
        }
        else Debug.LogWarning($"DİKKAT: {envType} için Environment Profile bulunamadı!");
    }

    // ── SİNEMATİK GEÇİŞ ─────────────────────────────────────────────────
    IEnumerator CinematicTransitionRoutine(Transform targetCameraPos)
    {
        float t = 0, fadeDuration = 0.5f;
        if (fadeGroup != null) fadeGroup.blocksRaycasts = true;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (fadeGroup != null) fadeGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        if (fadeGroup != null) fadeGroup.alpha = 1;

        CampCameraController camController = mainCamera.GetComponent<CampCameraController>();
        if (camController != null)
            camController.TeleportTo(targetCameraPos, state == BattleState.Fighting);
        else
        {
            mainCamera.transform.position = targetCameraPos.position;
            mainCamera.transform.rotation = targetCameraPos.rotation;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (fadeGroup != null) fadeGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        if (fadeGroup != null) { fadeGroup.alpha = 0; fadeGroup.blocksRaycasts = false; }
    }
}
