using UnityEngine;

public class GladiatorInventory : MonoBehaviour
{
    public ItemData weapon;
    public ItemData armor;
    public ItemData helmet;
    public ItemData shield;

    public GladiatorData data;

    void Start()
    {
        data = GetComponent<Gladiator>().data;
    }

    public void Equip(ItemData item)
    {
        switch (item.type)
        {
            case ItemType.Weapon:
                weapon = item;
                break;
            case ItemType.Armor:
                armor = item;
                break;
            case ItemType.Shield:
                shield = item;
                break;
            case ItemType.Helmet:
                helmet = item;
                break;
        }

        ApplyStats();
    }

    void ApplyStats()
    {
      //  data.strength = data.baseStrength;
        //data.defense = data.baseDefense;
        //data.speed = data.baseSpeed;
        //data.stamina = data.baseStamina;

        AddItemStats(weapon);
        AddItemStats(armor);
        AddItemStats(helmet);
        AddItemStats(shield);
    }

    void AddItemStats(ItemData item)
    {
        if (item == null) return;

        data.strength += item.bonusStrength;
        data.defense += item.bonusDefense;
        data.speed += item.bonusSpeed;
        data.stamina += item.bonusStamina;
    }
}
