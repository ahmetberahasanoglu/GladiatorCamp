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

    [Header("Arena Ortamları (Prop Swapping)")]
    public GameObject forestEnv;
    public GameObject caveEnv;
    public GameObject towerEnv;
    public GameObject winterEnv;
[Header("Atmosfer ve Ortam (AAA)")]
    public Light mainDirectionalLight; // Sahnendeki asıl güneş
    public List<EnvironmentProfile> environments; // Tüm ortamlarımızın listesi
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
    
    // YENİ: Yenilgi ekranında da ne kaybettiğimizi yazmak istersek diye (Opsiyonel)
    public TextMeshProUGUI defeatText; 

    public GameObject skillPanel;

    // YENİ: Savaşın zorluğunu hafızada tutuyoruz ki ödülü ona göre dağıtalım
    private int _currentDifficulty = 1;

    private int _currentEnemyCount = 1;

    [Header("Düşman Tier Sistemi")]
    public EnemyTierConfig enemyTierConfig;
    private int           _currentTier    = 1;   // MapEventManager tarafından set edilir
    private EnemyLoadout  _currentLoadout = null; // Spawn'da kullanılacak

        [Header("Sinematik Geçiş")]
    public CanvasGroup fadeGroup;

[Header("Taktik Ekranı UI")]
    public SquadSelectionUIManager squadSelectionUI; 
    
    // Geçici olarak hafızada tutacağımız savaş bilgileri
    private BattleEnvironment pendingEnv;
    [Header("Hedef İşaretleme (Focus) Sistemi")]
    public Transform currentFocusTarget; 
    public GameObject focusMarkerPrefab; 
    private GameObject _activeFocusMarker;
    [Header("Geri Çekilme (Retreat) Sistemi")]
    public Button retreatButton; // Savaş anında ekranda basılacak buton
    public int baseRetreatPenalty = 30; // 3 kişiyle tam kadro kaçmanın maksimum cezası

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (skillPanel != null) skillPanel.SetActive(false);
        if (retreatButton != null) retreatButton.onClick.AddListener(RetreatFromBattle);
        
    }
    
    [Header("Vahşi Hayvan (Ayı)")]
    public GameObject bearPrefab; 
    private bool _isBearBattle = false; 

  void Update()
    {
        // Sadece savaş devam ediyorsa tıklamaları algıla
        if (state == BattleState.Fighting)
        {
            HandleMouseClick();
        }
    }
    public void RetreatFromBattle()
    {
        // Sadece savaşırken çekilebiliriz
        if (state != BattleState.Fighting) return;

        // 1. Sahada hayatta olan askerlerimizi sayalım
        int aliveSoldiers = 0;
        var allUnits = FindObjectsByType<GladiatorAI>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit.CompareTag("MySoldier") && !unit.isDead && unit.isInBattle)
            {
                aliveSoldiers++;
            }
        }

        // 2. Senin Ceza Matematiğin
        // Formül: (Hayatta Kalan / 3) * Maksimum Ceza. 
        // Örn (Maks 30 ceza): 3 asker sağsa -30, 2 sağsa -20, 1 sağsa -10 itibar cezası yer!
        float penaltyMultiplier = (float)aliveSoldiers / 3f; 
        int calculatedPenalty = Mathf.RoundToInt(baseRetreatPenalty * penaltyMultiplier);

        // 3. Savaşı Durdur ve Herkesi Dondur
        state = BattleState.Lost; // Geri çekilmek teknik olarak o düğümü kaybetmektir
        
        // Varsa geri çekilme borusu çalınabilir
        // AudioManager.Instance.PlaySFX(AudioManager.Instance.retreatHorn, 1f); 

        foreach (var unit in allUnits)
        {
            if(unit.agent != null && unit.agent.isActiveAndEnabled) unit.agent.isStopped = true;
            if(unit.animator != null) unit.animator.SetBool("isRunning", false);
            
            // Eğer saldırı halindelerse onu da iptal edelim
            unit.StopAllCoroutines(); 
        }

        if (skillPanel != null) skillPanel.SetActive(false);
        topPanel.SetActive(true);

        // 4. Yenilgi/Geri Çekilme Ekranını Güncelle
        if (defeatText != null)
        {
            defeatText.text = $"GERİ ÇEKİLME!\n\nAskerlerini son anda sahadan çektin.\n<color=red>-{calculatedPenalty} İtibar (Korkaklık Bedeli)</color>";
        }

        // 5. Cezayı "Sefer Çantasına" yansıt (Kampa dönmeden uygulanmaz)
        if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
        {
            ExpeditionManager.Instance.AddLoot(0, -calculatedPenalty);
        }
        else if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.ChangeReputation(-calculatedPenalty);
        }

        defeatPanel.SetActive(true);
    }
    void HandleMouseClick()
    {
        // Sol tık basıldığında
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Tıklanan obje düşman mı?
                if (hit.collider.CompareTag("EnemySoldier"))
                {
                    GladiatorAI enemyAI = hit.collider.GetComponent<GladiatorAI>();
                    if (enemyAI != null && !enemyAI.isDead)
                    {
                        SetFocusTarget(hit.collider.transform);
                    }
                }
            }
        }
    }
    public void SetFocusTarget(Transform enemyTransform)
    {
        currentFocusTarget = enemyTransform;

        // Varsa tatlı bir "Ping" veya "Hücum" sesi çal
        // AudioManager.Instance.PlaySFX(AudioManager.Instance.pingSound, 1f);

        // --- GÖRSEL İŞARETLEYİCİYİ YARAT VE DÜŞMANA BAĞLA ---
        if (_activeFocusMarker == null && focusMarkerPrefab != null)
        {
            _activeFocusMarker = Instantiate(focusMarkerPrefab);
        }

        if (_activeFocusMarker != null)
        {
            _activeFocusMarker.SetActive(true);
            _activeFocusMarker.transform.SetParent(enemyTransform);
            // Düşmanın ayaklarının altına veya kafasının üstüne yerleştir
            _activeFocusMarker.transform.localPosition = new Vector3(0, 0.1f, 0); 
        }

        // --- TÜM ASKERLERE ANINDA EMİR VER ---
        GladiatorAI[] allUnits = FindObjectsByType<GladiatorAI>(FindObjectsSortMode.None);
        foreach(var unit in allUnits)
        {
            if (unit.CompareTag("MySoldier") && !unit.isDead)
            {
                unit.target = enemyTransform; // Hedeflerini zorla değiştir
                
                // Koşmaları için NavMesh'i anında uyar
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
        if (_activeFocusMarker != null)
        {
            _activeFocusMarker.SetActive(false);
            _activeFocusMarker.transform.SetParent(null); // Ayrıl
        }
    }
    public void StartBearBattle(int bearCount, int difficulty, BattleEnvironment envType)
    {
        Debug.Log("BattleManager: Ayı Savaşı Taktik Ekranı Açılıyor...");
        
        _currentEnemyCount = bearCount; // Genelde 1 veya 2 ayı olur
        _currentDifficulty = difficulty;
        pendingEnv = envType;
        _isBearBattle = true; // Savaş başladığında Ayı doğurmasını söyler

        if (MapManager.Instance != null) MapManager.Instance.HideMap();

        if (squadSelectionUI != null) 
            squadSelectionUI.OpenPanel(false, bearCount, difficulty); // Taktik ekranını normal açar
    }

   public void StartBattle(int enemyCount, int difficulty, BattleEnvironment envType, int tier = 1)
    {     
        _currentEnemyCount = enemyCount;
        _currentDifficulty = difficulty;
        _currentTier       = tier;
        _currentLoadout    = enemyTierConfig != null
            ? enemyTierConfig.GetRandomLoadout(tier)
            : null;
        pendingEnv = envType;
        _isBearBattle = false;
        if (MapManager.Instance != null) MapManager.Instance.HideMap();

   
        if (squadSelectionUI != null) 
            squadSelectionUI.OpenPanel(false, enemyCount, difficulty);
        else 
            Debug.LogError("SquadSelectionUI atanmamış!");
    }

    public void StartBossBattle(int difficulty, BattleEnvironment envType)
    {
        Debug.Log($"BattleManager: BOSS SAVAŞI Taktik Ekranı Açılıyor!");
        
        _currentDifficulty = difficulty;
        _currentEnemyCount = 5; 
        pendingEnv = envType;

        if (MapManager.Instance != null) MapManager.Instance.HideMap();

        // UI'a "Bu BOSS savaşı" diyoruz
        if (squadSelectionUI != null) 
            squadSelectionUI.OpenPanel(true, 1, difficulty);
        else 
            Debug.LogError("SquadSelectionUI atanmamış!");
    }
    void SpawnBoss()
    {
        if (bossPrefab == null) 
        {
            Debug.LogError("HATA: Boss Prefab'ı atanmamış!");
            return;
        }

        // Boss'u düşmanların normalde dizildiği yerin tam merkezinde (biraz daha önde) çıkartalım
        Vector3 spawnPos = enemySpawnPoint.position;
        // İstersen Boss'u biraz daha oyuncuya yakın başlatmak için: spawnPos.z -= 2f; 

        GameObject boss = Instantiate(bossPrefab, spawnPos, enemySpawnPoint.rotation);
        
        var agent = boss.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(spawnPos); 
            agent.isStopped = true; 
        }
        boss.GetComponent<GladiatorAI>().isInBattle = true;
    }
    // Artık herkesi değil, oyuncunun UI'da seçtiği o 3 özel askeri parametre olarak alacak
    public void SpawnPlayerArmy(List<Gladiator> selectedSquad)
    {
        var allAI = FindObjectsOfType<GladiatorAI>();
        foreach(var ai in allAI)
        {
            if (ai.CompareTag("MySoldier"))
            {
                // Eğer asker taktik ekranında seçildiyse savaştadır, seçilmediyse kamptadır
                ai.isInBattle = selectedSquad.Contains(ai.GetComponent<Gladiator>());
            }
        }
        int row = 0;
        float spacing = 4.0f; 

        // Seçilen 3 askeri sırasıyla diziyoruz (Index 0 = En ön, Index 2 = En arka)
        foreach(var soldier in selectedSquad)
        {
            if(soldier != null) 
            {
                // Askerin mevcut işini iptal et (Talim, dua, çalışma)
                var training = soldier.GetComponent<GladiatorTraining>();
                if (training != null && training.IsTraining) training.StopTraining();
                
                var praying = soldier.GetComponent<GladiatorPraying>();
                if (praying != null && praying.isPraying) praying.StopPraying();
                
                if (soldier.data.currentActivity == SoldierActivity.Working)
                {
                    soldier.SetActivity(SoldierActivity.Idling);
                }

                soldier.SetIdle();
                
                // --- 3'LÜ DİZİLİM MANTIĞI ---
                Vector3 targetPos = playerSpawnPoint.position;
                targetPos.x = playerSpawnPoint.position.x; // Hepsi aynı hizada arka arkaya dizilecek
                targetPos.z -= row * spacing; // Her asker bir eskisinin 2 metre arkasında duracak
                // ---------------------------

                var agent = soldier.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.Warp(targetPos); 
                    agent.ResetPath(); 
                    agent.isStopped = true; 
                    // Düşmanı savaşta kabul et
             
                } else
                {
                    soldier.transform.position = targetPos;
                }
             
               

                soldier.transform.rotation = playerSpawnPoint.rotation;

                row++; // Bir sonraki asker arkaya geçsin
            }
        }
    }
