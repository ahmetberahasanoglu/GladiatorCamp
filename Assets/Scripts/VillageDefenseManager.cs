using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Köy Meydanı Savunması — 3 dalga, köylü hayatta kalma mekaniği.
/// Modern UI ve İkon Temelli Versiyon.
/// </summary>
public class VillageDefenseManager : MonoBehaviour
{
    public static VillageDefenseManager Instance;

    // ── SPAWN ─────────────────────────────────────────────────────────────
    [Header("Düşman Spawn Noktaları")]
    public List<Transform> enemySpawnPoints;   // Köyün çevresi, 3-4 nokta

    [Header("Oyuncu Başlangıç")]
    public Transform playerSpawnPoint;         // Köy meydanının ortası

    [Header("Köylü Pozisyonları")]
    public List<Transform> villagerSpawnPoints; // Köylülerin durduğu yerler

    // ── PREFABLAR ─────────────────────────────────────────────────────────
    [Header("Prefablar")]
    public GameObject playerSoldierPrefab;     // Kendi asker modelinin prefabı
    public GameObject enemyPrefab;
    public GameObject eliteEnemyPrefab;        // 3. dalga
    public GameObject villagerPrefab;          // Savunulacak köylü NPC

    [Header("Savaş Sistemi")]
    public Transform currentFocusTarget;
    public GameObject focusIndicatorPrefab; // Düşmanın tepesinde çıkacak kırmızı ok/kılıç ikonu
    private GameObject _activeFocusIndicator;

    [Header("RTS Seçim Sistemi")]
    public GladiatorAI currentlySelectedSoldier; // O an tıkladığımız asker
    public GameObject selectionIndicatorPrefab; // Askerin altında çıkacak yeşil halka
    private GameObject _activeSelectionIndicator;

    // ── DALGA AYARLARI ────────────────────────────────────────────────────
    [Header("Dalga")]
    public float breakDuration   = 10f;
    private readonly int[] _baseEnemies = { 3, 4, 5 }; // Tier 1 baz

    // ── MODERN UI BİLEŞENLERİ (GÜNCELLENDİ) ────────────────────────────────
    [Header("Modern UI - Köylü İkonları (3 Adet)")]
    [Tooltip("Hiyerarşideki 3 köylü Image objesini sırasıyla buraya atın")]
    public List<Image> villagerIcons; 
    public Color villagerAliveColor = Color.white; // Hayattayken ikonun rengi
    public Color villagerDeadColor = new Color(0.2f, 0.2f, 0.2f, 0.4f); // Öldüğünde soluk renk

    [Header("Modern UI - Dalga İkonları (3 Adet)")]
    [Tooltip("Zaman çizelgesindeki 3 dalga Image objesini sırasıyla buraya atın")]
    public List<Image> waveIcons; 
    public Sprite futureWaveSprite;    // Henüz kilitli/gelecek dalga görseli
    public Sprite activeWaveSprite;    // Şu anki aktif dalga görseli
    public Sprite completedWaveSprite; // Temizlenmiş/bitmiş dalga görseli

    [Header("UI Panelleri")]
    public TextMeshProUGUI timerText; // Mola geri sayımı için ufak minimalist yazı
    public TextMeshProUGUI statusText; // Mola geri sayımı için ufak minimalist yazı
    public GameObject      breakPanel;
    public GameObject      resultPanel;
    public TextMeshProUGUI resultText;
    public Button          returnButton;

    // Dalga arası köylü yardım butonları
    [Header("Dalga Arası Yardım")]
    public GameObject helpPanel;
    public Button     helpHealButton;      // "Yaralıyı İyileştir"
    public Button     helpFoodButton;      // "+10 Erzak"
    public Button     helpSkipButton;      // "Teşekkürler, gerek yok"

    // ── İÇ DURUM ──────────────────────────────────────────────────────────
    private int  _currentWave    = 0;
    private int  _totalWaves     = 3;
    private bool _playerDefeated = false;
    private int  _villagersAlive = 0;
    private int  _villagersTotal = 3;
    
