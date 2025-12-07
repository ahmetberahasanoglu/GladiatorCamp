using UnityEngine;

public class GladiatorInventory : MonoBehaviour
{
    [Header("Üzerindeki Ekipmanlar")]
    public ItemData weapon;
    public ItemData armor;
    public ItemData helmet;
    public ItemData shield;

    public JanissaryData data; 
    private int baseStr, baseDef, baseSpd, baseSta, baseMor;
    private bool isInitialized = false;

    void Start()
    {
       InitializeBaseStats();
    }
public void InitializeBaseStats()
    {
        if (isInitialized) return; 

        if (data == null) data = GetComponent<Gladiator>().data;
        if (data == null) return;

    
        baseStr = data.strength;
        baseDef = data.defense;
        baseSpd = data.speed;
        baseSta = data.stamina;
        baseMor = data.morale;

        isInitialized = true;
    }
    public void EquipWithoutCalc(ItemData item)
    {
        if (item == null) return;
        
        switch (item.type)
        {
            case ItemType.Weapon: weapon = item; break;
            case ItemType.Armor: armor = item; break;
            case ItemType.Shield: shield = item; break;
            case ItemType.Helmet: helmet = item; break;
        }
        
      
    }
   // GladiatorInventory.cs içindeki Equip fonksiyonunu bununla değiştir:

    public void Equip(ItemData newItem)
    {
        if (newItem == null) return;
        if (data == null)
        {
            data = GetComponent<Gladiator>().data;
            if (data == null) return;
        }

        ItemData oldItem = null; 

    
        switch (newItem.type)
        {
            case ItemType.Weapon:
                oldItem = weapon; 
                weapon = newItem; 
                break;

            case ItemType.Armor:
                oldItem = armor;
                armor = newItem;
                break;

            case ItemType.Shield:
                oldItem = shield;
                shield = newItem;
                break;

            case ItemType.Helmet:
                oldItem = helmet;
                helmet = newItem;
                break;
        }

        if (oldItem != null)
        {
            InventoryStorage.Instance.AddItem(oldItem);
            Debug.Log($"{oldItem.itemID} depoya geri gönderildi.");
        }
        RecalculateStats();
    }
    void RecalculateStats()
    {
        data.strength = baseStr;
        data.defense = baseDef;
        data.speed = baseSpd;
        data.stamina = baseSta;
        data.morale = baseMor;

        AddBonus(weapon);
        AddBonus(armor);
        AddBonus(helmet);
        AddBonus(shield);
        GetComponent<Gladiator>().RefreshStats();
    }

    void AddBonus(ItemData item)
    {
        if (item == null) return;
        data.strength += item.bonusStrength;
        data.defense += item.bonusDefense;
        data.speed += item.bonusSpeed;
        data.stamina += item.bonusStamina;
    }
}