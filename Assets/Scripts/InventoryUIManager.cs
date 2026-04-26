using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Paneller")]
    public GameObject inventoryPanel;
    public Transform contentParent; 
    public InventorySlotUI slotPrefab;
    
    [Header("Toplam Stat Textleri")]
    public TextMeshProUGUI totalArmorText;
    public TextMeshProUGUI totalStrengthText;
    public TextMeshProUGUI totalSpeedText;
    public TextMeshProUGUI totalStaminaText;
    
    // --- YENİ EKLENEN UI ELEMENTİ ---
    [Tooltip("Set durumunu ve sinerjiyi gösterecek Text objesi")]
    public TextMeshProUGUI setBonusText; 

    [Header("Yeni Eklenenler")]
    public ItemDetailPanel detailPanel; 
    
    public EquippedSlotUI weaponSlot;
    public EquippedSlotUI armorSlot;
    public EquippedSlotUI helmetSlot;
    public EquippedSlotUI shieldSlot;

    public GladiatorInventory _currentGladiator;

    void Awake()
    {
        Instance = this;
        inventoryPanel.SetActive(false);
        detailPanel.gameObject.SetActive(false); 
    }

    public void UnequipItem(ItemType type)
    {
        if (_currentGladiator == null) return;

        ItemData itemToRemove = _currentGladiator.GetEquippedItem(type);

        if (itemToRemove != null)
        {
            _currentGladiator.RemoveItem(type); 
            InventoryStorage.Instance.AddItem(itemToRemove);
            
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show($"{itemToRemove.itemID} depoya kaldırıldı.", NotificationType.Info);
            
            RefreshList();    
            RefreshEquipped();
        }
    }

    public void RefreshEquipped()
    {
         if (_currentGladiator == null) return;
        
        weaponSlot.Setup(_currentGladiator.weapon);
        armorSlot.Setup(_currentGladiator.armor);
        helmetSlot.Setup(_currentGladiator.helmet);
        shieldSlot.Setup(_currentGladiator.shield);

        int totalDef = 0, totalStr = 0, totalSpeed = 0, totalStamina = 0;

        if (_currentGladiator.helmet) { totalDef += _currentGladiator.helmet.bonusDefense; totalSpeed += _currentGladiator.helmet.bonusSpeed; totalStamina += _currentGladiator.helmet.bonusStamina; }
        if (_currentGladiator.armor) { totalDef += _currentGladiator.armor.bonusDefense; totalSpeed += _currentGladiator.armor.bonusSpeed; totalStamina += _currentGladiator.armor.bonusStamina; }
        if (_currentGladiator.shield) { totalDef += _currentGladiator.shield.bonusDefense; totalSpeed += _currentGladiator.shield.bonusSpeed; totalStamina += _currentGladiator.shield.bonusStamina; }
        if (_currentGladiator.weapon) totalStr += _currentGladiator.weapon.bonusStrength;

        totalArmorText.text = "Armor: " + totalDef;
        totalStrengthText.text = "Strength: " + totalStr;
        totalSpeedText.text = "Speed: " + totalSpeed;
        totalStaminaText.text = "Stamina: " + totalStamina;

        // --- YENİ: SET BİLGİSİ VE SİNERJİ HESAPLAMA ---
        UpdateSetBonusUI();
    }

    // Ekrandaki 1/3, 2/3 yazılarını ve Sinerjileri günceller
    private void UpdateSetBonusUI()
    {
        if (setBonusText == null || _currentGladiator == null) return;

        int fireCount = 0, poisonCount = 0, faithCount = 0;

        CountSetPieces(_currentGladiator.weapon, ref fireCount, ref poisonCount, ref faithCount);
        CountSetPieces(_currentGladiator.armor, ref fireCount, ref poisonCount, ref faithCount);
        CountSetPieces(_currentGladiator.helmet, ref fireCount, ref poisonCount, ref faithCount);
        CountSetPieces(_currentGladiator.shield, ref fireCount, ref poisonCount, ref faithCount);

        string setText = "";
        
        // Renkli ve şık bir gösterim
        if (fireCount > 0) setText += $"Ateş Seti: <color=#FF5733>{fireCount}/3</color>\n";
        if (poisonCount > 0) setText += $"Zehir Seti: <color=#33FF57>{poisonCount}/3</color>\n";
        if (faithCount > 0) setText += $"İnanç Seti: <color=#F0E68C>{faithCount}/3</color>\n";

        if (setText == "") setText = "<color=#888888>Aktif Set Yok</color>";

        // --- FITRAT VE SET SİNERJİSİ KONTROLÜ ---
        // Askerin fıtratının JanissaryData (data) içinde 'trait' adında bir değişken olduğunu varsayıyoruz
        if (_currentGladiator.data != null)
        {
            SoldierTrait currentTrait = _currentGladiator.data.trait; 

            if (currentTrait == SoldierTrait.Obur && poisonCount >= 3)
                setText += "\n<b><color=#33FF57>SİNERJİ AKTİF: Asitli Mide (Obur+Zehir)</color></b>";
                
            else if (currentTrait == SoldierTrait.Dindar && faithCount >= 3)
                setText += "\n<b><color=#F0E68C>SİNERJİ AKTİF: Kutsal İrade (Dindar+İnanç)</color></b>";
                
            else if (currentTrait == SoldierTrait.Yetenekli && fireCount >= 3)
                setText += "\n<b><color=#FF5733>SİNERJİ AKTİF: Cehennem Ustas (Yetenekli+Ateş)</color></b>";
        }

        setBonusText.text = setText;
    }

    private void CountSetPieces(ItemData item, ref int f, ref int p, ref int fa)
    {
        if (item == null) return;
        if (item.setType == ItemSetType.Fire) f++;
        else if (item.setType == ItemSetType.Poison) p++;
        else if (item.setType == ItemSetType.Faith) fa++;
    }

    public void OpenInventoryFor(GladiatorInventory gladiator)
    {
        _currentGladiator = gladiator;
        inventoryPanel.SetActive(true);
        RefreshList();      
        RefreshEquipped();  
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
            slot.Setup(item, () => OnItemSelected(item));
        }
    }

    void OnItemSelected(ItemData item)
    {
        detailPanel.ShowDetails(item, _currentGladiator);
    }

    public void EquipItem(ItemData item)
    {
        InventoryStorage.Instance.RemoveItem(item);
        _currentGladiator.Equip(item);
        RefreshList();
        RefreshEquipped();
    }
}