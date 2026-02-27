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
    public TextMeshProUGUI diceText;    
    
    [Header("Yeni: Devam Butonu")]
    public GameObject continueButton;   // Zar durunca çıkacak buton

    [Header("Animasyon (Juiciness)")]
    public float rollDuration = 1.5f;   
    public float shakeIntensity = 20f;  

    public Sprite[] diceSprites;        

    // Hafızada tutulacaklar
    private Action<int> currentCallback;
    private int finalResult;

    void Awake()
    {
        Instance = this;
        if (dicePanel != null) dicePanel.SetActive(false);
    }

    public void RollDice(Action<int> onRollComplete)
    {
        currentCallback = onRollComplete;
        
        dicePanel.SetActive(true);
        if (continueButton != null) continueButton.SetActive(false); // Başta butonu gizle
        
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

        // 3. VURGU (POP EFEKTİ)
        diceTransform.localScale = originalScale * 1.5f;
        yield return new WaitForSeconds(0.15f);
        diceTransform.localScale = originalScale;

        // 4. OYUNCUYA KONTROLÜ VER (Butonu Aç)
        if (continueButton != null)
        {
            continueButton.SetActive(true);
        }
        else
        {
            // Eğer buton eklemeyi unutursan oyun çökmesin diye eski usul devam etsin
            yield return new WaitForSeconds(1.5f);
            FinishRoll();
        }
    }

    // Butona tıklandığında çalışacak fonksiyon
    public void FinishRoll()
    {
        dicePanel.SetActive(false);
        if (continueButton != null) continueButton.SetActive(false);
        
        // Zar sonucunu harita olayına (MapEventManager'a) gönder
        currentCallback?.Invoke(finalResult);
    }

    void UpdateDiceVisual(int number)
    {
        if (diceSprites != null && diceSprites.Length >= 6)
        {
            diceImage.sprite = diceSprites[number - 1];
        }
        
        if (diceText != null)
        {
            diceText.text = number.ToString();
        }
    }
}