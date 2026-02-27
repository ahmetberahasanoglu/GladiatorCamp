using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CenkGameManager : MonoBehaviour
{
    public static CenkGameManager Instance;

    [Header("Ana Paneller")]
    public GameObject cenkPanel; // Oyunun ana ekranı (Canvas)
    public TextMeshProUGUI infoText; // "Sıra Sende", "Kazandın!" gibi yazılar
    public GameObject closeButton; // Paneli kapatma butonu

    [Header("Kart Alanları (Transformlar)")]
    public Transform playerHandArea;
    public Transform enemyHandArea;
    public Transform playerBoardArea; // Oyuncunun attığı kartların duracağı yer
    public Transform enemyBoardArea;  // Düşmanın attığı kartların duracağı yer

    [Header("Skorlar")]
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI enemyScoreText;

    [Header("Prefab")]
    public GameObject cardPrefab;

    // Destemizdeki temel kart türleri
    private string[] cardNames = { "Acemi (2)", "Piyade (4)", "Okçu (5)", "Süvari (7)", "Silahtar (9)" };
    private int[] cardPowers = { 2, 4, 5, 7, 9 };

    private int playerScore = 0;
    private int enemyScore = 0;
    private int turnCount = 0;

    private List<CenkCardUI> enemyHandCards = new List<CenkCardUI>();

    void Awake()
    {
        Instance = this;
        if (cenkPanel != null) cenkPanel.SetActive(false);
    }

    public void OpenMinigame()
    {
        cenkPanel.SetActive(true);
        closeButton.SetActive(false);
        StartNewGame();
    }

    public void CloseMinigame()
    {
        cenkPanel.SetActive(false);
    }

    void StartNewGame()
    {
        // Temizlik
        ClearArea(playerHandArea); ClearArea(enemyHandArea);
        ClearArea(playerBoardArea); ClearArea(enemyBoardArea);
        enemyHandCards.Clear();

        playerScore = 0;
        enemyScore = 0;
        turnCount = 0;
        UpdateScores();

        infoText.text = "Oyun Başladı! Kartını Seç.";

        // Herkese 3 rastgele kart dağıt
        for (int i = 0; i < 3; i++)
        {
            CreateRandomCard(playerHandArea, true);
            CenkCardUI enemyCard = CreateRandomCard(enemyHandArea, false);
            enemyHandCards.Add(enemyCard);
        }
    }

    CenkCardUI CreateRandomCard(Transform parentArea, bool isPlayer)
    {
        int randomIndex = Random.Range(0, cardNames.Length);
        
        GameObject newCardObj = Instantiate(cardPrefab, parentArea);
        CenkCardUI cardUI = newCardObj.GetComponent<CenkCardUI>();
        
        cardUI.SetupCard(cardNames[randomIndex], cardPowers[randomIndex], isPlayer);
        return cardUI;
    }

    // --- OYUNCU HAMLESİ ---
    public void PlayerPlaysCard(CenkCardUI playedCard)
    {
        // 1. Kartı sahaya sür
        playedCard.transform.SetParent(playerBoardArea);
        playedCard.cardButton.interactable = false; // Bir daha tıklanmasın

        // 2. Skoru ekle
        playerScore += playedCard.cardPower;
        UpdateScores();

        // 3. Sırayı düşmana ver
        StartCoroutine(EnemyTurnRoutine());
    }

    // --- DÜŞMAN HAMLESİ ---
    IEnumerator EnemyTurnRoutine()
    {
        infoText.text = "Rakip düşünüyor...";
        
        // Oyuncunun art arda tıklamasını önlemek için ufak bir bekleme
        yield return new WaitForSeconds(1.0f);

        // Düşmanın elindeki rastgele bir kartı seç
        if (enemyHandCards.Count > 0)
        {
            int randomIndex = Random.Range(0, enemyHandCards.Count);
            CenkCardUI chosenCard = enemyHandCards[randomIndex];
            enemyHandCards.RemoveAt(randomIndex);

            // Kartı sahaya sür ve yüzünü çevir
            chosenCard.transform.SetParent(enemyBoardArea);
            chosenCard.RevealEnemyCard();

            // Skoru ekle
            enemyScore += chosenCard.cardPower;
            UpdateScores();
        }

        turnCount++;
        
        if (turnCount >= 3)
        {
            EndGame();
        }
        else
        {
            infoText.text = "Sıra Sende. Kartını Seç.";
        }
    }

    void EndGame()
    {
        closeButton.SetActive(true);

        if (playerScore > enemyScore)
        {
            infoText.text = "<color=green>ZAFER!</color> Askerlerin morali yükseldi!";
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(15);
        }
        else if (playerScore < enemyScore)
        {
            infoText.text = "<color=red>YENİLGİ!</color> Biraz altın kaybettin.";
            // (İsteğe bağlı) MoneyManager.Instance.Spend(10); 
            // Veya moral düşürebilirsin: CampMoraleManager.Instance.ChangeMorale(-5);
        }
        else
        {
            infoText.text = "<color=yellow>BERABERLİK.</color> Dostane bir maçtı.";
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(5);
        }
    }

    void UpdateScores()
    {
        playerScoreText.text = $"Güç: {playerScore}";
        enemyScoreText.text = $"Güç: {enemyScore}";
    }

    void ClearArea(Transform area)
    {
        foreach (Transform child in area)
        {
            Destroy(child.gameObject);
        }
    }
}