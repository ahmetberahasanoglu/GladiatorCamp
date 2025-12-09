using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour//,IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Bileşenleri")]
    public TextMeshProUGUI nameText;
    public Button equipButton;
    public Image iconImage; 
    public TextMeshProUGUI statsText;
    private ItemData _myItem;
    public void Setup(ItemData item, Action onEquipClicked)
    {
        _myItem = item;
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
        // Alt alta olması için AppendLine() kullanılabilr
      
        StringBuilder sb = new StringBuilder();

        if (item.bonusStrength != 0) sb.Append($"STR: {Colorize(item.bonusStrength)}  ");
        if (item.bonusDefense != 0)  sb.Append($"DEF: {Colorize(item.bonusDefense)}  ");
        if (item.bonusSpeed != 0)    sb.Append($"SPD: {Colorize(item.bonusSpeed)}  ");
        if (item.bonusStamina != 0)  sb.Append($"STA: {Colorize(item.bonusStamina)}  ");
       

        // Eğer özellik yoksa
        if (sb.Length == 0) sb.Append("-");
        if (statsText != null) statsText.text = sb.ToString();
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(() => onEquipClicked());
    }
    string Colorize(int val)
    {
        if (val > 0) return $"<color=green>+{val}</color>"; // Yeşil +5
        if (val < 0) return $"<color=red>{val}</color>";    // Kırmızı -2
        return val.ToString();
    }
     /*
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Hangi askere baktığımızı bulmamız lazım (InventoryUI'dan çekebilirsin)
        var currentGlad = InventoryUIManager.Instance._currentGladiator; 
        TooltipManager.Instance.ShowTooltip(_myItem, currentGlad);
    }

   
    public void OnPointerExit(PointerEventData eventData)
    {
       // TooltipManager.Instance.HideTooltip();
    }*/
}