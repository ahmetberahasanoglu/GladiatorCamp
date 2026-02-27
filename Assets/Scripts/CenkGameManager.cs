using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class CenkGameManager : MonoBehaviour
{
    public static CenkGameManager Instance;

    [Header("UI Panelleri")]
    public GameObject cenkPanel;
    public TextMeshProUGUI infoText;
    public GameObject closeButton;
    public Button passButton; 

    [Header("Kart Alanları")]
    public Transform enemyHandArea;   
    public Transform enemyBoardArea;  
    public Transform playerBoardArea; 
    public Transform playerHandArea;  

    [Header("Skorlar ve Taçlar")]
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI enemyScoreText;
    public GameObject[] playerCrowns; 
    public GameObject[] enemyCrowns;

    [Header("Kart Sayacı (YENİ)")]
    public TextMeshProUGUI playerCardCountText; // Örn: 1/3 yazacak
    public TextMeshProUGUI enemyCardCountText;  // Örn: 2/3 yazacak
    private const int MAX_CARDS_PER_ROUND = 3;  // Sınırımız

    [Header("Prefab")]
    public GameObject cardPrefab;

    private string[] cardNames = { "Kılıçlı Er", "Zırhlı Yaya", "Uzun Yay", "Arbaletçi", "Hafif Süvari", "Ağır Sipahi" };
    private int[] cardPowers = { 3, 5, 4, 6, 5, 8 };

    private int playerTotalScore, enemyTotalScore;
    private int playerWins, enemyWins;
    private bool playerPassed, enemyPassed;
    private bool isPlayerTurn;
    
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
        playerWins = 0; enemyWins = 0;
        
        foreach(var c in playerCrowns) c.SetActive(false);
        foreach(var c in enemyCrowns) c.SetActive(false);

        ClearArea(playerHandArea); ClearArea(enemyHandArea);
        enemyHandCards.Clear();
        
        for (int i = 0; i < 7; i++)
        {
            CreateRandomCard(playerHandArea, true);
            CenkCardUI enemyCard = CreateRandomCard(enemyHandArea, false);
            enemyHandCards.Add(enemyCard);
        }

        StartNewRound();
    }

    void StartNewRound()
    {
        ClearArea(playerBoardArea);
        ClearArea(enemyBoardArea);

        playerPassed = false;
        enemyPassed = false;
        isPlayerTurn = true;
        
        passButton.interactable = true;
        infoText.text = "Yeni Raunt! Sıra Sende.";
        UpdateScoresAndCounters(); // İlk başta 0/3 yazsın
    }

    CenkCardUI CreateRandomCard(Transform parentArea, bool isPlayer)
    {
        int r = Random.Range(0, cardNames.Length);
        GameObject obj = Instantiate(cardPrefab, parentArea);
        CenkCardUI card = obj.GetComponent<CenkCardUI>();
        card.SetupCard(cardNames[r], cardPowers[r], isPlayer);
        return card;
    }

    // --- OYUNCU HAMLESİ ---
    public void PlayerPlaysCard(CenkCardUI playedCard)
    {
        if (!isPlayerTurn || playerPassed) return;

        // Kartı sahaya sür
        playedCard.transform.SetParent(playerBoardArea);
        playedCard.cardButton.interactable = false;

        UpdateScoresAndCounters();
        isPlayerTurn = false;

        // --- YENİ: OTOMATİK PAS KONTROLÜ ---
        if (playerBoardArea.childCount >= MAX_CARDS_PER_ROUND)
        {
            playerPassed = true;
            passButton.interactable = false;
            infoText.text = "Sahadaki limit doldu! Otomatik pas geçildi.";
        }
        else
        {
            infoText.text = "Rakip düşünüyor...";
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    public void OnPassButtonClicked()
    {
        if (!isPlayerTurn) return;
        playerPassed = true;
        passButton.interactable = false;
        infoText.text = "Pas geçtin. Rakip bekleniyor...";
        isPlayerTurn = false;
        StartCoroutine(EnemyTurnRoutine());
    }

    // --- DÜŞMAN (AI) HAMLESİ ---
    IEnumerator EnemyTurnRoutine()
    {
        if (enemyPassed)
        {
            CheckRoundEnd();
            yield break;
        }

        // Düşünme efekti
        if (!playerPassed) infoText.text = "Rakip düşünüyor...";
        yield return new WaitForSeconds(1.2f);

        // AI MANTIĞI KONTROLÜ
        if (enemyHandCards.Count == 0 || enemyBoardArea.childCount >= MAX_CARDS_PER_ROUND || (playerPassed && enemyTotalScore > playerTotalScore))
        {
            enemyPassed = true;
            infoText.text = "Rakip Pas Geçti!";
            yield return new WaitForSeconds(1.5f);
            CheckRoundEnd();
            yield break;
        }

        // AI Kart Oynar
        int r = Random.Range(0, enemyHandCards.Count);
        CenkCardUI cardToPlay = enemyHandCards[r];
        enemyHandCards.RemoveAt(r);

        cardToPlay.transform.SetParent(enemyBoardArea);
        cardToPlay.RevealEnemyCard();

        infoText.text = "Rakip kart oynadı.";
        UpdateScoresAndCounters();
        
        // Oyuncunun yazıyı okuması için 1 saniye bekle
        yield return new WaitForSeconds(1.0f);

        if (!playerPassed)
        {
            isPlayerTurn = true;
            infoText.text = "Sıra Sende.";
        }
        else
        {
            // Oyuncu pas geçtiyse AI kendi kendine oynamaya devam eder
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    // --- SİNERJİ, SKOR VE SAYAÇ (YENİLENDİ) ---
    void UpdateScoresAndCounters()
    {
        // Puanları Hesapla
        playerTotalScore = CalculateRowScore(playerBoardArea);
        enemyTotalScore = CalculateRowScore(enemyBoardArea);

        playerScoreText.text = $"Puan: {playerTotalScore}";
        enemyScoreText.text = $"Puan: {enemyTotalScore}";

        // Sayaçları Güncelle (0/3, 1/3 vb.)
        if (playerCardCountText != null) 
            playerCardCountText.text = $"{playerBoardArea.childCount}/{MAX_CARDS_PER_ROUND}";
        
        if (enemyCardCountText != null) 
            enemyCardCountText.text = $"{enemyBoardArea.childCount}/{MAX_CARDS_PER_ROUND}";
    }

    int CalculateRowScore(Transform row)
    {
        CenkCardUI[] cardsInRow = row.GetComponentsInChildren<CenkCardUI>();
        int rowScore = 0;

        var groupedCards = cardsInRow.GroupBy(c => c.cardName);

        foreach (var group in groupedCards)
        {
            int count = group.Count();
            int multiplier = count > 1 ? 2 : 1; 

            foreach (CenkCardUI card in group)
            {
                card.currentPower = card.basePower * multiplier;
                card.UpdateVisuals(); 
                rowScore += card.currentPower;
            }
        }
        return rowScore;
    }

    // --- RAUNT VE OYUN SONU ---
    void CheckRoundEnd()
    {
        if (playerPassed && enemyPassed)
        {
            StartCoroutine(ResolveRound());
        }
        else if (!playerPassed)
        {
            isPlayerTurn = true;
            infoText.text = "Sıra Sende.";
        }
    }

    IEnumerator ResolveRound()
    {
        infoText.text = "Raunt Bitti!";
        yield return new WaitForSeconds(1.5f);

        if (playerTotalScore > enemyTotalScore)
        {
            playerCrowns[playerWins].SetActive(true);
            playerWins++;
            infoText.text = "Raundu KAZANDIN!";
        }
        else if (enemyTotalScore > playerTotalScore)
        {
            enemyCrowns[enemyWins].SetActive(true);
            enemyWins++;
            infoText.text = "Raundu KAYBETTİN!";
        }
        else
        {
            infoText.text = "Berabere! Kimse taç alamadı.";
        }

        yield return new WaitForSeconds(2.0f);

        if (playerWins >= 2) FinishGame(true);
        else if (enemyWins >= 2) FinishGame(false);
        else StartNewRound(); 
    }

    public void FinishGame(bool playerWon)
    {
        closeButton.SetActive(true);
        passButton.interactable = false;

        if (playerWon)
        {
            infoText.text = "<color=green>CENK'İ KAZANDIN!</color> Orduya moral geldi.";
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(20);
        }
        else
        {
            infoText.text = "<color=red>CENK'İ KAYBETTİN!</color>";
        }
    }

    public void CloseMinigame()
    {
        cenkPanel.SetActive(false);
    }

    void ClearArea(Transform area)
    {
        foreach (Transform child in area) Destroy(child.gameObject);
    }
}