private void ChangeEnvironment(BattleEnvironment envType)
    {
      
        foreach (var env in environments)
        {
            if (env.environmentProps != null) env.environmentProps.SetActive(false);
            if (env.volumeObject != null) env.volumeObject.SetActive(false);
        }

        EnvironmentProfile activeEnv = environments.Find(x => x.envType == envType);

        if (activeEnv != null)
        {
            if (activeEnv.environmentProps != null) activeEnv.environmentProps.SetActive(true);
            if (activeEnv.volumeObject != null) activeEnv.volumeObject.SetActive(true);

            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.color = activeEnv.sunColor;
                mainDirectionalLight.intensity = activeEnv.sunIntensity;
                
                // Mağaraya girince gölgeleri kapatıp performansı artırabilirsin
                mainDirectionalLight.shadows = activeEnv.enableShadows ? LightShadows.Soft : LightShadows.None;
            }
        }
        else
        {
            Debug.LogWarning($"DİKKAT: {envType} için bir Environment Profile bulunamadı!");
        }
    }
   void SpawnGenericEnemyArray(GameObject prefabToSpawn, int count)
    {
        if (prefabToSpawn == null) return;

        int row = 0; int col = 0; float spacing = 2.0f;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = enemySpawnPoint.position;
            spawnPos.x += col * spacing;
            spawnPos.z += row * spacing;

            GameObject newEnemy = Instantiate(prefabToSpawn, spawnPos, enemySpawnPoint.rotation);
            
            var agent = newEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(spawnPos); 
                agent.isStopped = true; 
            }

            // ── LOADOUT UYGULA ──────────────────────────────────────────────
            var ai = newEnemy.GetComponent<GladiatorAI>();
            if (ai != null) ai.isInBattle = true;

            var gladiator = newEnemy.GetComponent<Gladiator>();
            if (gladiator != null && gladiator.data != null)
            {
                if (_currentLoadout != null)
                {
                    // Temel statları loadout'tan al
                    gladiator.data.strength = _currentLoadout.baseStrength  + _currentLoadout.weaponBonus;
                    gladiator.data.defense  = _currentLoadout.baseDefense   + _currentLoadout.armorBonus;
                    gladiator.data.speed    = _currentLoadout.baseSpeed;
                    gladiator.data.stamina  = _currentLoadout.baseStamina;
                    gladiator.data.level    = _currentLoadout.baseLevel;
                    gladiator.data.gladiatorName = _currentLoadout.displayName;

                    // Element tipi — GladiatorAI'da CombatElementSystem kullanılıyor
                    gladiator.data.elementType = _currentLoadout.elementType;

                    // İsim güncelle (savaşta label için)
                    if (!string.IsNullOrEmpty(_currentLoadout.displayName))
                        gladiator.data.gladiatorName = _currentLoadout.displayName;

                    // Mesh aktifleştirme
                    if (_currentLoadout.activeMeshNames != null)
                    {
                        foreach (var meshName in _currentLoadout.activeMeshNames)
                        {
                            var meshObj = newEnemy.transform.Find(meshName);
                            if (meshObj != null) meshObj.gameObject.SetActive(true);
                        }
                    }
                }
                else
                {
                    // Loadout yoksa tier'a göre sabit çarpan uygula (fallback)
                    float tierMult = 1f + (_currentTier - 1) * 0.4f; // T1:1.0, T2:1.4, T3:1.8
                    gladiator.data.strength = Mathf.RoundToInt(gladiator.data.strength * tierMult);
                    gladiator.data.defense  = Mathf.RoundToInt(gladiator.data.defense  * tierMult);
                    gladiator.data.stamina  = Mathf.RoundToInt(gladiator.data.stamina  * tierMult);
                    gladiator.data.level    = _currentTier;
                }

                // Can hesabını güncelle (stamina değişti)
                gladiator.RecalculateMaxHealth();
            }

            col++;
            if (col > 5) { col = 0; row++; }
        }
    }
