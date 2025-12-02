using UnityEngine;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("UI Referanslarý")]
    public GameObject inventoryPanel; // Açýlýp kapanacak ana pencere
    public Transform contentParent;   // Sýralanacak yer (Grid)
    public InventorySlotUI slotPrefab; // Az önce yazdýðýmýz kutucuk

    private GladiatorInventory _targetGladiator; // O an giydirdiðimiz gladyatör

    void Awake()
    {
        Instance = this;
        inventoryPanel.SetActive(false); // Oyun baþlarken kapalý olsun
    }

    // 1. ADIM: Gladyatöre týklayýnca bu çalýþacak
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

    // 2. ADIM: Depodaki eþyalarý listele
    void RefreshList()
    {
        // Temizlik
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        // Depodaki her eþya için kutucuk üret
        foreach (var item in InventoryStorage.Instance.storedItems)
        {
            var slot = Instantiate(slotPrefab, contentParent);

            // Kutucuða görev ver: "Bana týklanýrsa EquipItem fonksiyonunu çalýþtýr"
            slot.Setup(item, () => EquipItem(item));
        }
    }

    // 3. ADIM: Eþyayý Giydir
    void EquipItem(ItemData item)
    {
        if (_targetGladiator != null)
        {
            // Gladyatöre eþyayý ver
            _targetGladiator.Equip(item);

            // Eþyayý depodan sil (Tek kullanýmlýk mantýðý)
            InventoryStorage.Instance.RemoveItem(item);

            // Listeyi yenile (Eþya listeden gitsin)
            RefreshList();

            Debug.Log($"{item.itemName} kuþandý!");
        }
    }
}