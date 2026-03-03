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
    public Transform playerSpawnPoint; // Askerlerin dizileceği yer
    public Transform enemySpawnPoint;  // Düşmanların dizileceği yer
    public Camera mainCamera;          
    public GameObject enemyPrefab;     // Düşman Askerinin Prefabı (BUNU ATAMALISIN!)

    [Header("Kamera Pozisyonları")]
    public Transform campCameraPos;    // Kamp açısı
    public Transform battleCameraPos;  // Savaş alanı açısı

    [Header("Durum")]
    public BattleState state = BattleState.Idle;

    [Header("UI Panelleri")]
    public GameObject lootPanel;      // Zafer ekranı (Ganimet)
    public GameObject defeatPanel;    // Yenilgi ekranı
    public GameObject bgPanel;
    public GameObject topPanel;
    public TextMeshProUGUI lootText;


public GameObject skillPanel;
    void Awake()
    {
        Instance = this;
    }

    public void StartBattle(int enemyCount, int difficulty)
    {
        Debug.Log("BattleManager: Savaş Başlatılıyor...");
        AudioManager.Instance.StartBattleAcoustics();
        // 1. Haritayı Kapat
        if (MapManager.Instance != null) MapManager.Instance.HideMap();

        // 2. Kamerayı Taşı (UnscaledTime kullanarak, oyun donuksa bile çalışır)
        StartCoroutine(MoveCameraRoutine(battleCameraPos));

        // 3. Orduları Sahaya Sür
        SpawnPlayerArmy();
        SpawnEnemyArmy(enemyCount);

        state = BattleState.Fighting;
        if (skillPanel != null) skillPanel.SetActive(true);
        topPanel.SetActive(false);
        bgPanel.SetActive(false);
    }
void Start()
    {
        // Oyun başladığında Skill paneli kapalı olsun (Çünkü kamptayız)
        if (skillPanel != null) skillPanel.SetActive(false);
    }
   void SpawnPlayerArmy()
    {
        var soldiers = FindObjectsOfType<Gladiator>();
        
        if (soldiers.Length == 0) Debug.LogError("HATA: Sahnede hiç asker bulunamadı!");

        int row = 0;
        int col = 0;
        float spacing = 2.0f; // Askerler arası biraz daha boşluk verelim

        foreach(var soldier in soldiers)
        {
            if(soldier != null) 
            {
                // Hedef Pozisyonu Hesapla
                Vector3 targetPos = playerSpawnPoint.position;
                targetPos.x += col * spacing; 
                targetPos.z -= row * spacing; 

                // --- DÜZELTME BURADA ---
                // NavMeshAgent varsa Warp kullanmalıyız, yoksa çalışmaz!
                var agent = soldier.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    // 1. Önce ajanı kapatırsak daha garanti olur ama Warp genelde yeterlidir.
                    agent.Warp(targetPos); 
                    
                    // 2. Eski yürüyüş hedefini iptal et (Yoksa kampa geri koşmaya çalışabilir)
                    agent.ResetPath(); 
                    
                    // 3. Durduralım (Savaş başlayana kadar beklemesi için)
                    agent.isStopped = true; 
                }
                else
                {
                    // Agent yoksa düz taşı
                    soldier.transform.position = targetPos;
                }
                // -----------------------

                // Yönünü düşmana çevir
                soldier.transform.rotation = playerSpawnPoint.rotation;

                // Grid hesabı
                col++;
                if(col > 5) { col = 0; row++; }
            }
        }
        Debug.Log("Bizim askerler IŞINLANDI.");
    }

