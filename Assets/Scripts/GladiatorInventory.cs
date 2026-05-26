using UnityEngine;
using System.Collections.Generic;

public class GladiatorInventory : MonoBehaviour
{
    [Header("Üzerindeki Ekipmanlar")]
    public ItemData weapon;
    public ItemData armor;
    public ItemData helmet;
    public ItemData shield;

    [Header("Efsanevi Set Sistemi (YENİ)")]
    [Tooltip("Askerin üzerinde anlık olarak aktif olan set bonusu")]
    public ItemSetType activeSet = ItemSetType.None; 
    public int activeSetPieceCount = 0; // Kaç parça takılı olduğunu tutar

    public JanissaryData data; 
    private int baseStr, baseDef, baseSpd, baseSta, baseMor;
    private bool isInitialized = false;

    void Start()
    {
       InitializeBaseStats();

       if (weapon != null) ToggleMesh(weapon.targetMeshName, true);
       if (armor != null) ToggleMesh(armor.targetMeshName, true);
       if (helmet != null) ToggleMesh(helmet.targetMeshName, true);
       if (shield != null) ToggleMesh(shield.targetMeshName, true);
       
       RecalculateStats();
       Gladiator gladiator = GetComponent<Gladiator>();
       if (gladiator != null)
       {
           gladiator.RecalculateMaxHealth();
       }
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

        ToggleMesh(item.targetMeshName, true);
    }

    public void Equip(ItemData newItem)
    {
        if (newItem == null) return;
        if (data == null)
        {
            data = GetComponent<Gladiator>().data;
            if (data == null) return;
        }
        Gladiator gladiator = GetComponent<Gladiator>();

        ItemData oldItem = null; 

        switch (newItem.type)
        {
            case ItemType.Weapon: oldItem = weapon; weapon = newItem; break;
            case ItemType.Armor:  oldItem = armor; armor = newItem; break;
            case ItemType.Shield: oldItem = shield; shield = newItem; break;
            case ItemType.Helmet: oldItem = helmet; helmet = newItem; break;
        }

        if (oldItem != null)
        {
            InventoryStorage.Instance.AddItem(oldItem);
            NotificationManager.Instance.Show($"{oldItem.itemID} depoya geri gönderildi.", NotificationType.Info);
            ToggleMesh(oldItem.targetMeshName, false);
        }

        ToggleMesh(newItem.targetMeshName, true);

        RecalculateStats();
        
        if (gladiator != null)
        {
            gladiator.RecalculateMaxHealth();
            gladiator.RefreshStats(); 
        }
    }

    public void RemoveItem(ItemType type)
    {
        ItemData removedItem = null;

        switch (type)
        {
            case ItemType.Weapon: removedItem = weapon; weapon = null; break;
            case ItemType.Armor:  removedItem = armor; armor = null; break;
            case ItemType.Helmet: removedItem = helmet; helmet = null; break;
            case ItemType.Shield: removedItem = shield; shield = null; break;
        }

        if (removedItem != null) ToggleMesh(removedItem.targetMeshName, false);

        RecalculateStats();
        
        Gladiator gladiator = GetComponent<Gladiator>();
        if (gladiator != null)
        {
            gladiator.RecalculateMaxHealth();
            gladiator.RefreshStats();
        }
    }

    private void ToggleMesh(string meshName, bool state)
    {
        if (string.IsNullOrEmpty(meshName)) return;

        Transform[] allChildren = GetComponentsInChildren<Transform>(true); 
        foreach (Transform child in allChildren)
        {
            if (child.name == meshName)
            {
                child.gameObject.SetActive(state);
                return; 
            }
        }
        Debug.LogWarning($"<color=yellow>DİKKAT:</color> Karakterin içinde '{meshName}' adında bir model bulunamadı!");
    }

   void RecalculateStats()
    {
        data.strength = baseStr;
        data.defense = baseDef;
        data.speed = baseSpd;
        data.stamina = baseSta;

        AddBonus(weapon);
        AddBonus(armor);
        AddBonus(helmet);
        AddBonus(shield);
        
        // ── YENİ: SİLAH TÜRÜ VE MENZİLİNİ DATA'YA AKTAR ──
        if (weapon != null)
        {
            data.weaponClass = weapon.weaponClass;
            data.attackRange = weapon.weaponRange;
            data.isRanged    = weapon.isRanged;
        }
        else
        {
            // Silahsızsa (Yumruk) varsayılan değerler
            data.weaponClass = WeaponClass.Unarmed; 
            data.attackRange = 1.5f; 
            data.isRanged    = false;
        }
        // ─────────────────────────────────────────────────

        CalculateSetBonus();

        GetComponent<Gladiator>().RefreshStats();
    }

    // --- YENİ: SET DEDEKTİFİ ---
    void CalculateSetBonus()
    {
        activeSet = ItemSetType.None;
        activeSetPieceCount = 0;

        // Üzerimizdeki setleri saymak için geçici sayaçlar
        int fireCount = 0;
        int poisonCount = 0;
        int faithCount = 0;

        // Eşyaları kontrol et ve sayaçları artır
        CheckAndCountSet(weapon, ref fireCount, ref poisonCount, ref faithCount);
        CheckAndCountSet(armor, ref fireCount, ref poisonCount, ref faithCount);
        CheckAndCountSet(helmet, ref fireCount, ref poisonCount, ref faithCount);
        CheckAndCountSet(shield, ref fireCount, ref poisonCount, ref faithCount);

        // Hangi setten en az 3 parça varsa, onu "Aktif Set" olarak belirle
        if (fireCount >= 3) { activeSet = ItemSetType.Fire; activeSetPieceCount = fireCount; }
        else if (poisonCount >= 3) { activeSet = ItemSetType.Poison; activeSetPieceCount = poisonCount; }
        else if (faithCount >= 3) { activeSet = ItemSetType.Faith; activeSetPieceCount = faithCount; }
    }

    // Set türünü okuyup ilgili sayacı artıran yardımcı metod
    void CheckAndCountSet(ItemData item, ref int fCount, ref int pCount, ref int faCount)
    {
        if (item == null) return;
        
        if (item.setType == ItemSetType.Fire) fCount++;
        else if (item.setType == ItemSetType.Poison) pCount++;
        else if (item.setType == ItemSetType.Faith) faCount++;
    }
    // ------------------------------------

    public ItemData GetEquippedItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon: return weapon;
            case ItemType.Armor:  return armor;
            case ItemType.Helmet: return helmet;
            case ItemType.Shield: return shield;
            default: return null;
        }
    }

    void AddBonus(ItemData item)
    {
        if (item == null) return;
        data.strength += item.bonusStrength;
        data.defense += item.bonusDefense;
        data.speed += item.bonusSpeed;
        data.stamina += item.bonusStamina;
    }

    public void PermanentlyIncreaseStat(TrainingType type, int amount)
    {
        switch (type)
        {
            case TrainingType.Strength: baseStr += amount; break;
            case TrainingType.Defense:  baseDef += amount; break;
            case TrainingType.Speed:    baseSpd += amount; break;
            case TrainingType.Stamina:  baseSta += amount; break;
        }
        RecalculateStats(); 
    }
}