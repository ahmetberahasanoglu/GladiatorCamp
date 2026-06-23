using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gece Baskını sahnesi yöneticisi.
/// 3 dalga halinde düşman gelir, her dalga arasında 10 saniyelik mola.
/// MapEventManager bu sahneyi SceneManager.LoadScene("NightRaid") ile açar,
/// sonucu PlayerPrefs üzerinden alır.
/// </summary>
public class NightRaidManager : MonoBehaviour
{
    public static NightRaidManager Instance;

    // ── SPAWN NOKTALARI ───────────────────────────────────────────────────
    [Header("Spawn Noktaları (4 yön)")]
    public Transform spawnNorth;
    public Transform spawnSouth;
    public Transform spawnEast;
    public Transform spawnWest;

    [Header("Oyuncu Spawn")]
    public Transform playerSpawnCenter; // Askerler kampın ortasına yerleşir

    // ── PREFABLAR ─────────────────────────────────────────────────────────
    [Header("Prefablar")]
    public GameObject enemyPrefab;
    public GameObject eliteEnemyPrefab; // 3. dalga için güçlü düşman

    // ── DALGA AYARLARI ────────────────────────────────────────────────────
    [Header("Dalga Ayarları")]
    public int[] enemiesPerWave = { 3, 4, 5 }; // Her dalgadaki düşman sayısı
    public float breakDuration  = 10f;           // Dalgalar arası süre (saniye)

    // ── UI ────────────────────────────────────────────────────────────────
    [Header("UI")]
    public TextMeshProUGUI waveText;        // "DALGA 1 / 3"
    public TextMeshProUGUI timerText;       // Mola geri sayımı
    public TextMeshProUGUI statusText;      // "Düşmanlar geliyor!" / "Nefes al..."
    public GameObject      breakPanel;     // Mola ekranı
    public GameObject      resultPanel;    // Kazandın / Kaybettin ekranı
    public TextMeshProUGUI resultText;
    public Button          returnButton;   // Kampa dön butonu

    [Header("Kamera")]
    public Camera battleCamera;
    public Vector3 overheadPosition  = new Vector3(0, 18, 0);
    public Vector3 overheadRotation  = new Vector3(90, 0, 0);
    public Vector3 normalPosition    = new Vector3(0, 8, -10);
    public Vector3 normalRotation    = new Vector3(25, 0, 0);

    // ── İÇ DURUM ──────────────────────────────────────────────────────────
    private int   _currentWave      = 0;
    private int   _totalWaves;
    private bool  _raidActive       = false;
    private bool  _playerDefeated   = false;
    private List<GameObject> _activeEnemies = new List<GameObject>();
    private List<Gladiator>  _playerSoldiers = new List<Gladiator>();

    // ── AWAKE / START ──────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _totalWaves = enemiesPerWave.Length;

        // Tier'a göre dalga boyutlarını ayarla
        int tier = PlayerPrefs.GetInt("NightRaidTier", 1);
        for (int i = 0; i < enemiesPerWave.Length; i++)
            enemiesPerWave[i] = Mathf.Max(2, (i + 2) + (tier - 1));

        if (resultPanel != null) resultPanel.SetActive(false);
        if (breakPanel  != null) breakPanel.SetActive(false);
        if (timerText   != null) timerText.gameObject.SetActive(false);
        if (returnButton != null) returnButton.onClick.AddListener(ReturnToCamp);

        // Oyuncunun askerlerini bul ve yerleştir
        SetupPlayerSoldiers();

