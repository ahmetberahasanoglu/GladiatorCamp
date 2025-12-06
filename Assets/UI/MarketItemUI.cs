using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kulland���n� varsay�yorum, standart Text ise 'Text' yap.

public class MarketItemUI : MonoBehaviour
{
    [Header("UI Bile�enleri")]
    public TextMeshProUGUI nameText; 
    public TextMeshProUGUI priceText;
    public Image iconImage;          
    public Button buyButton;         

    private ItemData _myItemData;    

    // Bu fonksiyonu MarketManager �a��racak
    public void Setup(ItemData item)
    {
        _myItemData = item;

        // G�rselleri g�ncelle
        nameText.text = item.itemID;
        priceText.text = item.price.ToString() + " Altin";

        // E�er ItemData'ya ikon eklediysen:
        // iconImage.sprite = item.icon; 

        // Butonu temizle ve yeni g�revi ata
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    void OnBuyClicked()
    {
        // Patron'a (MarketManager) haber ver
        MarketManager.Instance.Buy(_myItemData);
    }
}