using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action kullanmak için gerekli

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI Bileþenleri")]
    public TextMeshProUGUI nameText;
    public Button equipButton;

    // Bu fonksiyonu Manager çaðýracak ve "Týklanýnca ne yapacaðýný" söyleyecek
    public void Setup(ItemData item, Action onEquipClicked)
    {
        nameText.text = item.itemName;

        // Butona týklanýnca Manager'dan gelen emri uygula
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(() => onEquipClicked());
    }
}