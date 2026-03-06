using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum BattleState { Idle, Fighting, Won, Lost }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    
    [Header("Kamp Dizilimi")]
    public Transform campStandingPoint;

    [Header("Gerekli Referanslar")]
    public Transform playerSpawnPoint; 
    public Transform enemySpawnPoint;  
    public Camera mainCamera;          
    public GameObject enemyPrefab;     

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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (skillPanel != null) skillPanel.SetActive(false);
    }

    public void StartBattle(int enemyCount, int difficulty)
    {
        Debug.Log("BattleManager: Savaş Başlatılıyor...");
        
        // Zorluk ve sayıyı hafızaya al
        _currentEnemyCount = enemyCount;
        _currentDifficulty = difficulty;

        AudioManager.Instance.StartBattleAcoustics();
        
        if (MapManager.Instance != null) MapManager.Instance.HideMap();

        StartCoroutine(MoveCameraRoutine(battleCameraPos));

        SpawnPlayerArmy();
        SpawnEnemyArmy(enemyCount);

        state = BattleState.Fighting;
        if (skillPanel != null) skillPanel.SetActive(true);
        topPanel.SetActive(false);
        bgPanel.SetActive(false);
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

        StartCoroutine(MoveCameraRoutine(campCameraPos));
        
        state = BattleState.Idle;
    }

    IEnumerator MoveCameraRoutine(Transform target)
    {
        float duration = 1.5f; 
        float t = 0;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while(t < duration)
        {
            t += Time.unscaledDeltaTime; 
            mainCamera.transform.position = Vector3.Lerp(startPos, target.position, t / duration);
            mainCamera.transform.rotation = Quaternion.Lerp(startRot, target.rotation, t / duration);
            yield return null;
        }
        
        mainCamera.transform.position = target.position;
        mainCamera.transform.rotation = target.rotation;
    }
}