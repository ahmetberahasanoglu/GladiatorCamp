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

    [Header("Pes Etme Ayarları")]
    public TextMeshProUGUI surrenderButtonText; 
    private bool isFirstTurn = true; 

    [Header("Kart Sayacı")]
    public TextMeshProUGUI playerCardCountText; 
    public TextMeshProUGUI enemyCardCountText;  
    private const int MAX_CARDS_PER_ROUND = 3;  

    [Header("Prefab ve Görseller")]
    public GameObject cardPrefab;
    public Sprite[] cardSprites; 

    // KART VERİTABANI
    private string[] cardNames = { "Kılıçlı Er", "Zırhlı Yaya", "Uzun Yay", "Arbaletçi", "Hafif Süvari", "Ağır Sipahi" };
    private int[] cardPowers = { 3, 5, 4, 6, 5, 8 };

    private int playerTotalScore, enemyTotalScore;
    private int playerWins, enemyWins;
    private bool playerPassed, enemyPassed;
    private bool isPlayerTurn;

    [Header("Günlük Sınır")]
    public bool hasPlayedToday = false; 
    
    private List<CenkCardUI> enemyHandCards = new List<CenkCardUI>();

    void Awake()
    {
        Instance = this;
        if (cenkPanel != null) cenkPanel.SetActive(false);
    }

    void Start()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay += ResetDailyPlayLimit;
        }
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay -= ResetDailyPlayLimit;
        }
    }

    void ResetDailyPlayLimit()
    {
        hasPlayedToday = false; 
    }

    public void OpenMinigame()
    {
        cenkPanel.SetActive(true);
        closeButton.SetActive(false);
        hasPlayedToday = true;
        playerWins = 0; enemyWins = 0;
        
        foreach(var c in playerCrowns) c.SetActive(false);
        foreach(var c in enemyCrowns) c.SetActive(false);

        ClearArea(playerHandArea); 
        ClearArea(enemyHandArea);
        enemyHandCards.Clear();
        
        for (int i = 0; i < 7; i++)
        {
            CreateRandomCard(playerHandArea, true);
            CenkCardUI enemyCard = CreateRandomCard(enemyHandArea, false);
            enemyHandCards.Add(enemyCard);
        }

        // BUG 2 ÇÖZÜMÜ: isFirstTurn sadece oyun İLK açıldığında true olmalıdır!
        isFirstTurn = true; 
        if (surrenderButtonText != null) surrenderButtonText.text = "Çık";

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
        UpdateScoresAndCounters(); 
    }

    CenkCardUI CreateRandomCard(Transform parentArea, bool isPlayer)
    {
        int r = Random.Range(0, cardNames.Length);
        GameObject obj = Instantiate(cardPrefab, parentArea);
        CenkCardUI card = obj.GetComponent<CenkCardUI>();
        
        Sprite selectedSprite = null;
        if (cardSprites != null && cardSprites.Length > r)
        {
            selectedSprite = cardSprites[r];
        }

        card.SetupCard(cardNames[r], cardPowers[r], selectedSprite, isPlayer);
        return card;
    }

    // --- OYUNCU HAMLESİ ---
    public void PlayerPlaysCard(CenkCardUI playedCard)
    {
        if (!isPlayerTurn || playerPassed) return;
        
        if (isFirstTurn)
        {
            isFirstTurn = false;
            if (surrenderButtonText != null) surrenderButtonText.text = "Çekil (-5)";
        }
        
        playedCard.transform.SetParent(playerBoardArea);
        playedCard.cardButton.interactable = false;

        UpdateScoresAndCounters();
        isPlayerTurn = false;

        if (playerBoardArea.childCount >= MAX_CARDS_PER_ROUND)
        {
            playerPassed = true;
            passButton.interactable = false;
            infoText.text = "Tur bitti!";
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

        // BUG 3 ÇÖZÜMÜ: Oyuncu pas dediğinde de maça katılmış demektir, ceza başlar.
        if (isFirstTurn)
        {
            isFirstTurn = false;
            if (surrenderButtonText != null) surrenderButtonText.text = "Çekil (-5)";
        }

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

        if (!playerPassed) infoText.text = "Rakip düşünüyor...";
        yield return new WaitForSeconds(1.2f);

        if (enemyHandCards.Count == 0 || enemyBoardArea.childCount >= MAX_CARDS_PER_ROUND || (playerPassed && enemyTotalScore > playerTotalScore))
        {
            enemyPassed = true;
            infoText.text = "Rakip Pas Geçti!";
            yield return new WaitForSeconds(1.5f);
            CheckRoundEnd();
            yield break;
        }

        int r = Random.Range(0, enemyHandCards.Count);
        CenkCardUI cardToPlay = enemyHandCards[r];
        enemyHandCards.RemoveAt(r);

        cardToPlay.transform.SetParent(enemyBoardArea);
        cardToPlay.RevealEnemyCard();

        infoText.text = "Rakip kart oynadı.";
        UpdateScoresAndCounters();
        
        yield return new WaitForSeconds(1.0f);

        if (!playerPassed)
        {
            isPlayerTurn = true;
            infoText.text = "Sıra Sende.";
        }
        else
        {
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    // --- SİNERJİ, SKOR VE SAYAÇ ---
    void UpdateScoresAndCounters()
    {
        playerTotalScore = CalculateRowScore(playerBoardArea);
        enemyTotalScore = CalculateRowScore(enemyBoardArea);

        playerScoreText.text = $"Puan: {playerTotalScore}";
        enemyScoreText.text = $"Puan: {enemyTotalScore}";

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
            int multiplier = count; 

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

    public void SurrenderGame()
    {
        if (closeButton.activeSelf) return;

        StopAllCoroutines(); 
        
        if (passButton != null) passButton.interactable = false;
        closeButton.SetActive(true);

        if (isFirstTurn)
        {
            infoText.text = "Masadan kalktın.";
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Savaşa başlamadan masadan ayrıldın.", NotificationType.Info);
        }
        else
        {
            infoText.text = "<color=red>PES ETTİN!</color> Masadan çekildin.";
            if (CampMoraleManager.Instance != null) 
                CampMoraleManager.Instance.ChangeMorale(-5); 

            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Savaştan çekildin. Askerlerin morali bozuldu (-5)", NotificationType.Warning);
        }
    }

    public void CloseMinigame()
    {
        cenkPanel.SetActive(false);
    }

    // BUG 1 ÇÖZÜMÜ: YENİ CLEAR AREA
    void ClearArea(Transform area)
    {
        // Tahtadaki kartları yok etmeden önce ebeveynlikten çıkarıyoruz. 
        // Böylece UpdateScoresAndCounters çağrıldığında bu kartları "0" olarak görür.
        while (area.childCount > 0)
        {
            Transform child = area.GetChild(0);
            child.SetParent(null); 
            Destroy(child.gameObject);
        }
    }
}