using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Sahne değiştirmek için

public class ArcheryGameManager : MonoBehaviour
{
    public static ArcheryGameManager Instance;

  [Header("Oyun Kuralları")]
    public int maxShots = 5;      
    public int targetScore = 20;  
    private int currentShots = 0;
    private int totalScore = 0;

    public bool isGameOver = false; 
    private bool isWon=false;

    [Header("UI Elemanları")]
    public TextMeshProUGUI scoreText; // Sol üstteki Puan yazısı
    public TextMeshProUGUI shotText;  // Sağ üstteki Atış yazısı
    [Header("Ödüller")]
    public int rewardGold = 100;
    public int rewardReputation = 10;
    [Header("Oyun Sonu Paneli")]
 public GameObject resultPanel;     // Panelin kendisi
    public TextMeshProUGUI titleText;  // "Zafer" veya "Yenilgi" başlığı
    public TextMeshProUGUI descText;   // "Puanın: 20" açıklaması
    public TextMeshProUGUI rewardText; // "+100 Altın" yazısı

    void Awake()
    {
        Instance = this;
    }

   void Start()
    {
        // YENİ: Oyun başlar başlamaz imleci kesin olarak kilitle ve gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        isGameOver = false;

        UpdateUI();
        if (resultPanel != null) resultPanel.SetActive(false);

       int soldierSpeed = ArcheryBridge.SoldierSpeed;
string soldierName = ArcheryBridge.SoldierName;
        if (WindManager.Instance != null)
            WindManager.Instance.SetSoldierStat(soldierSpeed);
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
      isGameOver = true; 

        // Oyuncunun tekrar UI'a tıklayabilmesi için fareyi görünür yapıyoruz
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (resultPanel != null) resultPanel.SetActive(true);

        // KAZANDIK MI KAYBETTİK Mİ?
        if (totalScore >= targetScore)
        {
            isWon=true;
           if(titleText) { titleText.text = "TEBRİKLER!";  } // titleText.color = new Color(0.2f, 0.4f, 0.1f);
            if(descText) descText.text = $"Türkmen Beyi yeteneğinden etkilendi. İddialaştığın puanı geçtin.\nToplam Puan: {totalScore}";
            if(rewardText) rewardText.text = $"+{rewardGold} Altın\n+{rewardReputation} İtibar";
           // PlayerPrefs.SetInt("PlayerGold", PlayerPrefs.GetInt("PlayerGold", 0) + rewardGold);
           // PlayerPrefs.SetInt("PlayerReputation", PlayerPrefs.GetInt("PlayerReputation", 0) + rewardReputation);
            //PlayerPrefs.Save();
          //  MoneyManager.Instance.Add(500);
        }
        else
        {
            isWon=false;
            if(titleText) { titleText.text = "BAŞARISIZ"; titleText.color = new Color(0.6f, 0.1f, 0.1f); } // Koyu Kırmızı
            if(descText) descText.text = $"Bey sana güldü, Yeterli puanı toplayamadın.\n(Gereken: {targetScore}, Sen: {totalScore})";
            if(rewardText) rewardText.text = "Ödül Yok";
        }
    }
public void ReturnToMap()
    {
        ArcheryBridge.SetResult(isWon, totalScore);
        SceneManager.LoadScene("CampScene"); 
    }
    // Sonuç panelindeki "Geri Dön" butonuna bunu bağlayacağız
    public void ReturnToMainGame()
    {
       
        SceneManager.LoadScene("CampScene"); 
    }
}