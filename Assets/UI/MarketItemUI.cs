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

    [Header("Set Badge / Element Görselleri")]
    [Tooltip("Eski arkaplan rengi (Artık gizleyebilir veya hafif saydam yapabilirsin)")]
    public Image setBadgeBg; 
    public TextMeshProUGUI setBadgeText;
    
    [Tooltip("YENİ: Element ikonunu gösterecek Image bileşeni")]
    public Image elementIconImage; 

    [Header("Element İkon Havuzu (Inspector'dan Ata)")]
    public Sprite fireIcon;
    public Sprite poisonIcon;
    public Sprite faithIcon;
    public Sprite normalIcon; // Eğer 'None' durumunda fiziksel kılıç/kalkan ikonunu göstermek istersen

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

        // --- 5. Set Badge ve İkon ---
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
        
        // Element ikonu her zaman aktif olabilir (Normal fiziksel itemleri göstermek için) 
        // veya sadece set varsa aktif edebilirsin. Şimdilik hep aktif bırakalım.
        if (elementIconImage != null) elementIconImage.gameObject.SetActive(true);

        Color setColor = item.GetSetColor();

        // Arka planı tamamen kaldırabilir veya estetik için çok saydam (alpha 0.2) bir renk yapabilirsin
        if (setBadgeBg != null && hasSet)
        {
            setBadgeBg.color = new Color(setColor.r, setColor.g, setColor.b, 0.2f); 
        }

        if (setBadgeText != null && hasSet)
        {
            setBadgeText.text = item.GetSetDisplayName() + " Seti";
            setBadgeText.color = setColor; // Yazıyı da set rengine boyayalım
        }

        // --- YENİ: İkon Ataması ve Renklendirme ---
        if (elementIconImage != null)
        {
            elementIconImage.sprite = GetIconForSet(item.setType);
            
            // Eğer seti varsa ikon o elementin renginde (Kırmızı, Yeşil vb.) parlasın.
            // Seti yoksa (None), standart siyah/koyu kahve kalsın.
            elementIconImage.color = hasSet ? setColor : new Color(0.2f, 0.2f, 0.2f, 1f); 
        }
    }

    // Set tipine göre ilgili ikonu döndürür
    Sprite GetIconForSet(ItemSetType type)
    {
        return type switch
        {
            ItemSetType.Fire => fireIcon,
            ItemSetType.Poison => poisonIcon,
            ItemSetType.Faith => faithIcon,
            _ => normalIcon // Hiçbir set yoksa çapraz kılıç/kalkan ikonunu döndür
        };
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