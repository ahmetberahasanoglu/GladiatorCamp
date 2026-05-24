using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WrestlingMiniGame : MonoBehaviour
{
    public static WrestlingMiniGame Instance;
    [Header("UI Elementleri")]
    public GameObject miniGamePanel;
    public Slider timingSlider; // Sağa sola giden ibre
    public RectTransform sweetSpotRect; // Yeşil alanın UI objesi
    public Button actionButton;
    public TextMeshProUGUI scoreText;
    private Action<bool> onGameFinished;
    

    [Header("Oyun Ayarları")]
    public float baseSliderSpeed = 2f;
    private float currentSliderSpeed;
    private bool movingRight = true;
    private bool isGameActive = false;

    private int successfulHits = 0;
    private int requiredHits = 3; // Kazanmak için 3 kere tutturmalı

    // Yeşil alanın sınırları
    private float minSuccessValue;
    private float maxSuccessValue;
    void Awake()
    {
        Instance=this;
    }
    void Start()
    {
        actionButton.onClick.AddListener(TryHit);
    }

    // Etkinlikten "Güreşçi Gönder" dendiğinde bu fonksiyon çalışacak
   // Artık rakibin gücünü de alıyoruz
    public void StartWrestling(Gladiator selectedSoldier, int opponentStrength,Action<bool> callback)
    {
        onGameFinished = callback;
        successfulHits = 0;
        UpdateScoreText();
        miniGamePanel.SetActive(true);

        // --- RYO MATEMATİĞİ: BİZİM GÜCÜMÜZ vs RAKİBİN GÜCÜ ---
        // Eğer bizim asker 30, rakip 10 ise oran 3.0 olur (Çok kolay).
        // Bizim asker 10, rakip 40 ise oran 0.25 olur (Çok zor).
        float powerRatio = (float)selectedSoldier.data.strength / (float)Mathf.Max(1, opponentStrength);
        
        // Çarpanı 0.1 (Çok ince) ile 0.8 (Çok geniş) arasında sınırla ki oyun bozulmasın
        float sweetSpotSizeMultiplier = Mathf.Clamp(powerRatio * 0.3f, 0.1f, 0.8f); 
        // -----------------------------------------------------

        // Yeşil alanın görsel genişliğini (Width) ayarla
        float totalSliderWidth = timingSlider.GetComponent<RectTransform>().rect.width;
        sweetSpotRect.sizeDelta = new Vector2(totalSliderWidth * sweetSpotSizeMultiplier, sweetSpotRect.sizeDelta.y);

        // Slider değer aralığını (0 ile 1 arası) yeşil alana göre hesapla
        float center = 0.5f; 
        float halfSize = sweetSpotSizeMultiplier / 2f;
        minSuccessValue = center - halfSize;
        maxSuccessValue = center + halfSize;

        currentSliderSpeed = baseSliderSpeed;
        isGameActive = true;
    }

    void Update()
    {
        if (!isGameActive) return;

        // İbreyi sağa sola hareket ettir (PingPong Mantığı)
        if (movingRight)
        {
            timingSlider.value += currentSliderSpeed * Time.deltaTime;
            if (timingSlider.value >= 1f) movingRight = false;
        }
        else
        {
            timingSlider.value -= currentSliderSpeed * Time.deltaTime;
            if (timingSlider.value <= 0f) movingRight = true;
        }
    }

    void TryHit()
    {
        if (!isGameActive) return;

        // Oyuncu butona bastığında ibre yeşil alanın içinde mi?
        if (timingSlider.value >= minSuccessValue && timingSlider.value <= maxSuccessValue)
        {
            successfulHits++;
            UpdateScoreText();
            
            // Varsa seyirci alkış sesi çal!
            // AudioManager.Instance.PlaySFX(AudioManager.Instance.cheerSound);

            if (successfulHits >= requiredHits)
            {
                WinMiniGame();
            }
            else
            {
                // Her başarılı vuruşta oyun biraz hızlansın (Heyecan artsın)
                currentSliderSpeed += 0.5f; 
            }
        }
        else
        {
            // Iskaldı!
            LoseMiniGame();
        }
    }

    void UpdateScoreText()
    {
        if (scoreText) scoreText.text = $"Başarılı Hamle: {successfulHits} / {requiredHits}";
    }

   void WinMiniGame()
    {
        isGameActive = false;
        CloseMiniGame();
        onGameFinished?.Invoke(true); // Etkinlik paneline "KAZANDI" (true) mesajı gönder!
    }

    void LoseMiniGame()
    {
        isGameActive = false;
        CloseMiniGame();
        onGameFinished?.Invoke(false); // Etkinlik paneline "KAYBETTİ" (false) mesajı gönder!
    }

    void CloseMiniGame()
    {
        miniGamePanel.SetActive(false);
        // Harita ekranına veya event sonuç ekranına geri dön
    }
}