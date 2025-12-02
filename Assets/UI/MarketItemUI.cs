using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullandýðýný varsayýyorum, standart Text ise 'Text' yap.

public class MarketItemUI : MonoBehaviour
{
    [Header("UI Bileþenleri")]
    public TextMeshProUGUI nameText;  // Eþya Ýsmi
    public TextMeshProUGUI priceText; // Fiyat
    public Image iconImage;           // Eþyanýn resmi (varsa)
    public Button buyButton;          // Satýn al butonu

    private ItemData _myItemData;     // Bu kutucuk hangi eþyayý tutuyor?

    // Bu fonksiyonu MarketManager çaðýracak
    public void Setup(ItemData item)
    {
        _myItemData = item;

        // Görselleri güncelle
        nameText.text = item.itemName;
        priceText.text = item.price.ToString() + " Altýn";

        // Eðer ItemData'ya ikon eklediysen:
        // iconImage.sprite = item.icon; 

        // Butonu temizle ve yeni görevi ata
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    void OnBuyClicked()
    {
        // Patron'a (MarketManager) haber ver
        MarketManager.Instance.Buy(_myItemData);
    }
}