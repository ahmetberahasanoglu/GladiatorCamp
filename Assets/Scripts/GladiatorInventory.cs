using UnityEngine;

public class GladiatorInventory : MonoBehaviour
{
    [Header("Üzerindeki Ekipmanlar")]
    public ItemData weapon;
    public ItemData armor;
    public ItemData helmet;
    public ItemData shield;

    private GladiatorData data; // Scriptable Object Referansý

    // Gladyatörün "Çýplak" (Doðal) güçlerini hafýzada tutmalýyýz
    // Yoksa her eþya taktýðýnda kalýcý olarak güçlenir.
    private int baseStr, baseDef, baseSpd, baseSta, baseMor;
    private bool isInitialized = false;

    void Start()
    {
        // Veriyi çek ve baz statlarý kaydet
        if (!isInitialized)
        {
            data = GetComponent<Gladiator>().data;

            // Eðer oyun baþladýysa ve statlar bozuksa sýfýrlamak için iyi bir nokta
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
        // 1. Eþyayý ilgili slot'a yerleþtir
        switch (item.type)
        {
            case ItemType.Weapon: weapon = item; break;
            case ItemType.Armor: armor = item; break;
            case ItemType.Shield: shield = item; break;
            case ItemType.Helmet: helmet = item; break;
        }

        // 2. Statlarý baþtan hesapla
        RecalculateStats();
    }

    void RecalculateStats()
    {
        // Önce karakteri "Çýplak" haline döndür
        data.strength = baseStr;
        data.defense = baseDef;
        data.speed = baseSpd;
        data.stamina = baseSta;
        data.morale = baseMor;

        // Sonra üzerindeki eþyalarýn bonuslarýný ekle
        AddBonus(weapon);
        AddBonus(armor);
        AddBonus(helmet);
        AddBonus(shield);
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