using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public TextMeshProUGUI nameText;
    public Button equipButton;
    public Image iconImage; // YENİ EKLENEN: İkon resmi için referans
    public TextMeshProUGUI statsText;

    // Manager'ın çağırdığı Setup fonksiyonu
    public void Setup(ItemData item, Action onEquipClicked)
    {
        nameText.text = item.itemID;

        if (item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false; 
        }

        // 2. STATLARI HAZIRLA (StringBuilder ile)
        // Eğer slotların darsa yan yana yazması için Append(" ") kullanabilirsin.
        // Alt alta olması için AppendLine() kullan.
      
        StringBuilder sb = new StringBuilder();

        if (item.bonusStrength != 0) sb.Append($"STR: {Colorize(item.bonusStrength)}  ");
        if (item.bonusDefense != 0)  sb.Append($"DEF: {Colorize(item.bonusDefense)}  ");
        if (item.bonusSpeed != 0)    sb.Append($"SPD: {Colorize(item.bonusSpeed)}  ");
        if (item.bonusStamina != 0)  sb.Append($"STA: {Colorize(item.bonusStamina)}  ");
       

        // Eğer özellik yoksa
        if (sb.Length == 0) sb.Append("-");

        // Text'e ata (Null kontrolü yapıyoruz ki Unity'de atamayı unutursan oyun çökmesin)
        if (statsText != null) statsText.text = sb.ToString();

        // 3. Buton
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(() => onEquipClicked());
    }
    string Colorize(int val)
    {
        if (val > 0) return $"<color=green>+{val}</color>"; // Yeşil +5
        if (val < 0) return $"<color=red>{val}</color>";    // Kırmızı -2
        return val.ToString();
    }
}