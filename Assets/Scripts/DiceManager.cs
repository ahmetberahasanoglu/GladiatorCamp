using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;

    [Header("UI Ayarları")]
    public GameObject dicePanel;        
    public RectTransform diceTransform; 
    public Image diceImage;             
    //public TextMeshProUGUI diceText;    
    
    [Header("Yeni: UI Elemanları")]
    public GameObject continueButton;   
    public TextMeshProUGUI statusText; // "BAŞARILI / BAŞARISIZ" yazısı
    public TextMeshProUGUI targetText; // Opsiyonel: Zardan önce "Hedef: 4" yazısı

    [Header("Animasyon (Juiciness)")]
    public float rollDuration = 1.5f;   
    public float shakeIntensity = 20f;  

    public Sprite[] diceSprites;        

    private Action<int> currentCallback;
    private int finalResult;
    private int currentTarget;

    void Awake()
    {
        Instance = this;
        if (dicePanel != null) dicePanel.SetActive(false);
    }

    // Hedefsiz düz zar atmak istersek
    public void RollDice(Action<int> onRollComplete)
    {
        StartRoll(0, onRollComplete);
    }

    // BG3 Tarzı: Hedefli zar atmak istersek (Örn: Hedef 4)
    public void RollDice(int targetNumber, Action<int> onRollComplete)
    {
        StartRoll(targetNumber, onRollComplete);
    }

    private void StartRoll(int targetNumber, Action<int> onRollComplete)
    {
        currentTarget = targetNumber;
        currentCallback = onRollComplete;
        
        dicePanel.SetActive(true);
        
        // Başlangıçta butonları ve yazıları gizle
        if (continueButton != null) continueButton.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
        
        if (targetText != null) 
        {
            if (currentTarget > 0)
            {
                targetText.gameObject.SetActive(true);
                targetText.text = $"Minimum {currentTarget}";
            }
            else
            {
                targetText.gameObject.SetActive(false);
            }
        }
        
        StartCoroutine(RollRoutine());
    }

    IEnumerator RollRoutine()
    {
        float timer = 0f;
        int currentFace = 1;
        Vector3 originalScale = diceTransform.localScale;
        
        // 1. ZAR DÖNME / TİTREME EFEKTİ
        while (timer < rollDuration)
        {
            currentFace = UnityEngine.Random.Range(1, 7); 
            UpdateDiceVisual(currentFace);

            float randomZ = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            diceTransform.rotation = Quaternion.Euler(0, 0, randomZ);

            timer += 0.1f; 
            yield return new WaitForSeconds(0.1f);
        }

        // 2. KESİN SONUCU BELİRLE
        finalResult = UnityEngine.Random.Range(1, 7);
        UpdateDiceVisual(finalResult);
        diceTransform.rotation = Quaternion.identity; 

        // 3. VURGU (Zar durunca büyür)
        diceTransform.localScale = originalScale * 1.5f;
        yield return new WaitForSeconds(0.15f);
        diceTransform.localScale = originalScale;

        // --- 4. YENİ: BAŞARI KONTROLÜ VE YAZISI ---
        if (currentTarget > 0 && statusText != null)
        {
            statusText.gameObject.SetActive(true);
            if (finalResult >= currentTarget)
            {
                statusText.text = "<color=green>BAŞARILI</color>";
                // İleride buraya tatlı bir 'Çın!' sesi ekleyebilirsin
            }
            else
            {
                statusText.text = "<color=red>BAŞARISIZ</color>";
                // İleride buraya boğuk bir 'Güm' sesi ekleyebilirsin
            }
        }

        // 5. DEVAM BUTONUNU AÇ
        if (continueButton != null)
        {
            continueButton.SetActive(true);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
            FinishRoll();
        }
    }

    public void FinishRoll()
    {
        dicePanel.SetActive(false);
        if (continueButton != null) continueButton.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
        
        currentCallback?.Invoke(finalResult);
    }

    void UpdateDiceVisual(int number)
    {
        if (diceSprites != null && diceSprites.Length >= 6)
        {
            diceImage.sprite = diceSprites[number - 1];
        }
       
    }
}