    private int _finalGoldReward = 0;
    private int _finalRepReward = 0;
    private int _finalMoraleReward = 0;
    private int _finalFoodReward = 0;
    
    private int[] _waveEnemyCounts;
    private bool _helpChosen = false;

    private List<GameObject> _activeEnemies  = new List<GameObject>();
    private List<Gladiator>  _playerSoldiers = new List<Gladiator>();
    private List<VillagerNPC> _villagers     = new List<VillagerNPC>();

    [Header("Haydut Verileri (Loadout)")]
    public EnemyLoadout normalBanditLoadout; // 1. ve 2. dalga haydutları
    public EnemyLoadout eliteBanditLoadout;  // 3. dalga haydut başı (Boss)
    public GameObject selectionRing;

    // ── AWAKE / START ──────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Tier'a göre düşman sayısını ayarla
        int tier = PlayerPrefs.GetInt("VillageDefenseTier", 1);
        int[] enemiesPerWave = new int[_totalWaves];
        for (int i = 0; i < _totalWaves; i++)
            enemiesPerWave[i] = Mathf.Max(2, _baseEnemies[i] + (tier - 1));

        _waveEnemyCounts = enemiesPerWave;

        if (resultPanel  != null) resultPanel.SetActive(false);
        if (breakPanel   != null) breakPanel.SetActive(false);
        if (helpPanel    != null) helpPanel.SetActive(false);
        if (timerText    != null) timerText.gameObject.SetActive(false);

        if (returnButton    != null) returnButton.onClick.AddListener(ReturnToCamp);
        if (helpHealButton  != null) helpHealButton.onClick.AddListener(OnHelpHeal);
        if (helpFoodButton  != null) helpFoodButton.onClick.AddListener(OnHelpFood);
        if (helpSkipButton  != null) helpSkipButton.onClick.AddListener(OnHelpSkip);

        SetupPlayerSoldiers();
        SpawnVillagers();
        
        // Yenilenen modern UI fonksiyonları ilk durum için tetikleniyor
        UpdateVillagerUI();
        UpdateWaveUI(0, false); // Henüz dalga başlamadı