public void EndBattle(bool isVictory)
    {
        state = isVictory ? BattleState.Won : BattleState.Lost;

        if (skillPanel != null) skillPanel.SetActive(false);
        topPanel.SetActive(true);

        if (isVictory)
        {
            // Ödül Hesapla (Örn: Düşman başı 50 altın)
            int goldReward = 300; // Şimdilik sabit
            lootText.text = $"ZAFER!\n\nKazanılan: {goldReward} Akçe\nİtibar Arttı!";
            
            // Parayı cüzdana ekle (Burada MoneyManager çağrılmalı)
            if(MoneyManager.Instance != null)
            {
                int currentPending = PlayerPrefs.GetInt("PendingGold", 0);
    PlayerPrefs.SetInt("PendingGold", currentPending + goldReward);
     //MoneyManager.Instance.Add(goldReward);
    PlayerPrefs.Save();
            }
           

            lootPanel.SetActive(true); // Ganimet panelini aç
        }
        else
        {
            defeatPanel.SetActive(true); // Yenilgi panelini aç
        }
    }
    public void CheckBattleStatus()
    {
        // Eğer savaş zaten bittiyse tekrar kontrol etme
        if (state != BattleState.Fighting) return;

        // Sahnedeki tüm AI scriptlerini bul
        var allUnits = FindObjectsOfType<GladiatorAI>();

        int livingMySoldiers = 0;
        int livingEnemies = 0;

        foreach (var unit in allUnits)
        {
            // Eğer ölü değilse say
            if (!unit.isDead)
            {
                if (unit.CompareTag("MySoldier"))
                {
                    livingMySoldiers++;
                }
                else if (unit.CompareTag("EnemySoldier"))
                {
                    livingEnemies++;
                }
            }
        }

        // --- KARAR ANI ---

        // 1. Durum: Düşmanların hepsi yerde -> ZAFER
        if (livingEnemies <= 0)
        {
            Debug.Log("Düşman kalmadı! Zafer!");
            EndBattle(true); // Zafer parametresiyle bitir
        }
        // 2. Durum: Bizimkilerin hepsi yerde -> YENİLGİ
        else if (livingMySoldiers <= 0)
        {
            Debug.Log("Asker kalmadı! Yenilgi!");
            EndBattle(false); // Yenilgi parametresiyle bitir
            if (MapManager.Instance != null)
            {
    MapManager.Instance.RetreatToPreviousNode();
            }
        }
    }

    // "Kampa Dön" butonuna bağlanacak fonksiyon
  public void ReturnToCamp()
    {
        // 1. UI Panellerini Kapat
        lootPanel.SetActive(false);
        defeatPanel.SetActive(false);
        bgPanel.SetActive(false);
        // Tekrar huzurlu kamp müziğine dön (Kış ise kış müziği çalar)
    bool isWinter = DayManager.Instance.currentDay >= DayManager.Instance.maxDays;
    AudioManager.Instance.PlayCampMusic(isWinter);
        if (skillPanel != null) skillPanel.SetActive(false);
        topPanel.SetActive(true);
        if (MapManager.Instance != null) MapManager.Instance.HideMap(); // Haritayı da kapat

        // 2. Sahnedeki TÜM askerleri (Ölü/Diri) bul
        var allUnits = FindObjectsOfType<GladiatorAI>();
        
        // Kamp dizilimi için basit matematik değişkenleri
        int row = 0;
        int col = 0;
        float spacing = 1.5f;

        foreach (var unit in allUnits)
        {
            // --- DÜŞMANLAR YOK OLACAK ---
            if (unit.CompareTag("EnemySoldier"))
            {
                // Hafızadan sil
                Destroy(unit.gameObject);
            }
            // --- BİZİMKİLER KAMPA DÖNECEK ---
            else if (unit.CompareTag("MySoldier"))
            {
                // 1. Askeri "Canlı/Ayakta" moduna resetle
                unit.ReviveForCamp(); 

                // 2. Kampa Işınla (Warp kullanmak şart!)
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

                    // Yönünü düzelt (Kameraya baksınlar)
                    unit.transform.rotation = campStandingPoint.rotation;

                    // Sıraya diz
                    col++;
                    if(col > 5) { col = 0; row++; }
                }
            }
        }

        // 3. Kamerayı Kampa Çevir
        StartCoroutine(MoveCameraRoutine(campCameraPos));
        
        state = BattleState.Idle;
        
        Debug.Log("Düşmanlar temizlendi, ordu kampa döndü.");
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
            
            // --- DÜŞMAN İÇİN DE AYNI KONTROL ---
            var agent = newEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                // Instantiate bazen NavMesh'e tam oturmaz, Warp ile garantiye alalım
                agent.Warp(spawnPos); 
                // Şimdilik dursunlar
                agent.isStopped = true; 
            }
            
            col++;
            if(col > 5) { col = 0; row++; }
        }
    }

    IEnumerator MoveCameraRoutine(Transform target)
    {
        float duration = 1.5f; // 1.5 saniyede gitsin
        float t = 0;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while(t < duration)
        {
            // UnscaledDeltaTime: Oyun hızı 0 olsa bile kamera hareket eder
            t += Time.unscaledDeltaTime; 
            
            mainCamera.transform.position = Vector3.Lerp(startPos, target.position, t / duration);
            mainCamera.transform.rotation = Quaternion.Lerp(startRot, target.rotation, t / duration);
            yield return null;
        }
        
        mainCamera.transform.position = target.position;
        mainCamera.transform.rotation = target.rotation;
    }
}