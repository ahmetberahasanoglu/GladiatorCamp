using UnityEngine;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("UI Referanslar�")]
    public GameObject inventoryPanel; 
    public Transform contentParent;   
    public InventorySlotUI slotPrefab; 

    private GladiatorInventory _targetGladiator; 

    void Awake()
    {
        Instance = this;
        inventoryPanel.SetActive(false);
    }

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


    void RefreshList()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);
        foreach (var item in InventoryStorage.Instance.storedItems)
        {
            var slot = Instantiate(slotPrefab, contentParent);
            slot.Setup(item, () => EquipItem(item));
        }
    }


    void EquipItem(ItemData itemToEquip)
    {
        if (_targetGladiator != null)
        {
             InventoryStorage.Instance.RemoveItem(itemToEquip);
            _targetGladiator.Equip(itemToEquip);
            RefreshList();
            Debug.Log($"{itemToEquip.itemID} kuşandı!");
        }
    }
}