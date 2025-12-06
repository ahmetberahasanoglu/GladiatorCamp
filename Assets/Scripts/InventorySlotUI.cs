using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action kullanmak i�in gerekli

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI Bile�enleri")]
    public TextMeshProUGUI nameText;
    public Button equipButton;

    // Bu fonksiyonu Manager �a��racak ve "T�klan�nca ne yapaca��n�" s�yleyecek
    public void Setup(ItemData item, Action onEquipClicked)
    {
        nameText.text = item.itemID;

        // Butona t�klan�nca Manager'dan gelen emri uygula
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(() => onEquipClicked());
    }
}