// Taktik ekranında "Sefer Başlasın" butonuna basılınca UI bu fonksiyonu çağıracak
   public void ExecuteBattleWithSquad(List<Gladiator> selectedSquad, bool isBossBattle)
    {
        ChangeEnvironment(pendingEnv);
        AudioManager.Instance.StartBattleAcoustics();
        StartCoroutine(CinematicTransitionRoutine(battleCameraPos));

        SpawnPlayerArmy(selectedSquad);

        if (isBossBattle)
        {
            SpawnBoss();
        }
        else if (_isBearBattle) // EĞER AYI SAVAŞIYSA
        {
            SpawnGenericEnemyArray(bearPrefab, _currentEnemyCount);
        }
        else // EĞER NORMAL SAVAŞSA
        {
            SpawnGenericEnemyArray(enemyPrefab, _currentEnemyCount);
        }

        state = BattleState.Fighting;
        if (skillPanel != null) skillPanel.SetActive(true);
        topPanel.SetActive(false);
        bgPanel.SetActive(false);
    }
    public void CheckBattleStatus()
    {
        if (state != BattleState.Fighting) return;

        var allUnits = FindObjectsOfType<GladiatorAI>();

        int livingMySoldiers = 0;
        int livingEnemies = 0;

        foreach (var unit in allUnits)
        {
            if (!unit.isDead && unit.isInBattle)
            {
                if (unit.CompareTag("MySoldier")) livingMySoldiers++;
                else if (unit.CompareTag("EnemySoldier")) livingEnemies++;
            }
        }

        if (livingEnemies <= 0)
        {
            EndBattle(true); 
        }
        else if (livingMySoldiers <= 0)
        {
            EndBattle(false); 
            if (MapManager.Instance != null) MapManager.Instance.RetreatToPreviousNode();
        }
    }

   public void EndBattle(bool isVictory)
    {
        state = isVictory ? BattleState.Won : BattleState.Lost;

        if (skillPanel != null) skillPanel.SetActive(false);
        topPanel.SetActive(true);

        if (isVictory)
        {
            int goldReward = _currentEnemyCount * 50 * _currentDifficulty; // Örn: 5 Düşman * Zorluk 2 = 500 Akçe
            int foodReward = _currentEnemyCount * 10;                      // Düşman kampından yağmalanan erzak
            int moraleReward = 10 + (_currentDifficulty * 5);              // Zor savaş kazanmak daha çok moral verir
            int repReward = 5 * _currentDifficulty;                        // İtibar artışı

            // YENİ: Oyuncuya nelerin çantaya gittiğini, nelerin anında işlendiğini UI'da belli ediyoruz
            if (lootText != null)
                lootText.text = $"ZAFER!\n\n<color=yellow>+{goldReward} Akçe (Çantaya)</color>\n<color=green>+{repReward} İtibar (Çantaya)</color>\n+{foodReward} Erzak\n+{moraleReward} Moral";

            // --- 1. ÇANTAYA GİDENLER (Riske Atılanlar) ---
            if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
            {
                // Kazanılan altın ve itibarı geçici sefere gönder!
                ExpeditionManager.Instance.AddLoot(goldReward, repReward);
            }
            else
            {
                // Eğer bir bug olur da seferde değilken savaşırsak direkt verilsin (Güvenlik Önlemi)
                if (MoneyManager.Instance != null) MoneyManager.Instance.Add(goldReward);
                if (ReputationManager.Instance != null) ReputationManager.Instance.ChangeReputation(repReward);
            }

            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(moraleReward);
            if (SupplyManager.Instance != null) SupplyManager.Instance.BuyFood(foodReward);

            lootPanel.SetActive(true); 
        }
        else
        {
            // YENİLGİ DURUMU
            int moralePenalty = -20 - (_currentDifficulty * 5); 
            int repPenalty = -10;

            if (defeatText != null)
                defeatText.text = $"AĞIR YENİLGİ...\n\nOtağ yasa boğuldu.\n<color=red>{moralePenalty} Moral</color>\n<color=red>{repPenalty} İtibar (Çantaya)</color>";

            // Moral anında düşer
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(moralePenalty);
            
   
            if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
            {
                ExpeditionManager.Instance.AddLoot(0, repPenalty);
            }
            else
            {
                if (ReputationManager.Instance != null) ReputationManager.Instance.ChangeReputation(repPenalty);
            }

            defeatPanel.SetActive(true); 
        }
    }

   public void ReturnToCamp()
    {
        lootPanel.SetActive(false);
        defeatPanel.SetActive(false);
        bgPanel.SetActive(false);
        
        bool isWinter = DayManager.Instance.currentDay >= DayManager.Instance.maxDays;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCampMusic(isWinter);
        
        if (skillPanel != null) skillPanel.SetActive(false);
        topPanel.SetActive(true);
        if (MapManager.Instance != null) MapManager.Instance.ShowMap(); 

        // SAVAŞ KAZANILDI MI? (Sadece kazandıysak Gazi unvanı ve Ekstra Moral vereceğiz)
        bool wasVictorious = (state == BattleState.Won);

        var allUnits = FindObjectsOfType<GladiatorAI>();
        
        int row = 0;
        int col = 0;
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

                if (wasVictorious)
                {
                    unit.MakeGazi();
                }

                if (campStandingPoint != null)
                {
                    Vector3 campPos = campStandingPoint.position;
                    campPos.x += col * spacing;
                    campPos.z -= row * spacing;

                    if (unit.GetComponent<UnityEngine.AI.NavMeshAgent>() != null)
                    {
                        unit.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(campPos);
                    }
                    else
                    {
                        unit.transform.position = campPos;
                    }

                    unit.transform.rotation = campStandingPoint.rotation;

                    col++;
                    if(col > 5) { col = 0; row++; }
                }
            }
        }

        foreach (var env in environments)
        {
            if (env.environmentProps != null) env.environmentProps.SetActive(false);
            if (env.volumeObject != null) env.volumeObject.SetActive(false);
        }

        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.ForceCampLighting();
        }

        // --- EĞER KAMPA ZAFERLE DÖNÜLDÜYSE EKSTRA KAMP MORALİ (Opsiyonel) ---
        if (wasVictorious && CampMoraleManager.Instance != null)
        {
            // İstersen buraya ufak bir ekstra moral koyabilirsin, çünkü askerler kampa döndü
            // CampMoraleManager.Instance.ChangeMorale(5); 
        }

        StartCoroutine(CinematicTransitionRoutine(campCameraPos));
        
        state = BattleState.Idle;
        BattlePowerUI powerUI = FindFirstObjectByType<BattlePowerUI>();
         if(powerUI != null)
    {
            powerUI.ResetGameSpeed();
    }

        // --- 2. SORUNUN ÇÖZÜMÜ: TOP PANELİ ZORLA GÜNCELLE ---
        // Askerlerin levelleri veya sayıları değişmiş olabilir. 
        // Tüm sistemi baştan hesaplatıp ekranda (Maaş, Erzak vs.) gösteriyoruz.
        if (TopInfoBarUI.Instance != null)
        {
            TopInfoBarUI.Instance.ForceUpdateAll();
        }
    }

    IEnumerator CinematicTransitionRoutine(Transform targetCameraPos)
    {

        float t = 0;
        float fadeDuration = 0.5f;

        if (fadeGroup != null) fadeGroup.blocksRaycasts = true;

        while(t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (fadeGroup != null) fadeGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        if (fadeGroup != null) fadeGroup.alpha = 1;

      CampCameraController camController = mainCamera.GetComponent<CampCameraController>();
        if (camController != null)
        {
            // Eğer savaş başladıysa kamerayı kilitle (true), kampa dönüyorsak kilidi aç (false)
            bool shouldLock = (state == BattleState.Fighting);
            camController.TeleportTo(targetCameraPos, shouldLock);
        }
        else
        {
            
            mainCamera.transform.position = targetCameraPos.position;
            mainCamera.transform.rotation = targetCameraPos.rotation;
        }

        // savaş naraları/kılıç çekme sesleri
        // AudioManager.Instance.PlaySwordDraw();
        
        // Oyuncuunu sahnenin değiştiğini algılaması için ufak bir es 
        yield return new WaitForSecondsRealtime(0.2f);


        t = 0;
        while(t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (fadeGroup != null) fadeGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        if (fadeGroup != null) 
        {
            fadeGroup.alpha = 0;
            fadeGroup.blocksRaycasts = false; 
        }
    }
}