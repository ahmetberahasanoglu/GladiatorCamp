using UnityEngine;

public class GladiatorInventory : MonoBehaviour
{
    [Header("Üzerindeki Ekipmanlar")]
    public ItemData weapon;
    public ItemData armor;
    public ItemData helmet;
    public ItemData shield;

    private JanissaryData data; 
    private int baseStr, baseDef, baseSpd, baseSta, baseMor;
    private bool isInitialized = false;

    void Start()
    {
        // Veriyi �ek ve baz statlar� kaydet
        if (!isInitialized)
        {
            data = GetComponent<Gladiator>().data;

            // E�er oyun ba�lad�ysa ve statlar bozuksa s�f�rlamak i�in iyi bir nokta
            baseStr = data.strength;
            baseDef = data.defense;
            baseSpd = data.speed;
            baseSta = data.stamina;
            baseMor = data.morale;

            isInitialized = true;
        }
    }

    public void Equip(ItemData item)
    {
        // 1. E�yay� ilgili slot'a yerle�tir
        switch (item.type)
        {
            case ItemType.Weapon: weapon = item; break;
            case ItemType.Armor: armor = item; break;
            case ItemType.Shield: shield = item; break;
            case ItemType.Helmet: helmet = item; break;
        }

        // 2. Statlar� ba�tan hesapla
        RecalculateStats();
    }

    void RecalculateStats()
    {
        // �nce karakteri "��plak" haline d�nd�r
        data.strength = baseStr;
        data.defense = baseDef;
        data.speed = baseSpd;
        data.stamina = baseSta;
        data.morale = baseMor;

        // Sonra �zerindeki e�yalar�n bonuslar�n� ekle
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