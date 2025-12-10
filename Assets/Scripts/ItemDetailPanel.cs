using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDetailPanel : MonoBehaviour
{
    [Header("Görseller")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
   // public TextMeshProUGUI descriptionText; // Fiyatı buraya yazabiliriz
    public Button equipButton;

    [Header("Statlar")]
    // Yanına parantez içinde farkı yazacağımız textler
    public TextMeshProUGUI strText; 
    public TextMeshProUGUI defText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI staText;

    private ItemData _selectedItem;
    private GladiatorInventory _currentGladiator;

    public void ShowDetails(ItemData item, GladiatorInventory gladiator)
    {
        _selectedItem = item;
        _currentGladiator = gladiator;

        gameObject.SetActive(true);

        // 1. Temel Bilgiler
        itemIcon.sprite = item.icon;
        itemNameText.text = item.itemID;
      //  descriptionText.text = $"Fiyat: {item.price} Akçe"; // Veya "Satış: {item.price / 2}"

        // 2. Karşılaştırma Mantığı
        // Bu eşyanın tipi ne? (Örn: Silah)
        // Gladyatörün üstünde şu an o tipten ne var?
        ItemData currentEquipped = GetCurrentEquippedItem(item.type);

        // Statları Yazdır (CompareStat fonksiyonunu aşağıda yazdık)
        strText.text = "Güç: " + CompareStat(item.bonusStrength, currentEquipped ? currentEquipped.bonusStrength : 0);
        defText.text = "Defans: " + CompareStat(item.bonusDefense, currentEquipped ? currentEquipped.bonusDefense : 0);
        spdText.text = "Hız: " + CompareStat(item.bonusSpeed, currentEquipped ? currentEquipped.bonusSpeed : 0);
        staText.text = "Dayanıklılık: " + CompareStat(item.bonusStamina, currentEquipped ? currentEquipped.bonusStamina : 0);

        // 3. Butonu Ayarla
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(() => 
        {
            InventoryUIManager.Instance.EquipItem(_selectedItem);
            // Kuşandıktan sonra detayları güncelle (Farklar 0'a dönsün)
            ShowDetails(_selectedItem, _currentGladiator);
        });
    }

    // O slotta şu an ne takılı?
    ItemData GetCurrentEquippedItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon: return _currentGladiator.weapon;
            case ItemType.Armor: return _currentGladiator.armor;
            case ItemType.Helmet: return _currentGladiator.helmet;
            case ItemType.Shield: return _currentGladiator.shield;
            default: return null;
        }
    }

    // Matematik ve Renklendirme
    string CompareStat(int newVal, int oldVal)
    {
        int diff = newVal - oldVal;
        
        string baseText = newVal.ToString();

        if (diff > 0)
            return $"{baseText} <color=green>(+{diff})</color>"; // Örn: 10 (+2)
        else if (diff < 0)
            return $"{baseText} <color=red>({diff})</color>";    // Örn: 5 (-3)
        else
            return baseText; // Değişim yoksa sadece sayıyı yaz
    }
}