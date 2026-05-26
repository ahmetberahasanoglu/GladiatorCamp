using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class MarketItemUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Image iconImage;
    public Button buyButton;
    public TextMeshProUGUI statsText;

    [Header("Set Badge")]
    [Tooltip("'Ateş', 'Zehir', 'İnanç' gibi etiketin arkasındaki Image (RectTransform)")]
    public Image setBadgeBg;
    [Tooltip("Set ismini gösteren TMP text")]
    public TextMeshProUGUI setBadgeText;

    private ItemData _myItemData;

    public void Setup(ItemData item)
    {
        _myItemData = item;

        // --- 1. Görünen İsim (itemName) ---
        nameText.text = item.DisplayName;

        // --- 2. Fiyat ---
        priceText.text = item.price.ToString() + " Akçe";

        // --- 3. İkon ---
        if (item.icon != null) iconImage.sprite = item.icon;

        // --- 4. Statlar ---
        StringBuilder sb = new StringBuilder();
        if (item.bonusStrength != 0) sb.Append($"STR: {Colorize(item.bonusStrength)}  ");
        if (item.bonusDefense != 0)  sb.Append($"DEF: {Colorize(item.bonusDefense)}  ");
        if (item.bonusSpeed != 0)    sb.Append($"SPD: {Colorize(item.bonusSpeed)}  ");
        if (item.bonusStamina != 0)  sb.Append($"STA: {Colorize(item.bonusStamina)}  ");
        if (sb.Length == 0) sb.Append("Özellik Yok");
        statsText.text = sb.ToString();

        // --- 5. Set Badge ---
        RefreshSetBadge(item);

        // --- 6. Buton ---
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    void RefreshSetBadge(ItemData item)
    {
        bool hasSet = item.setType != ItemSetType.None;

        if (setBadgeBg != null)  setBadgeBg.gameObject.SetActive(hasSet);
        if (setBadgeText != null) setBadgeText.gameObject.SetActive(hasSet);

        if (!hasSet) return;

        Color setColor = item.GetSetColor();

        if (setBadgeBg != null)
            setBadgeBg.color = setColor;

        if (setBadgeText != null)
        {
            setBadgeText.text = item.GetSetDisplayName() + " Seti";
            setBadgeText.color = Color.white;
        }
    }

    string Colorize(int val)
    {
        if (val > 0) return $"<color=green>+{val}</color>";
        if (val < 0) return $"<color=red>{val}</color>";
        return val.ToString();
    }

    void OnBuyClicked()
    {
        MarketManager.Instance.Buy(_myItemData);
    }
}
