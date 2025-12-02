using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketItemUI : MonoBehaviour
{
    public ItemData item;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    void Start()
    {
        nameText.text = item.itemName;
        priceText.text = item.price + " gold";

        buyButton.onClick.AddListener(() => MarketManager.Instance.Buy(item));
    }
}
