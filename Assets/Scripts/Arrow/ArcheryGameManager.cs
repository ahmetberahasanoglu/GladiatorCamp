using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Sahne değiştirmek için

public class ArcheryGameManager : MonoBehaviour
{
    public static ArcheryGameManager Instance;

    [Header("Oyun Kuralları")]
    public int maxShots = 5;      // Toplam atış hakkı
    public int targetScore = 20;  // Bey'in istediği kazanma puanı
    private int currentShots = 0;
    private int totalScore = 0;

    [Header("UI Elemanları")]
    public TextMeshProUGUI scoreText; // Sol üstteki Puan yazısı
    public TextMeshProUGUI shotText;  // Sağ üstteki Atış yazısı
    
    [Header("Oyun Sonu Paneli")]
    public GameObject resultPanel;    // Kazandın/Kaybettin paneli
    public TextMeshProUGUI resultText;// Paneldeki yazı

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
        if (resultPanel != null) resultPanel.SetActive(false); // Başlangıçta paneli gizle
    }

    // Ok hedefe saplanınca Arrow.cs burayı çağıracak
    public void AddScore(int score)
    {
        totalScore += score;
        UpdateUI();
    }

    // Yaydan ok çıkınca BowController.cs burayı çağıracak
    public void RegisterShot()
    {
        currentShots++;
        UpdateUI();

        // Eğer son oku da attıysak oyunu bitir (Okun uçması için 2 saniye süre veriyoruz)
        if (currentShots >= maxShots)
        {
            Invoke("EndGame", 2f); 
        }
    }

    // Daha ok atabilir miyiz?
    public bool CanShoot()
    {
        return currentShots < maxShots;
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"{totalScore} / {targetScore}";//Puan: 
        if (shotText != null) shotText.text = $"{currentShots} / {maxShots}";//Atış: 
    }

    void EndGame()
    {
        // Oyuncunun tekrar UI'a tıklayabilmesi için fareyi görünür yapıyoruz
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (resultPanel != null) resultPanel.SetActive(true);

        // KAZANDIK MI KAYBETTİK Mİ?
        if (totalScore >= targetScore)
        {
            resultText.text = $"Tebrikler!\n{totalScore} puan topladın.\nTürkmen Beyi yeteneğinden etkilendi!\n\nÖDÜL: +500 Altın, +10 İtibar";
            // İleride buraya MoneyManager.Instance.AddGold(500) gibi kodlar ekleyeceğiz.
        }
        else
        {
            resultText.text = $"Başarısız!\n {totalScore} puan toplayabildin.\nBey sana güldü, itibar kaybettin.\n\nCEZA: -5 İtibar";
        }
    }

    // Sonuç panelindeki "Geri Dön" butonuna bunu bağlayacağız
    public void ReturnToMainGame()
    {
        // BURAYA KENDİ HARİTA SAHNENİN ADINI YAZ (Örn: "MapScene", "SampleScene" vb.)
        SceneManager.LoadScene("MapScene"); 
    }
}