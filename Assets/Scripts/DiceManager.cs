using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;

    [Header("UI Ayarları")]
    public GameObject dicePanel;        // Tüm ekranı hafif karartan veya ortada çıkan panel
    public RectTransform diceTransform; // Zarın kendisi (Döndürmek ve büyütmek için)
    public Image diceImage;             // Eğer zar resimlerin varsa
    public TextMeshProUGUI diceText;    // Resim yoksa zarın üstünde yazan sayı
    
    [Header("Animasyon (Juiciness)")]
    public float rollDuration = 1.5f;   // Zar kaç saniye dönecek
    public float shakeIntensity = 20f;  // Ne kadar titreyecek

    public Sprite[] diceSprites;        // İsteğe bağlı: 6 adet zar resmi (1'den 6'ya)

    void Awake()
    {
        Instance = this;
        if (dicePanel != null) dicePanel.SetActive(false);
    }

    // Başka scriptlerden çağrılacak ana fonksiyon. 
    // "Action<int> onRollComplete" sayesinde zar durduğunda ne olacağını ona söyleyeceğiz.
    public void RollDice(Action<int> onRollComplete)
    {
        dicePanel.SetActive(true);
        StartCoroutine(RollRoutine(onRollComplete));
    }

    IEnumerator RollRoutine(Action<int> onRollComplete)
    {
        float timer = 0f;
        int currentFace = 1;

        Vector3 originalScale = diceTransform.localScale;
        
        // 1. ZAR DÖNME / TİTREME EFEKTİ (JUICE)
        while (timer < rollDuration)
        {
            currentFace = UnityEngine.Random.Range(1, 7); // 1 ile 6 arası
            UpdateDiceVisual(currentFace);

            // Rastgele sağa sola rotasyon ver (Zar atılıyormuş hissi)
            float randomZ = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            diceTransform.rotation = Quaternion.Euler(0, 0, randomZ);

            timer += 0.1f; // Her 0.1 saniyede bir şekil değiştir
            yield return new WaitForSeconds(0.1f);
        }

        // 2. KESİN SONUCU BELİRLE
        int finalResult = UnityEngine.Random.Range(1, 7);
        UpdateDiceVisual(finalResult);
        diceTransform.rotation = Quaternion.identity; // Düzelt

        // 3. VURGU (POP EFEKTİ) - Zar durduğunda bir anlığına büyüsün
        diceTransform.localScale = originalScale * 1.5f;
        yield return new WaitForSeconds(0.1f);
        diceTransform.localScale = originalScale;

        // Oyuncu sonucu idrak etsin diye 1 saniye bekle
        yield return new WaitForSeconds(1.0f);

        // 4. BİTİR VE SONUCU GÖNDER
        dicePanel.SetActive(false);
        onRollComplete?.Invoke(finalResult);
    }

    void UpdateDiceVisual(int number)
    {
        // Eğer elinde zar spriteları varsa onları kullan
        if (diceSprites != null && diceSprites.Length >= 6)
        {
            diceImage.sprite = diceSprites[number - 1];
        }
        
        // Her halükarda texti de güncelle (Sprite yoksa bile çalışır)
        if (diceText != null)
        {
            diceText.text = number.ToString();
        }
    }
}