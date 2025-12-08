using UnityEngine;
using UnityEngine.UI;

public class EquippedSlotUI : MonoBehaviour
{
    public ItemType slotType; // Inspector'dan seç: Weapon, Armor vs.
    public Image iconImage;   // Eşyanın resmi
    public Sprite emptySprite; // Boşken görünecek siluet (Gri kılıç izi vs.)

    public void Setup(ItemData item)
    {
        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white; // Görünür yap
        }
        else
        {
            iconImage.sprite = emptySprite; // Boş siluet
            // Veya tamamen gizle: iconImage.color = new Color(1,1,1,0); 
        }
    }
}