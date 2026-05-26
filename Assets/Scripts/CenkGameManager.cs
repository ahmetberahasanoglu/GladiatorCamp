using UnityEngine;
using TMPro;
using System;
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

    // Kart veritabanı
    private string[] cardNames  = { "Kılıçlı Er", "Zırhlı Yaya", "Uzun Yay", "Arbaletçi", "Hafif Süvari", "Ağır Sipahi" };
    private int[]    cardPowers = { 3, 5, 4, 6, 5, 8 };

    private int  playerTotalScore, enemyTotalScore;
    private int  playerWins, enemyWins;
    private bool playerPassed, enemyPassed;
    private bool isPlayerTurn;

    [Header("Günlük Sınır")]
    public bool hasPlayedToday = false;

    private List<CenkCardUI> enemyHandCards = new List<CenkCardUI>();

    // ── HADİSE CALLBACK — MapEventManager buraya bağlanır ───────────────────
    /// <summary>
    /// Harita etkinliği olarak açıldığında MapEventManager bu callback'i doldurur.
    /// Normal kamp oyununda null kalır.
    /// </summary>
    [HideInInspector]
    public Action<bool> OnCenkFinished;
    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
        if (cenkPanel != null) cenkPanel.SetActive(false);
    }

    void Start()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay += ResetDailyPlayLimit;
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay -= ResetDailyPlayLimit;
    }

    void ResetDailyPlayLimit() => hasPlayedToday = false;

    public void OpenMinigame()
    {
        cenkPanel.SetActive(true);
        closeButton.SetActive(false);
        hasPlayedToday = true;
        playerWins = 0; enemyWins = 0;

        foreach (var c in playerCrowns) c.SetActive(false);
        foreach (var c in enemyCrowns)  c.SetActive(false);

        ClearArea(playerHandArea);
        ClearArea(enemyHandArea);
        enemyHandCards.Clear();

        for (int i = 0; i < 7; i++)
        {
            CreateRandomCard(playerHandArea, true);
            CenkCardUI enemyCard = CreateRandomCard(enemyHandArea, false);
            enemyHandCards.Add(enemyCard);
        }

        isFirstTurn = true;
        if (surrenderButtonText != null) surrenderButtonText.text = "Çık";

        StartNewRound();
    }

    void StartNewRound()
    {
        ClearArea(playerBoardArea);
        ClearArea(enemyBoardArea);

        playerPassed = false;
        enemyPassed  = false;
        isPlayerTurn = true;

        passButton.interactable = true;
        infoText.text = "Yeni Raunt! Sıra Sende.";
        UpdateScoresAndCounters();
    }

    CenkCardUI CreateRandomCard(Transform parentArea, bool isPlayer)
    {
        int r = UnityEngine.Random.Range(0, cardNames.Length);
        GameObject obj = Instantiate(cardPrefab, parentArea);
        CenkCardUI card = obj.GetComponent<CenkCardUI>();
        Sprite selectedSprite = (cardSprites != null && cardSprites.Length > r) ? cardSprites[r] : null;
        card.SetupCard(cardNames[r], cardPowers[r], selectedSprite, isPlayer);
        return card;
    }

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
            playerPassed            = true;
            passButton.interactable = false;
            infoText.text           = "Tur bitti!";
        }
        else infoText.text = "Rakip düşünüyor...";

        StartCoroutine(EnemyTurnRoutine());
    }

    public void OnPassButtonClicked()
    {
        if (!isPlayerTurn) return;

        if (isFirstTurn)
        {
            isFirstTurn = false;
            if (surrenderButtonText != null) surrenderButtonText.text = "Çekil (-5)";
        }

        playerPassed            = true;
        passButton.interactable = false;
        infoText.text           = "Pas geçtin. Rakip bekleniyor...";
        isPlayerTurn            = false;
        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        if (enemyPassed) { CheckRoundEnd(); yield break; }
        if (!playerPassed) infoText.text = "Rakip düşünüyor...";
        yield return new WaitForSeconds(1.2f);

        if (enemyHandCards.Count == 0 || enemyBoardArea.childCount >= MAX_CARDS_PER_ROUND
            || (playerPassed && enemyTotalScore > playerTotalScore))
        {
            enemyPassed   = true;
            infoText.text = "Rakip Pas Geçti!";
            yield return new WaitForSeconds(1.5f);
            CheckRoundEnd(); yield break;
        }

        int r = UnityEngine.Random.Range(0, enemyHandCards.Count);
        CenkCardUI cardToPlay = enemyHandCards[r];
        enemyHandCards.RemoveAt(r);
        cardToPlay.transform.SetParent(enemyBoardArea);
        cardToPlay.RevealEnemyCard();
        infoText.text = "Rakip kart oynadı.";
        UpdateScoresAndCounters();
        yield return new WaitForSeconds(1.0f);

        if (!playerPassed) { isPlayerTurn = true; infoText.text = "Sıra Sende."; }
        else StartCoroutine(EnemyTurnRoutine());
    }

    void UpdateScoresAndCounters()
    {
        playerTotalScore = CalculateRowScore(playerBoardArea);
        enemyTotalScore  = CalculateRowScore(enemyBoardArea);
        playerScoreText.text = $"Puan: {playerTotalScore}";
        enemyScoreText.text  = $"Puan: {enemyTotalScore}";
        if (playerCardCountText != null) playerCardCountText.text = $"{playerBoardArea.childCount}/{MAX_CARDS_PER_ROUND}";
        if (enemyCardCountText  != null) enemyCardCountText.text  = $"{enemyBoardArea.childCount}/{MAX_CARDS_PER_ROUND}";
    }

    int CalculateRowScore(Transform row)
    {
        CenkCardUI[] cardsInRow = row.GetComponentsInChildren<CenkCardUI>();
        int rowScore = 0;
        var grouped  = cardsInRow.GroupBy(c => c.cardName);
        foreach (var group in grouped)
        {
            int count = group.Count();
            foreach (CenkCardUI card in group)
            {
                card.currentPower = card.basePower * count;
                card.UpdateVisuals();
                rowScore += card.currentPower;
            }
        }
        return rowScore;
    }

    void CheckRoundEnd()
    {
        if (playerPassed && enemyPassed) StartCoroutine(ResolveRound());
        else if (!playerPassed) { isPlayerTurn = true; infoText.text = "Sıra Sende."; }
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
        else infoText.text = "Berabere!";

        yield return new WaitForSeconds(2.0f);

        if      (playerWins >= 2) FinishGame(true);
        else if (enemyWins  >= 2) FinishGame(false);
        else StartNewRound();
    }

    public void FinishGame(bool playerWon)
    {
        closeButton.SetActive(true);
        passButton.interactable = false;

        if (playerWon)
        {
            infoText.text = "<color=green>CENK'İ KAZANDIN!</color> Orduya moral geldi.";
            // Normal kamp oyunu ödülü (harita etkinliğinde callback üstlenir)
            if (OnCenkFinished == null && CampMoraleManager.Instance != null)
                CampMoraleManager.Instance.ChangeMorale(20);
        }
        else
        {
            infoText.text = "<color=red>CENK'İ KAYBETTİN!</color>";
        }

        // ── HADİSE CALLBACK ─────────────────────────────────────────────────
        // Kapat butonuna bastığında MapEventManager'a sonucu bildir
        var cachedCallback = OnCenkFinished;
        if (cachedCallback != null)
        {
            bool result = playerWon;
            // Kapat butonunu tıklamadan önce callback'i tetikle
            Button closeBtn = closeButton.GetComponent<Button>();
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(() =>
                {
                    CloseMinigame();
                    cachedCallback.Invoke(result);
                });
            }
        }
        // ────────────────────────────────────────────────────────────────────
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
            NotificationManager.Instance?.Show("Savaşa başlamadan masadan ayrıldın.", NotificationType.Info);
        }
        else
        {
            infoText.text = "<color=red>PES ETTİN!</color>";
            CampMoraleManager.Instance?.ChangeMorale(-5);
            NotificationManager.Instance?.Show("Savaştan çekildin. (-5 Moral)", NotificationType.Warning);
        }

        // Harita etkinliğiyse yenilgi olarak bildir
        var cachedCallback = OnCenkFinished;
        if (cachedCallback != null)
        {
            Button closeBtn = closeButton.GetComponent<Button>();
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(() => { CloseMinigame(); cachedCallback.Invoke(false); });
            }
        }
    }

    public void CloseMinigame() => cenkPanel.SetActive(false);

    void ClearArea(Transform area)
    {
        while (area.childCount > 0)
        {
            Transform child = area.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }
}
