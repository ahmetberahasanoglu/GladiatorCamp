using UnityEngine;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Paneller")]
    public GameObject inventoryPanel;
    public Transform contentParent; // Liste
    public InventorySlotUI slotPrefab;
    
    [Header("Yeni Eklenenler")]
    public ItemDetailPanel detailPanel; // Sağdaki detay penceresi
    
    // Karakterin üstündekileri gösteren 4 slot
    public EquippedSlotUI weaponSlot;
    public EquippedSlotUI armorSlot;
    public EquippedSlotUI helmetSlot;
    public EquippedSlotUI shieldSlot;

    private GladiatorInventory _currentGladiator;

    void Awake()
    {
        Instance = this;
        inventoryPanel.SetActive(false);
        detailPanel.gameObject.SetActive(false); // Başta kapalı
    }

    public void OpenInventoryFor(GladiatorInventory gladiator)
    {
        _currentGladiator = gladiator;
        inventoryPanel.SetActive(true);
        
        RefreshList();      // Depodaki eşyalar
        RefreshEquipped();  // Üstündeki eşyalar
        
        // Detay panelini kapat (Yeni açılışta bir şey seçili değil)
        detailPanel.gameObject.SetActive(false);
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        _currentGladiator = null;
    }

    void RefreshList()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        foreach (var item in InventoryStorage.Instance.storedItems)
        {
            var slot = Instantiate(slotPrefab, contentParent);
            
            // DİKKAT: Tıklanınca artık 'EquipItem' DEĞİL, 'ShowDetails' çağırıyoruz.
            // slot.Setup fonksiyonunu güncellememiz gerekecek (Aşağıda belirttim)
            slot.Setup(item, () => OnItemSelected(item));
        }
    }

    // Üstündeki eşyaları güncelle
    public void RefreshEquipped()
    {
        if (_currentGladiator == null) return;
        
        weaponSlot.Setup(_currentGladiator.weapon);
        armorSlot.Setup(_currentGladiator.armor);
        helmetSlot.Setup(_currentGladiator.helmet);
        shieldSlot.Setup(_currentGladiator.shield);
    }

    // Listeden bir şeye tıklanınca
    void OnItemSelected(ItemData item)
    {
        // Detay panelini aç ve karşılaştır
        detailPanel.ShowDetails(item, _currentGladiator);
    }

    // Detay panelindeki "Kuşan" butonuna basılınca çalışır (Public yaptık)
    public void EquipItem(ItemData item)
    {
        // 1. Depodan sil
        InventoryStorage.Instance.RemoveItem(item);
        
        // 2. Giydir (Eski eşya otomatik depoya gider - Önceki kodumuzda yapmıştık)
        _currentGladiator.Equip(item);

        // 3. Her şeyi yenile
        RefreshList();
        RefreshEquipped();
    }
}