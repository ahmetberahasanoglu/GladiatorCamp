using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CenkCardUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI powerText;
    public Button cardButton;

    [HideInInspector] public int cardPower;
    [HideInInspector] public string cardName;

    // Kartı görsel olarak hazırlar
    public void SetupCard(string name, int power, bool isPlayer)
    {
        cardName = name;
        cardPower = power;

        if (nameText != null) nameText.text = cardName;
        if (powerText != null) powerText.text = cardPower.ToString();

        // Eğer düşman kartıysa tıklanamaz yap ve arkasını dönük göster (isim yerine ? yazabilirsin)
        if (!isPlayer)
        {
            cardButton.interactable = false;
            if (nameText != null) nameText.text = "???";
            if (powerText != null) powerText.text = "?";
        }
        else
        {
            cardButton.interactable = true;
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    // Oyuncu kendi kartına tıkladığında Manager'a haber verir
    void OnCardClicked()
    {
        CenkGameManager.Instance.PlayerPlaysCard(this);
    }

    // Düşman kartı oynandığında gerçek değerini göster
    public void RevealEnemyCard()
    {
        if (nameText != null) nameText.text = cardName;
        if (powerText != null) powerText.text = cardPower.ToString();
    }
}