        // 2 saniye giriş gecikmesiyle ilk dalgayı başlat
        StartCoroutine(RaidSequence());
    }

    // ── ASKER KURULUMU ────────────────────────────────────────────────────
    void SetupPlayerSoldiers()
    {
        // PlayerPrefs'ten hangi askerlerin seçildiğini oku
        // MapEventManager savaşa giren askerlerin isimlerini kaydeder
        var soldiers = FindObjectsByType<Gladiator>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        float spacing = 2.5f;
        int   idx     = 0;

        foreach (var s in soldiers)
        {
            if (!s.CompareTag("MySoldier") || s.data == null
                || s.data.currentHealth <= 0) continue;

            _playerSoldiers.Add(s);

            // Kampın ortasında yarım daire şeklinde diz
            float angle = idx * (180f / Mathf.Max(1, soldiers.Length - 1)) - 90f;
            float rad   = angle * Mathf.Deg2Rad;
            Vector3 pos = playerSpawnCenter != null
                ? playerSpawnCenter.position + new Vector3(
                    Mathf.Sin(rad) * spacing, 0, Mathf.Cos(rad) * spacing * 0.5f)
                : new Vector3(idx * spacing, 0, 0);

            var agent = s.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.Warp(pos);
            else s.transform.position = pos;

            var ai = s.GetComponent<GladiatorAI>();
            if (ai != null) ai.isInBattle = true;

            idx++;
        }
    }

    // ── ANA DALGA SIRASI ──────────────────────────────────────────────────
    IEnumerator RaidSequence()
    {
        yield return new WaitForSeconds(2f);

        for (int wave = 0; wave < _totalWaves; wave++)
        {
            _currentWave = wave + 1;
            UpdateWaveUI($"DALGA {_currentWave} / {_totalWaves}", "Düşmanlar geliyor!");

            // Kamerayı yukarıdan göster (dalga gelişini hissettir)
            yield return StartCoroutine(MoveCameraTo(overheadPosition, overheadRotation, 0.8f));
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(MoveCameraTo(normalPosition, normalRotation, 0.8f));

            // Düşmanları spawn et
            SpawnWave(wave);

            _raidActive = true;

            // Tüm düşmanlar ölünce veya oyuncu yenilince devam et
            yield return StartCoroutine(WaitForWaveEnd());

            if (_playerDefeated) break;

            _raidActive = false;

            // Son dalga değilse mola ver
            if (wave < _totalWaves - 1)
            {
                yield return StartCoroutine(BreakPhase());
            }
        }

        // Sonuç
        if (_playerDefeated)
            ShowResult(false);
        else
            ShowResult(true);
    }

    // ── DALGA SPAWN ───────────────────────────────────────────────────────
    void SpawnWave(int waveIndex)
    {
        _activeEnemies.Clear();

        int count  = enemiesPerWave[Mathf.Clamp(waveIndex, 0, enemiesPerWave.Length - 1)];
        bool elite = waveIndex >= _totalWaves - 1; // Son dalga elite

        // Spawn noktalarını listele
        var points = new List<Transform>();
        if (spawnNorth != null) points.Add(spawnNorth);
        if (spawnSouth != null) points.Add(spawnSouth);
        if (spawnEast  != null) points.Add(spawnEast);
        if (spawnWest  != null) points.Add(spawnWest);

        if (points.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            Transform spawnPt = points[i % points.Count];
            GameObject prefab = (elite && eliteEnemyPrefab != null)
                ? eliteEnemyPrefab : enemyPrefab;

            if (prefab == null) continue;

            // Spawn noktasının etrafına hafif offset
            Vector3 offset = new Vector3(
                Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));

            GameObject enemy = Instantiate(prefab, spawnPt.position + offset,
                                           spawnPt.rotation);
            enemy.tag = "Enemy";

            var ai = enemy.GetComponent<GladiatorAI>();
            if (ai != null) ai.isInBattle = true;

            _activeEnemies.Add(enemy);
        }
    }

    // ── DALGA BİTİŞİ BEKLEMESİ ───────────────────────────────────────────
    IEnumerator WaitForWaveEnd()
    {
        while (true)
        {
            // Oyuncu askerleri kontrol
            bool allPlayersDead = true;
            foreach (var s in _playerSoldiers)
            {
                if (s != null && s.currentHealth > 0)
                { allPlayersDead = false; break; }
            }

            if (allPlayersDead)
            {
                _playerDefeated = true;
                yield break;
            }

            // Düşmanlar ölü mü?
            _activeEnemies.RemoveAll(e => e == null);
            bool allEnemiesDead = true;
            foreach (var e in _activeEnemies)
            {
                if (e != null && e.activeInHierarchy)
                {
                    var g = e.GetComponent<Gladiator>();
                    if (g != null && g.currentHealth > 0)
                    { allEnemiesDead = false; break; }
                }
            }

            if (allEnemiesDead) yield break;

            yield return new WaitForSeconds(0.5f);
        }
    }

    // ── MOLA FAZASI ───────────────────────────────────────────────────────
    IEnumerator BreakPhase()
    {
        if (breakPanel != null) breakPanel.SetActive(true);
        if (timerText  != null) timerText.gameObject.SetActive(true);

        UpdateWaveUI($"DALGA {_currentWave} Bitti!", "Nefes al...");

        float remaining = breakDuration;
        while (remaining > 0f)
        {
            if (timerText != null)
                timerText.text = $"Sonraki dalga: {Mathf.CeilToInt(remaining)}s";

            remaining -= Time.deltaTime;
            yield return null;
        }

        if (breakPanel != null) breakPanel.SetActive(false);
        if (timerText  != null) timerText.gameObject.SetActive(false);
    }

    // ── SONUÇ ─────────────────────────────────────────────────────────────
    void ShowResult(bool victory)
    {
        if (resultPanel != null) resultPanel.SetActive(true);

        // Kamerayı yukarıdan göster
        StartCoroutine(MoveCameraTo(overheadPosition, overheadRotation, 1.2f));

        if (victory)
        {
            if (resultText != null)
                resultText.text =
                    "BASKIN SAVUŞTURULDU!\n\n" +
                    "Askerlerin gecenin karanlığında düşmanı püskürttü.\n\n" +
                    "+30 İtibar  ·  +15 Moral  ·  Ganimet Arabası Hazır";

            // Ödüller PlayerPrefs'e — ReturnToCamp'ta uygulanır
            PlayerPrefs.SetInt("NightRaidResult", 1);
            PlayerPrefs.SetInt("NightRaidGold",   150 + (_currentWave * 30));
        }
        else
        {
            if (resultText != null)
                resultText.text =
                    "BASKINA UĞRADIN!\n\n" +
                    "Düşman kamp içine girdi. Çekilmek zorunda kaldınız.\n\n" +
                    "−20 İtibar  ·  −10 Moral";

            PlayerPrefs.SetInt("NightRaidResult", 0);
            PlayerPrefs.SetInt("NightRaidGold",   0);
        }

        PlayerPrefs.Save();
    }

    // ── KAMPA DÖN ─────────────────────────────────────────────────────────
    void ReturnToCamp()
    {
        // Sonuçları uygula
        int result = PlayerPrefs.GetInt("NightRaidResult", 0);
        int gold   = PlayerPrefs.GetInt("NightRaidGold", 0);

        if (result == 1)
        {
            if (MoneyManager.Instance        != null) MoneyManager.Instance.Add(gold);
            if (ReputationManager.Instance   != null) ReputationManager.Instance.ChangeReputation(30);
            if (CampMoraleManager.Instance   != null) CampMoraleManager.Instance.ChangeMorale(15);
        }
        else
        {
            if (ReputationManager.Instance   != null) ReputationManager.Instance.ChangeReputation(-20);
            if (CampMoraleManager.Instance   != null) CampMoraleManager.Instance.ChangeMorale(-10);
        }

      if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene"); // Veya harita sahnenin adı neyse
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CampScene");
        }
    }

    // ── KAMERA HAREKET ────────────────────────────────────────────────────
    IEnumerator MoveCameraTo(Vector3 targetPos, Vector3 targetRot, float duration)
    {
        if (battleCamera == null) yield break;

        Vector3 startPos = battleCamera.transform.position;
        Quaternion startRot = battleCamera.transform.rotation;
        Quaternion endRot   = Quaternion.Euler(targetRot);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float pct = Mathf.SmoothStep(0f, 1f, t / duration);
            battleCamera.transform.position = Vector3.Lerp(startPos, targetPos, pct);
            battleCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, pct);
            yield return null;
        }

        battleCamera.transform.position = targetPos;
        battleCamera.transform.rotation = endRot;
    }

    // ── UI YARDIMCI ───────────────────────────────────────────────────────
    void UpdateWaveUI(string wave, string status)
    {
        if (waveText   != null) waveText.text   = wave;
        if (statusText != null) statusText.text = status;
    }
}