        StartCoroutine(DefenseSequence());
    }

    // ── OYUNCU KURULUMU ───────────────────────────────────────────────────
    void SetupPlayerSoldiers()
    {
        var myArmyData = CampManager.Instance.activeMissionArmy;
        if (myArmyData == null || myArmyData.Count == 0)
        {
            Debug.LogError("CRITICAL ERROR: Savaş sahnesine gelen asker listesi BOŞ! Haritadaki buton verileri çantaya kopyalamamış.");
            return;
        }
        
        if (playerSoldierPrefab == null)
        {
            Debug.LogError("CRITICAL ERROR: VillageDefenseManager içinde 'Player Soldier Prefab' atanmamış!");
            return;
        }
        
        float spacing = 2.5f;
        int idx = 0;

        foreach (var data in myArmyData) 
        {
            if (data.currentHealth <= 0) continue; 

            GameObject soldierObj = Instantiate(playerSoldierPrefab); 
            soldierObj.tag = "MySoldier"; 
            Gladiator s = soldierObj.GetComponent<Gladiator>();
            s.data = data; 
            
            _playerSoldiers.Add(s);

            Vector3 pos = playerSpawnPoint != null
                ? playerSpawnPoint.position + new Vector3(idx * spacing - (myArmyData.Count * spacing / 2f), 0, 0)
                : new Vector3(idx * spacing, 0, 0);

            var agent = s.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.Warp(pos);
            else s.transform.position = pos;

            var ai = s.GetComponent<GladiatorAI>();
            if (ai != null) ai.isInBattle = true;

            idx++;
        }
    }

    // ── KÖYLÜLERİ SPAWN ET ───────────────────────────────────────────────
    void SpawnVillagers()
    {
        _villagers.Clear();
        _villagersTotal = villagerSpawnPoints != null
            ? Mathf.Min(villagerSpawnPoints.Count, 3) : 3;
        _villagersAlive = _villagersTotal;

        if (villagerPrefab == null) return;

        for (int i = 0; i < _villagersTotal; i++)
        {
            Vector3 pos = (villagerSpawnPoints != null && i < villagerSpawnPoints.Count)
                ? villagerSpawnPoints[i].position
                : new Vector3(i * 3f, 0, 5f);

            GameObject go  = Instantiate(villagerPrefab, pos, Quaternion.identity);
            go.tag = "MySoldier"; // Düşmanlar köylülere saldırsın
            
            VillagerNPC npc = go.GetComponent<VillagerNPC>();
            if (npc == null) npc = go.AddComponent<VillagerNPC>();
            npc.OnVillagerDied += OnVillagerDied;
            _villagers.Add(npc);
        }
    }

    // ── KÖYLÜLERİ TAKIP ──────────────────────────────────────────────────
    void OnVillagerDied()
    {
        _villagersAlive = Mathf.Max(0, _villagersAlive - 1);
        UpdateVillagerUI(); // Köylü ölünce görseli anında güncelle

        if (NotificationManager.Instance != null)
        {
            string msg = _villagersAlive > 0
                ? "Bir köylü düştü!"
                : "Tüm köylüler katledildi! Ödül yarıya indi.";
            NotificationManager.Instance.Show(msg, NotificationType.Warning);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider.CompareTag("MySoldier"))
                {
                    GladiatorAI clickedAI = hit.collider.GetComponent<GladiatorAI>();
                    if (clickedAI != null && !clickedAI.isDead)
                    {
                        SelectSoldier(clickedAI);
                    }
                }
                else if (hit.collider.CompareTag("EnemySoldier")) 
                {
                    if (currentlySelectedSoldier != null && !currentlySelectedSoldier.isDead)
                    {
                        currentlySelectedSoldier.forcedTarget = hit.collider.transform;
                        ShowFocusPing(hit.collider.transform);
                    }
                }
                else
                {
                    DeselectSoldier();
                }
            }
        }
    }

    void SelectSoldier(GladiatorAI soldier)
    {
        DeselectSoldier();

        currentlySelectedSoldier = soldier;
        
        GladiatorTraining training = soldier.GetComponent<GladiatorTraining>();
        if (training != null && training.selectionRing != null)
        {
            training.SetSelectedVisual(true); 
        }
        else if (selectionIndicatorPrefab != null) 
        {
            _activeSelectionIndicator = Instantiate(selectionIndicatorPrefab, soldier.transform);
            _activeSelectionIndicator.transform.localPosition = Vector3.up * 0.1f; 
        }
    }

    void DeselectSoldier()
    {
        if (currentlySelectedSoldier != null)
        {
            GladiatorTraining training = currentlySelectedSoldier.GetComponent<GladiatorTraining>();
            if (training != null && training.selectionRing != null)
            {
                training.SetSelectedVisual(false);
            }
        }

        currentlySelectedSoldier = null;
        
        if (_activeSelectionIndicator != null) 
        {
            Destroy(_activeSelectionIndicator);
        }
    }

    void ShowFocusPing(Transform enemy)
    {
        if (focusIndicatorPrefab != null)
        {
            GameObject ping = Instantiate(focusIndicatorPrefab, enemy);
            ping.transform.localPosition = Vector3.up * 2.5f;
            Destroy(ping, 1.5f);
        }
    }

    public void SetFocusTarget(Transform newTarget)
    {
        currentFocusTarget = newTarget;

        if (focusIndicatorPrefab != null)
        {
            if (_activeFocusIndicator != null) Destroy(_activeFocusIndicator);
            _activeFocusIndicator = Instantiate(focusIndicatorPrefab, newTarget);
            _activeFocusIndicator.transform.localPosition = Vector3.up * 2.5f; 
        }
    }

    public void ClearFocusTarget()
    {
        currentFocusTarget = null;
        if (_activeFocusIndicator != null) Destroy(_activeFocusIndicator);
    }

    // ── YENİLENEN MODERN UI SİSTEMİ FONKSİYONLARI ───────────────────────────
    
    void UpdateVillagerUI()
    {
        // Yazı mantığı kalktı, 3 adet Image ikonunun renk durumu yönetiliyor
        for (int i = 0; i < villagerIcons.Count; i++)
        {
            if (villagerIcons[i] == null) continue;

            if (i < _villagersAlive)
                villagerIcons[i].color = villagerAliveColor; // Sağ kalan köylü ikonu
            else
                villagerIcons[i].color = villagerDeadColor;  // Ölen köylü ikonu (soluk)
        }
    }

    void UpdateWaveUI(int activeWaveNumber, bool waveCleared)
    {
        // Dalga durumu 3 adet Image ikonu ve özel Sprite geçişleriyle yapılıyor
        for (int i = 0; i < waveIcons.Count; i++)
        {
            if (waveIcons[i] == null) continue;

            int waveNodeNumber = i + 1; // 1, 2, 3 nolu dalga düğümleri

            if (waveNodeNumber < activeWaveNumber || (waveNodeNumber == activeWaveNumber && waveCleared))
            {
                // Bitmiş/Temizlenmiş dalga ikonları
                if (completedWaveSprite != null) waveIcons[i].sprite = completedWaveSprite;
                waveIcons[i].color = new Color(1f, 1f, 1f, 0.6f); // Hafif soluk
            }
            else if (waveNodeNumber == activeWaveNumber && !waveCleared)
            {
                // Şu an oynamakta/çarpışmakta olduğumuz aktif dalga ikonu
                if (activeWaveSprite != null) waveIcons[i].sprite = activeWaveSprite;
                waveIcons[i].color = Color.white; // Tam parlak
            }
            else
            {
                // Gelecekteki kilitli dalga ikonları
                if (futureWaveSprite != null) waveIcons[i].sprite = futureWaveSprite;
                waveIcons[i].color = new Color(1f, 1f, 1f, 0.2f); // Oldukça silik
            }
        }
    }

    // ── ANA SAVUNMA SIRASI ────────────────────────────────────────────────
    IEnumerator DefenseSequence()
    {
        yield return new WaitForSeconds(1.5f);

        for (int wave = 0; wave < _totalWaves; wave++)
        {
            _currentWave = wave + 1;
            
            // Modern UI: Aktif olan dalga görselini yakıyoruz
            UpdateWaveUI(_currentWave, false);

            SpawnWave(wave);

            yield return StartCoroutine(WaitForWaveEnd());

            if (_playerDefeated) break;

            // Son dalga değilse mola + köylü yardımı
            if (wave < _totalWaves - 1)
            {
                // Modern UI: Dalga savuşturulduğu için ikonu tamamlandı durumuna alıyoruz
                UpdateWaveUI(_currentWave, true);
                yield return StartCoroutine(BreakAndHelp());
            }
        }

        ShowResult();
    }

    // ── DALGA SPAWN ───────────────────────────────────────────────────────
    void SpawnWave(int waveIndex)
    {
        _activeEnemies.Clear();
        if (enemySpawnPoints == null || enemySpawnPoints.Count == 0) return;

        int   count  = _waveEnemyCounts[Mathf.Clamp(waveIndex, 0, _waveEnemyCounts.Length - 1)];
        bool  elite  = waveIndex == _totalWaves - 1; 

        EnemyLoadout currentLoadout = elite && eliteBanditLoadout != null ? eliteBanditLoadout : normalBanditLoadout;
        
        int tier = PlayerPrefs.GetInt("VillageDefenseTier", 1);
        int tierStatBonus = (tier - 1) * 2; 

        for (int i = 0; i < count; i++)
        {
            Transform spawnPt = enemySpawnPoints[i % enemySpawnPoints.Count];
            GameObject prefab = (elite && eliteEnemyPrefab != null) ? eliteEnemyPrefab : enemyPrefab;
            if (prefab == null) continue;

            Vector3 offset = new Vector3(Random.Range(-1.2f, 1.2f), 0, Random.Range(-1.2f, 1.2f));

            GameObject enemy = Instantiate(prefab, spawnPt.position + offset, spawnPt.rotation);
            enemy.tag = "EnemySoldier";

            Gladiator g = enemy.GetComponent<Gladiator>();
            if (g != null)
            {
                if (g.data != null) g.data = Instantiate(g.data); 

                if (currentLoadout != null)
                {
                    g.data.weaponClass = currentLoadout.weaponClass;
                    g.data.attackRange = currentLoadout.weaponRange;
                    g.data.isRanged    = currentLoadout.isRanged;
                    g.data.gladiatorName = currentLoadout.displayName;

                    g.data.strength = currentLoadout.baseStrength + currentLoadout.weaponBonus + tierStatBonus;
                    g.data.defense  = currentLoadout.baseDefense + currentLoadout.armorBonus + tierStatBonus;
                    g.data.speed    = currentLoadout.baseSpeed;
                    g.data.level    = currentLoadout.baseLevel + (tier - 1);

                    if (currentLoadout.activeMeshNames != null)
                    {
                        foreach (var meshName in currentLoadout.activeMeshNames)
                        {
                            Transform meshObj = FindDeepChild(enemy.transform, meshName);
                            if (meshObj != null) meshObj.gameObject.SetActive(true);
                        }
                    }
                }
                else
                {
                    g.data.weaponClass = WeaponClass.Sword;
                    g.data.attackRange = 2.0f;
                }

                g.RecalculateMaxHealth();
                g.data.currentHealth = g.data.maxHealth;
            }

            var ai = enemy.GetComponent<GladiatorAI>();
            if (ai != null) ai.isInBattle = true;

            _activeEnemies.Add(enemy);
        }
    }
    
    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    // ── DALGA BİTİŞİ BEKLE ───────────────────────────────────────────────
    IEnumerator WaitForWaveEnd()
    {
        while (true)
        {
            int alivePlayers = 0;
            foreach (var s in _playerSoldiers)
            {
                if (s != null && s.data != null && s.data.currentHealth > 0) 
                    alivePlayers++;
            }

            if (alivePlayers == 0) { _playerDefeated = true; yield break; }

            _activeEnemies.RemoveAll(e => e == null);
            int aliveEnemies = 0;
            foreach (var e in _activeEnemies)
            {
                if (e == null) continue;
                var g = e.GetComponent<Gladiator>();
                if (g != null && g.data != null && g.data.currentHealth > 0) 
                    aliveEnemies++;
            }

            if (aliveEnemies == 0) yield break;

            yield return new WaitForSeconds(0.4f);
        }
    }

    // ── MOLA + KÖYLÜ YARDIMI ─────────────────────────────────────────────
    IEnumerator BreakAndHelp()
    {
        if (breakPanel != null) breakPanel.SetActive(true);
        if (timerText  != null) timerText.gameObject.SetActive(true);

        if (_villagersAlive > 0 && helpPanel != null)
        {
            helpPanel.SetActive(true);
            _helpChosen = false;

            float helpWait = 0f;
            while (!_helpChosen && helpWait < 5f)
            {
                helpWait += Time.deltaTime;
                yield return null;
            }
            helpPanel.SetActive(false);
        }

        float remaining = breakDuration;
        while (remaining > 0f)
        {
            if (statusText != null)
                statusText.text = $"Sonraki Saldırı: ${Mathf.CeilToInt(remaining)}s"; // Modern yazı formatı
            remaining -= Time.deltaTime;
            yield return null;
        }

        if (breakPanel != null) breakPanel.SetActive(false);
        if (statusText  != null) statusText.gameObject.SetActive(false);
    }

    void OnHelpHeal()
    {
        Gladiator wounded = null;
        float minHp = float.MaxValue;
        foreach (var s in _playerSoldiers)
        {
            if (s != null && s.data != null && s.data.currentHealth > 0 && s.data.currentHealth < minHp)
            { 
                minHp = s.data.currentHealth; 
                wounded = s; 
            }
        }
        
        if (wounded != null)
        {
            float healAmt = wounded.data.maxHealth * 0.4f;
            wounded.Heal(healAmt);
            NotificationManager.Instance?.Show(
                $"{wounded.data.gladiatorName} iyileştirildi! (+{Mathf.RoundToInt(healAmt)} Can)",
                NotificationType.Success);
        }
        _helpChosen = true;
    }

    void OnHelpFood()
    {
        SupplyManager.Instance?.AddFood(10);
        NotificationManager.Instance?.Show(
            "Köylüler 10 erzak verdi!", NotificationType.Success);
        _helpChosen = true;
    }

    void OnHelpSkip()
    {
        _helpChosen = true;
    }

    // ── SONUÇ ─────────────────────────────────────────────────────────────
    void ShowResult()
    {
        if (resultPanel != null) resultPanel.SetActive(true);

        bool fullSuccess = _villagersAlive > 0;
        int tier = ExpeditionManager.Instance != null ? ExpeditionManager.Instance.currentEncounterCount : 1;

        if (_playerDefeated)
        {
            if (resultText != null)
                resultText.text =
                    "<color=red>KÖYÜ KORUYAMADIN!</color>\n\n" +
                    "Haydutlar köyü ele geçirdi. Askerlerin geri çekilmek zorunda kaldı.\n\n" +
                    "−15 İtibar  ·  −10 Moral";

            _finalRepReward = -15;
            _finalMoraleReward = -10;
        }
        else if (fullSuccess)
        {
            _finalGoldReward = 150 + (tier * 30) + (_villagersAlive * 40);
            _finalRepReward = 25;
            _finalMoraleReward = 15;
            _finalFoodReward = _villagersAlive * 5; 

            if (resultText != null)
                resultText.text =
                    "<color=#FFD700>KÖY KURTARILDI!</color>\n\n" +
                    $"Köylüler sana minnettarlıkla yaklaştı. Sancaklar senin için kalkıyor!\n\n" +
                    $"+{_finalRepReward} İtibar  ·  +{_finalMoraleReward} Moral\n" +
                    $"+{_finalGoldReward} Akçe  ·  +{_finalFoodReward} Erzak";
        }
        else 
        {
            _finalGoldReward = 50 + (tier * 10);
            _finalRepReward = 5;
            _finalMoraleReward = -5; 

            if (resultText != null)
                resultText.text =
                    "<color=#FFA500>ZAFER, AMA KÖY KÜLE DÖNDÜ</color>\n\n" +
                    "Haydutları kovdun ama geriye kurtaracak kimse kalmadı.\n\n" +
                    $"+{_finalRepReward} İtibar  ·  +{_finalGoldReward} Akçe</color>\n" +
                    "Köylüleri koruyamadığın için askerlerin moralsiz. (−5 Moral)";
        }
    }

    // ── KAMPA DÖN ─────────────────────────────────────────────────────────
    void ReturnToCamp()
    {
        if (CampMoraleManager.Instance != null) 
            CampMoraleManager.Instance.ChangeMorale(_finalMoraleReward);

        if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
        {
            ExpeditionManager.Instance.AddLoot(_finalGoldReward, _finalRepReward);
            
            if (_finalFoodReward > 0 && SupplyManager.Instance != null)
                SupplyManager.Instance.AddFood(_finalFoodReward);

            UnityEngine.SceneManagement.SceneManager.LoadScene("CampScene"); 
        }
        else 
        {
            if (MoneyManager.Instance != null) MoneyManager.Instance.Add(_finalGoldReward);
            if (ReputationManager.Instance != null) ReputationManager.Instance.ChangeReputation(_finalRepReward);
            
            UnityEngine.SceneManagement.SceneManager.LoadScene("CampScene");
        }
    }
}