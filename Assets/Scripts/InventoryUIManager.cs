using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Paneller")]
    public GameObject inventoryPanel;
    public Transform contentParent; // Liste
    public InventorySlotUI slotPrefab;
    
    [Header("Toplam Stat Textleri")]
    public TextMeshProUGUI totalArmorText;
    public TextMeshProUGUI totalStrengthText;
    public TextMeshProUGUI totalSpeedText;
    public TextMeshProUGUI totalStaminaText;
    [Header("Yeni Eklenenler")]
    public ItemDetailPanel detailPanel; // Sağdaki detay penceresi
    
    // Karakterin üstündekileri gösteren 4 slot
    public EquippedSlotUI weaponSlot;
    public EquippedSlotUI armorSlot;
    public EquippedSlotUI helmetSlot;
    public EquippedSlotUI shieldSlot;

    public GladiatorInventory _currentGladiator;

    void Awake()
    {
        Instance = this;
        inventoryPanel.SetActive(false);
        detailPanel.gameObject.SetActive(false); // Başta kapalı
    }
public void RefreshEquipped()
    {
   
         if (_currentGladiator == null) return;
        
        weaponSlot.Setup(_currentGladiator.weapon);
        armorSlot.Setup(_currentGladiator.armor);
        helmetSlot.Setup(_currentGladiator.helmet);
        shieldSlot.Setup(_currentGladiator.shield);

        
        int totalDef = 0;
        if (_currentGladiator.helmet) totalDef += _currentGladiator.helmet.bonusDefense;
        if (_currentGladiator.armor) totalDef += _currentGladiator.armor.bonusDefense;
        if (_currentGladiator.shield) totalDef += _currentGladiator.shield.bonusDefense;
        int totalStr=0;
        if (_currentGladiator.weapon) totalStr += _currentGladiator.weapon.bonusStrength;
        int totalSpeed=0;
        if(_currentGladiator.armor) totalSpeed += _currentGladiator.armor.bonusSpeed;
        if(_currentGladiator.helmet) totalSpeed += _currentGladiator.helmet.bonusSpeed;
        if(_currentGladiator.shield) totalSpeed += _currentGladiator.shield.bonusSpeed;
        int totalStamina=0;
        if(_currentGladiator.armor) totalStamina += _currentGladiator.armor.bonusStamina;
        if(_currentGladiator.shield) totalStamina += _currentGladiator.shield.bonusStamina;
        if(_currentGladiator.helmet) totalStamina += _currentGladiator.helmet.bonusStamina;
    

        totalArmorText.text = "Total Armor: " + totalDef;
        totalStrengthText.text = "Total Strength: " + totalStr;
        totalSpeedText.text = "Total Speed: " + totalSpeed;
        totalStaminaText.text = "Total Stamina: " + totalStamina;
   
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