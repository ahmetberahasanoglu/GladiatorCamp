using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class HorseRidingMiniGame : MonoBehaviour
{
    public static HorseRidingMiniGame Instance;

    [Header("UI Elementleri")]
    public GameObject miniGamePanel;
    public RectTransform horseRect;
    public RectTransform obstaclePrefab;
    public RectTransform obstacleContainer;
    public Button jumpButton;
    public TextMeshProUGUI timeText;

    [Header("Başlangıç Ekranı")]
    public GameObject startScreen;          // "Başlamak için tıkla / Space" yazısı
    public TextMeshProUGUI startScreenText; // İsteğe bağlı, Inspector'dan bağla

    [Header("Zemin")]
    public RectTransform ground1;
    public RectTransform ground2;

    [Header("Fizik ve Ayarlar")]
    public float gravity           = -2500f;
    public float jumpForce         = 900f;
    public float baseObstacleSpeed = 500f;
    public float timeToWin         = 15f;

    // ── İç durum ──────────────────────────────────────────────────────────
    private float  _currentSpeed;
    private float  _groundY;
    private float  _groundWidth;
    private float  _spawnX;
    private float  _velocityY;
    private bool   _isGrounded   = true;
    private bool   _isActive     = false;   // gerçek oyun akıyor mu
    private bool   _waitingStart = false;   // başlangıç ekranında mı
    private float  _survivalTimer;
    private float  _spawnTimer;
    private Action<bool> _callback;
    private List<RectTransform> _obstacles = new List<RectTransform>();

    void Awake() { Instance = this; }

    void Start()
    {
        // Jump butonu + klavye her ikisini de destekle
        if (jumpButton) jumpButton.onClick.AddListener(OnJumpInput);
    }

    // ── BAŞLAT ────────────────────────────────────────────────────────────
    public void StartHorseRiding(Gladiator soldier, Action<bool> callback)
    {
        _callback = callback;
        StartCoroutine(InitAfterLayout(soldier));
    }

    private IEnumerator InitAfterLayout(Gladiator soldier)
    {
        miniGamePanel.SetActive(true);

        // Başlangıç ekranını göster
        if (startScreen != null)
        {
            startScreen.SetActive(true);
            if (startScreenText != null)
                startScreenText.text = "Hazır mısın?\n\n<size=80%>Tıkla veya <b>SPACE</b> ile zıpla\nEngellerin üzerinden atla!</size>";
        }

        yield return null;
        yield return null; // Canvas layout için 2 frame bekle

        // Ölçüleri hesapla
        _groundWidth = obstacleContainer != null
            ? obstacleContainer.rect.width
            : ground1.rect.width;
        _spawnX = _groundWidth * 0.5f;
        _groundY = horseRect.anchoredPosition.y;

        // Zemini sıfırla
        ground1.anchoredPosition = new Vector2(0f, ground1.anchoredPosition.y);
        ground2.anchoredPosition = new Vector2(_groundWidth, ground2.anchoredPosition.y);

        // Hız
        float speedMult = Mathf.Clamp(1.3f - (soldier.data.speed / 100f), 0.6f, 1.2f);
        _currentSpeed   = baseObstacleSpeed * speedMult;

        // Sıfırla — oyun henüz başlamadı
        _survivalTimer = 0f;
        _spawnTimer    = 1.5f;
        _velocityY     = 0f;
        _isGrounded    = true;
        _isActive      = false;
        _waitingStart  = true;

        ClearObstacles();
        horseRect.anchoredPosition = new Vector2(horseRect.anchoredPosition.x, _groundY);
        UpdateTimerText();
    }

    // ── UPDATE ────────────────────────────────────────────────────────────
    void Update()
    {
        // Space tuşu desteği
        if (Input.GetKeyDown(KeyCode.Space)) OnJumpInput();

        // Başlangıç ekranında tıklama — mouse veya dokunma
        if (_waitingStart && Input.GetMouseButtonDown(0)) BeginGame();

        if (!_isActive) return;

        _survivalTimer += Time.deltaTime;
        UpdateTimerText();

        if (_survivalTimer >= timeToWin) { Win(); return; }

        MoveGround();
        ApplyGravity();
        MoveObstacles();
        CheckCollisions();
    }

    // ── OYUNU BAŞLAT (ilk tıklama/space) ─────────────────────────────────
    void BeginGame()
    {
        _waitingStart = false;
        _isActive     = true;

        if (startScreen != null) startScreen.SetActive(false);

        // İlk hamleyi de zıplama olarak say
        DoJump();
    }

    // ── ZIPLAMA GİRİŞİ ────────────────────────────────────────────────────
    void OnJumpInput()
    {
        if (_waitingStart) { BeginGame(); return; }
        if (_isActive) DoJump();
    }

    void DoJump()
    {
        if (!_isGrounded) return;
        _velocityY  = jumpForce;
        _isGrounded = false;
    }

    // ── ZEMİN DÖNGÜSÜ ─────────────────────────────────────────────────────
    void MoveGround()
    {
        if (ground1 == null || ground2 == null) return;

        float delta = _currentSpeed * Time.deltaTime;
        ground1.anchoredPosition -= new Vector2(delta, 0);
        ground2.anchoredPosition -= new Vector2(delta, 0);

        float threshold = -_groundWidth;

        if (ground1.anchoredPosition.x <= threshold)
            ground1.anchoredPosition = new Vector2(
                ground2.anchoredPosition.x + _groundWidth,
                ground1.anchoredPosition.y);

        if (ground2.anchoredPosition.x <= threshold)
            ground2.anchoredPosition = new Vector2(
                ground1.anchoredPosition.x + _groundWidth,
                ground2.anchoredPosition.y);
    }

    // ── YERÇEKİMİ ─────────────────────────────────────────────────────────
    void ApplyGravity()
    {
        if (_isGrounded) return;

        _velocityY += gravity * Time.deltaTime;
        Vector2 pos = horseRect.anchoredPosition;
        pos.y += _velocityY * Time.deltaTime;

        if (pos.y <= _groundY)
        {
            pos.y       = _groundY;
            _velocityY  = 0f;
            _isGrounded = true;
        }

        horseRect.anchoredPosition = pos;
    }

    // ── ENGELLER ──────────────────────────────────────────────────────────
    void MoveObstacles()
    {
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            SpawnObstacle();
            _spawnTimer = UnityEngine.Random.Range(1.2f, 2.4f);
        }

        for (int i = _obstacles.Count - 1; i >= 0; i--)
        {
            var obs = _obstacles[i];
            if (obs == null) { _obstacles.RemoveAt(i); continue; }

            obs.anchoredPosition -= new Vector2(_currentSpeed * Time.deltaTime, 0);

            if (obs.anchoredPosition.x < -_spawnX - 200f)
            {
                Destroy(obs.gameObject);
                _obstacles.RemoveAt(i);
            }
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null || obstacleContainer == null) return;

        RectTransform obs = Instantiate(obstaclePrefab, obstacleContainer);
        obs.gameObject.SetActive(true);
        obs.anchoredPosition = new Vector2(_spawnX + 50f, _groundY);
        _obstacles.Add(obs);
    }

    // ── ÇARPIŞMA ──────────────────────────────────────────────────────────
    void CheckCollisions()
    {
        Rect horseR = GetWorldRect(horseRect);
        horseR = new Rect(horseR.x + horseR.width * 0.2f,
                          horseR.y + horseR.height * 0.1f,
                          horseR.width  * 0.6f,
                          horseR.height * 0.8f);

        foreach (var obs in _obstacles)
        {
            if (obs == null) continue;
            Rect obsR = GetWorldRect(obs);
            obsR = new Rect(obsR.x + obsR.width * 0.15f,
                            obsR.y,
                            obsR.width  * 0.7f,
                            obsR.height * 0.9f);

            if (horseR.Overlaps(obsR)) { Lose(); return; }
        }
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        return new Rect(c[0].x, c[0].y,
                        Vector3.Distance(c[0], c[3]),
                        Vector3.Distance(c[0], c[1]));
    }

    // ── BİTİŞ ─────────────────────────────────────────────────────────────
    void Win()
    {
        _isActive     = false;
        _waitingStart = false;
        miniGamePanel.SetActive(false);
        ClearObstacles();
        _callback?.Invoke(true);
    }

    void Lose()
    {
        _isActive     = false;
        _waitingStart = false;
        miniGamePanel.SetActive(false);
        ClearObstacles();
        _callback?.Invoke(false);
    }

    void ClearObstacles()
    {
        foreach (var o in _obstacles) if (o) Destroy(o.gameObject);
        _obstacles.Clear();
    }

    void UpdateTimerText()
    {
        if (timeText) timeText.text = $"Kalan: {Mathf.CeilToInt(timeToWin - _survivalTimer)}s";
    }
}

