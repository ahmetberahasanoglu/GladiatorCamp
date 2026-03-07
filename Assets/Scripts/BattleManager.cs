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
    public GameObject environmentProps; // Ağaçlar, kayalar (Prop grubu)
    public GameObject volumeObject;     // Bu ortama özel Box Volume objesi

    [Header("Işıklandırma")]
    public Color sunColor = Color.white; // Güneşin rengi
    public float sunIntensity = 1.0f;    // Güneşin gücü
    public bool enableShadows = true;    // Mağarada gölgeleri kapatmak isteyebilirsin


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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (skillPanel != null) skillPanel.SetActive(false);
        
    }

    public void StartBattle(int enemyCount, int difficulty,BattleEnvironment envType)
    {
        Debug.Log("BattleManager: Savaş Başlatılıyor...");
        ChangeEnvironment(envType);
        // Zorluk ve sayıyı hafızaya al
        
        _currentEnemyCount = enemyCount;
        _currentDifficulty = difficulty;

        AudioManager.Instance.StartBattleAcoustics();
        
        if (MapManager.Instance != null) MapManager.Instance.HideMap();

        StartCoroutine(CinematicTransitionRoutine(battleCameraPos));

        SpawnPlayerArmy();
        SpawnEnemyArmy(enemyCount);

        state = BattleState.Fighting;
        if (skillPanel != null) skillPanel.SetActive(true);
        topPanel.SetActive(false);
        bgPanel.SetActive(false);
    }
// YENİ: Normal ordular yerine tek bir Devasa Boss çağırır
    public void StartBossBattle(int difficulty, BattleEnvironment envType)
    {
        Debug.Log($"BattleManager: {envType} ortamında BOSS SAVAŞI başlatılıyor!");
        
        _currentDifficulty = difficulty;
        // Boss tek kişi ama ganimet hesaplaması için onu 5 düşman gücünde sayıyoruz!
        _currentEnemyCount = 5; 

        AudioManager.Instance.StartBattleAcoustics();
        // Eğer ileride Boss'a özel müzik eklersen tam buraya yazarsın.
        
        if (MapManager.Instance != null) MapManager.Instance.HideMap();

        ChangeEnvironment(envType);
        StartCoroutine(CinematicTransitionRoutine(battleCameraPos));

        SpawnPlayerArmy();
        SpawnBoss(); // Ordular yerine sadece Boss'u sahanın ortasına indir!

        state = BattleState.Fighting;
        if (skillPanel != null) skillPanel.SetActive(true);
        topPanel.SetActive(false);
        bgPanel.SetActive(false);
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
    }
    void SpawnPlayerArmy()
    {
        var soldiers = FindObjectsOfType<Gladiator>();
        
        if (soldiers.Length == 0) Debug.LogError("HATA: Sahnede hiç asker bulunamadı!");

        int row = 0;
        int col = 0;
        float spacing = 2.0f; 

        foreach(var soldier in soldiers)
        {
            if(soldier != null) 
            {
                Vector3 targetPos = playerSpawnPoint.position;
                targetPos.x += col * spacing; 
                targetPos.z -= row * spacing; 

                var agent = soldier.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.Warp(targetPos); 
                    agent.ResetPath(); 
                    agent.isStopped = true; 
                }
                else
                {
                    soldier.transform.position = targetPos;
                }

                soldier.transform.rotation = playerSpawnPoint.rotation;

                col++;
                if(col > 5) { col = 0; row++; }
            }
        }
    }
private void ChangeEnvironment(BattleEnvironment envType)
    {
        // 1. Önce sahnedeki TÜM ortamları ve Volume'ları kapat (Temizlik)
        foreach (var env in environments)
        {
            if (env.environmentProps != null) env.environmentProps.SetActive(false);
            if (env.volumeObject != null) env.volumeObject.SetActive(false);
        }

        // 2. İstenilen ortamı bul ve aktifleştir
        EnvironmentProfile activeEnv = environments.Find(x => x.envType == envType);

        if (activeEnv != null)
        {
            // Propları ve o ortama özel Box Volume'u aç
            if (activeEnv.environmentProps != null) activeEnv.environmentProps.SetActive(true);
            if (activeEnv.volumeObject != null) activeEnv.volumeObject.SetActive(true);

            // Güneşi manipüle et!
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
            
            col++;
            if(col > 5) { col = 0; row++; }
        }
    }

    public void CheckBattleStatus()
    {
        if (state != BattleState.Fighting) return;

        var allUnits = FindObjectsOfType<GladiatorAI>();

        int livingMySoldiers = 0;
        int livingEnemies = 0;

        foreach (var unit in allUnits)
        {
            if (!unit.isDead)
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
            // --- DİNAMİK AAA ÖDÜL HESAPLAMASI ---
            int goldReward = _currentEnemyCount * 50 * _currentDifficulty; // Örn: 5 Düşman * Zorluk 2 = 500 Akçe
            int foodReward = _currentEnemyCount * 10;                      // Düşman kampından yağmalanan erzak
            int moraleReward = 10 + (_currentDifficulty * 5);              // Zor savaş kazanmak daha çok moral verir
            int repReward = 5 * _currentDifficulty;                        // İtibar artışı

            if (lootText != null)
                lootText.text = $"ZAFER!\n\n+{goldReward} Akçe\n+{foodReward} Erzak\n+{repReward} İtibar\n+{moraleReward} Moral";

            // SİSTEMLERE DAĞITIM (PlayerPrefs TAMAMEN KALDIRILDI)
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
            // --- DİNAMİK AAA YENİLGİ CEZASI ---
            int moralePenalty = -20 - (_currentDifficulty * 5); 
            int repPenalty = -10;

            if (defeatText != null)
                defeatText.text = $"AĞIR YENİLGİ...\n\nOtağ yasa boğuldu.\n{moralePenalty} Moral\n{repPenalty} İtibar";

            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(moralePenalty);
            
            // İLERİDE EKLENECEK SİSTEM İÇİN:
            // if (ReputationManager.Instance != null) ReputationManager.Instance.AddReputation(repPenalty);

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
        if (MapManager.Instance != null) MapManager.Instance.HideMap(); 

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

        StartCoroutine(CinematicTransitionRoutine(campCameraPos));
        
        state = BattleState.Idle;
    }

    IEnumerator CinematicTransitionRoutine(Transform targetCameraPos)
    {
        // 1. EKRANI KARART (0.5 Saniyede)
        float t = 0;
        float fadeDuration = 0.5f;

        // Ekrana dokunmayı engelle ki geçiş anında oyuncu bir şeye basamasın
        if (fadeGroup != null) fadeGroup.blocksRaycasts = true;

        while(t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (fadeGroup != null) fadeGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        if (fadeGroup != null) fadeGroup.alpha = 1;

        // 2. SİHİR ANI: EKRAN SİMSİYAHKEN HER ŞEYİ ANINDA IŞINLA
        // Kamera boşlukta süzülmez, direkt hedefe yapışır
        mainCamera.transform.position = targetCameraPos.position;
        mainCamera.transform.rotation = targetCameraPos.rotation;

        // İstersen burada karanlıkta savaş naraları/kılıç çekme sesleri patlatabilirsin
        // AudioManager.Instance.PlaySwordDraw();
        
        // Seyircinin (Oyuncunun) zihninde sahnenin değiştiğini algılaması için çok ufak bir es (0.2s)
        yield return new WaitForSecondsRealtime(0.2f);

        // 3. EKRANI AYDINLAT (Savaş başlasın!)
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
            fadeGroup.blocksRaycasts = false; // Tıklama engelini kaldır
        }
    }
}