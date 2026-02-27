using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CenkCardUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI powerText;
    public Button cardButton;

    [HideInInspector] public string cardName;
    [HideInInspector] public int basePower;
    [HideInInspector] public int currentPower;
    [HideInInspector] public bool isPlayerCard;

    // Kartı görsel olarak hazırlar
    public void SetupCard(string name, int power, bool isPlayer)
    {
        cardName = name;
        basePower = power;
        currentPower = basePower;
        isPlayerCard = isPlayer;

        UpdateVisuals();

        // Eğer düşman kartıysa tıklanamaz yap ve arkasını dönük göster
        if (!isPlayer)
        {
            cardButton.interactable = false;
            if (nameText != null) nameText.text = "???";
            if (powerText != null) powerText.text = "?";
        }
        else
        {
            cardButton.interactable = true;
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    public void UpdateVisuals()
    {
        if (nameText != null) nameText.text = cardName;
        // Eğer sinerji ile gücü artmışsa yeşil yazsın
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
        UpdateVisuals();
    }
}