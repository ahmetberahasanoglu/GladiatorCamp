using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CenkCardUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI powerText;
    public Button cardButton;
    
    [Header("Görseller")]
    public Image cardArtImage;       // Kartın ana resmi (Kılıçlı Er vb.)
    public Sprite cardBackSprite;    // Opsiyonel: Düşman kartlarının arka yüzü (Kapalı hali)

    [HideInInspector] public string cardName;
    [HideInInspector] public int basePower;
    [HideInInspector] public int currentPower;
    [HideInInspector] public bool isPlayerCard;
    [HideInInspector] public Sprite cardFrontSprite; // Gerçek resmi hafızada tut

    // Kartı görsel olarak hazırlar (YENİ: artSprite eklendi)
    public void SetupCard(string name, int power, Sprite artSprite, bool isPlayer)
    {
        cardName = name;
        basePower = power;
        currentPower = basePower;
        cardFrontSprite = artSprite;
        isPlayerCard = isPlayer;

        UpdateVisuals();

        if (!isPlayer)
        {
            // --- DÜŞMAN KARTI (GİZLİ) ---
            cardButton.interactable = false;
            if (nameText != null) nameText.text = "???";
            if (powerText != null) powerText.text = "?";
            
            if (cardArtImage != null)
            {
                if (cardBackSprite != null) 
                    cardArtImage.sprite = cardBackSprite; // Arka yüzü göster
                else 
                    cardArtImage.color = Color.black; // Arka yüz yoksa resmi tamamen karart
            }
        }
        else
        {
            // --- OYUNCU KARTI (AÇIK) ---
            cardButton.interactable = true;
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnCardClicked);
            
            if (cardArtImage != null && cardFrontSprite != null)
            {
                cardArtImage.sprite = cardFrontSprite;
                cardArtImage.color = Color.white; // Rengi normale çevir
            }
        }
    }

    public void UpdateVisuals()
    {
        if (nameText != null) nameText.text = cardName;
        if (powerText != null) 
        {
            powerText.text = currentPower.ToString();
            powerText.color = currentPower > basePower ? Color.green : Color.white;
        }
    }

    void OnCardClicked()
    {
        CenkGameManager.Instance.PlayerPlaysCard(this);
    }

    public void RevealEnemyCard()
    {
        // Düşman kartı masaya atıldığında gerçek yüzünü gösterir
        UpdateVisuals();
        
        if (cardArtImage != null && cardFrontSprite != null)
        {
            cardArtImage.sprite = cardFrontSprite;
            cardArtImage.color = Color.white; // Karartmayı kaldır
        }
    }
}