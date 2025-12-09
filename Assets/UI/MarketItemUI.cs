using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text; // StringBuilder için gerekli (Metin birleştirme)

public class MarketItemUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Image iconImage;
    public Button buyButton;

    // YENİ: Statları yazacağımız text alanı
    public TextMeshProUGUI statsText; 

    private ItemData _myItemData;

    public void Setup(ItemData item)
    {
        _myItemData = item;

        // 1. Temel Bilgiler
        nameText.text = item.itemID;
        priceText.text = item.price.ToString() + " Akçe";
        
        if (item.icon != null) iconImage.sprite = item.icon;

        // 2. STATLARI HESAPLA (Sadece 0'dan büyük olanları göster)
        StringBuilder statsBuilder = new StringBuilder();

        if (item.bonusStrength >= 0)
        {
             statsBuilder.AppendLine($"Güç: <color=green>+{item.bonusStrength}</color>");
        }
        else if (item.bonusStrength<0){
             statsBuilder.AppendLine($"Güç: <color=red>{item.bonusStrength}</color>");
        }

        if (item.bonusDefense >= 0)  {statsBuilder.AppendLine($"Def: <color=green>+{item.bonusDefense}</color>");
        }
        else if (item.bonusDefense<0){
             statsBuilder.AppendLine($"Güç: <color=red>{item.bonusDefense}</color>");
        }
        if (item.bonusSpeed >= 0)   { statsBuilder.AppendLine($"Hız: <color=green>+{item.bonusSpeed}</color>");}
        else if (item.bonusSpeed<0){
             statsBuilder.AppendLine($"Güç: <color=red>{item.bonusSpeed}</color>");
        }

        if (item.bonusStamina >= 0)  {statsBuilder.AppendLine($"Day: <color=green>+{item.bonusStamina}</color>");}
        else if (item.bonusStamina<0){
             statsBuilder.AppendLine($"Güç: <color=red>{item.bonusStamina}</color>");
        }
        // if (item.bonusMorale != 0)   statsBuilder.AppendLine($"Moral: <color=green>+{item.bonusMorale}</color>");

        // Eğer hiçbir stat yoksa (mesela sadece süs eşyasıysa)
        if (statsBuilder.Length == 0) statsBuilder.Append("Özellik Yok");

        statsText.text = statsBuilder.ToString();

        // 3. Buton
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    void OnBuyClicked()
    {
        MarketManager.Instance.Buy(_myItemData);
    }
}