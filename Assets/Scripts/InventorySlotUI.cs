using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; 

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public TextMeshProUGUI nameText;
    public Button equipButton;
    public Image iconImage; // YENİ EKLENEN: İkon resmi için referans

    // Manager'ın çağırdığı Setup fonksiyonu
    public void Setup(ItemData item, Action onEquipClicked)
    {
        // Eşyanın ID'sini veya İsmini yaz
        nameText.text = item.itemID; // itemID yerine itemName daha güzel görünür

        // YENİ EKLENEN: İkonu güncelle
        if (item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true; // Resim varsa göster
        }
        else
        {
            // Resim yoksa boş beyaz kare görünmesin diye kapatabiliriz
            // veya varsayılan bir resim atayabiliriz.
            iconImage.enabled = false; 
        }

        // Buton ayarları
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(() => onEquipClicked());
    }
}