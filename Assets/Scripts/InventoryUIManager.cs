using UnityEngine;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("UI Referanslar�")]
    public GameObject inventoryPanel; // A��l�p kapanacak ana pencere
    public Transform contentParent;   // S�ralanacak yer (Grid)
    public InventorySlotUI slotPrefab; // Az �nce yazd���m�z kutucuk

    private GladiatorInventory _targetGladiator; // O an giydirdi�imiz gladyat�r

    void Awake()
    {
        Instance = this;
        inventoryPanel.SetActive(false); // Oyun ba�larken kapal� olsun
    }

    // 1. ADIM: Gladyat�re t�klay�nca bu �al��acak
    public void OpenInventoryFor(GladiatorInventory gladiator)
    {
        _targetGladiator = gladiator;
        inventoryPanel.SetActive(true);
        RefreshList();
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        _targetGladiator = null;
    }

    // 2. ADIM: Depodaki e�yalar� listele
    void RefreshList()
    {
        // Temizlik
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        // Depodaki her e�ya i�in kutucuk �ret
        foreach (var item in InventoryStorage.Instance.storedItems)
        {
            var slot = Instantiate(slotPrefab, contentParent);

            // Kutucu�a g�rev ver: "Bana t�klan�rsa EquipItem fonksiyonunu �al��t�r"
            slot.Setup(item, () => EquipItem(item));
        }
    }

    // 3. ADIM: E�yay� Giydir
    void EquipItem(ItemData item)
    {
        if (_targetGladiator != null)
        {
            // Gladyat�re e�yay� ver
            _targetGladiator.Equip(item);

            // E�yay� depodan sil (Tek kullan�ml�k mant���)
            InventoryStorage.Instance.RemoveItem(item);

            // Listeyi yenile (E�ya listeden gitsin)
            RefreshList();

            Debug.Log($"{item.itemID} ku�and�!");
        }
    }
}