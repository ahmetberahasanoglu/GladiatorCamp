using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
        [Header("Sinematik Geçiş")]
    public CanvasGroup fadeGroup;

[Header("Taktik Ekranı UI")]
    public SquadSelectionUIManager squadSelectionUI; 
    
    // Geçici olarak hafızada tutacağımız savaş bilgileri
    private BattleEnvironment pendingEnv;
 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (skillPanel != null) skillPanel.SetActive(false);
        
    }

   public void StartBattle(int enemyCount, int difficulty, BattleEnvironment envType)
    {
        Debug.Log("BattleManager: Taktik Ekranı Açılıyor...");
        
        _currentEnemyCount = enemyCount;
        _currentDifficulty = difficulty;
        pendingEnv = envType;

        if (MapManager.Instance != null) MapManager.Instance.HideMap();

        // UI'a "Bu normal savaş, şu kadar düşman var" diyoruz
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
        float spacing = 2.0f; 

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
    void SpawnEnemyArmy(int count)
    {
        if (enemyPrefab == null) return;

        int row = 0;
        int col = 0;
        float spacing = 2.0f;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = enemySpawnPoint.position;
            spawnPos.x += col * spacing;
            spawnPos.z += row * spacing;

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, enemySpawnPoint.rotation);
            
            var agent = newEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(spawnPos); 
                agent.isStopped = true; 
            }
            newEnemy.GetComponent<GladiatorAI>().isInBattle = true;
            col++;
            if(col > 5) { col = 0; row++; }
        }
    }
// Taktik ekranında "Sefer Başlasın" butonuna basılınca UI bu fonksiyonu çağıracak
    public void ExecuteBattleWithSquad(List<Gladiator> selectedSquad, bool isBossBattle)
    {
        ChangeEnvironment(pendingEnv);
        AudioManager.Instance.StartBattleAcoustics();
        StartCoroutine(CinematicTransitionRoutine(battleCameraPos));

        // İşte burada o yeni yazdığımız, parametre alan SpawnPlayerArmy'yi çağırıyoruz
        SpawnPlayerArmy(selectedSquad);

        if (isBossBattle)
        {
            SpawnBoss();
        }
        else
        {
            SpawnEnemyArmy(_currentEnemyCount);
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

            if (lootText != null)
                lootText.text = $"ZAFER!\n\n+{goldReward} Akçe\n+{foodReward} Erzak\n+{repReward} İtibar\n+{moraleReward} Moral";

      
            if (MoneyManager.Instance != null) MoneyManager.Instance.Add(goldReward);
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(moraleReward);
            ReputationManager.Instance.ChangeReputation(repReward);
            SupplyManager.Instance.BuyFood(foodReward);
            // İLERİDE EKLENECEK SİSTEMLER İÇİN HAZIR ALTYAPI:
            // if (ResourceManager.Instance != null) ResourceManager.Instance.AddFood(foodReward);
            // if (ReputationManager.Instance != null) ReputationManager.Instance.AddReputation(repReward);

            lootPanel.SetActive(true); 
        }
        else
        {

            int moralePenalty = -20 - (_currentDifficulty * 5); 
            int repPenalty = -10;

            if (defeatText != null)
                defeatText.text = $"AĞIR YENİLGİ...\n\nOtağ yasa boğuldu.\n{moralePenalty} Moral\n{repPenalty} İtibar";

            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(moralePenalty);
            if (ReputationManager.Instance != null) ReputationManager.Instance.ChangeReputation(repPenalty);
         

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