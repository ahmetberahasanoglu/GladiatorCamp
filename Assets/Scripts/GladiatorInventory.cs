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

       // Oyun başladığında üstünde bir şeyler varsa görselini de aç
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

        // Sadece hesaplama yapmadan giyiliyorsa bile görseli aç
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
            NotificationManager.Instance.Show($"{oldItem.itemID} depoya geri gönderildi.", NotificationType.Info);
            
            // --- ESKİ EŞYANIN GÖRSELİNİ KAPAT ---
            ToggleMesh(oldItem.targetMeshName, false);
        }

        // --- YENİ EŞYANIN GÖRSELİNİ AÇ ---
        ToggleMesh(newItem.targetMeshName, true);

        RecalculateStats();
           if (gladiator != null)
        {
            gladiator.RecalculateMaxHealth();
           gladiator.RefreshStats(); // Eğer UI yenilenmiyorsa bu satırı da aktif edebilirsin
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

        // --- ÇIKARILAN EŞYANIN GÖRSELİNİ KAPAT ---
        if (removedItem != null)
        {
            ToggleMesh(removedItem.targetMeshName, false);
        }

        RecalculateStats();
        Gladiator gladiator = GetComponent<Gladiator>();
        if (gladiator != null)
        {
            gladiator.RecalculateMaxHealth();
            gladiator.RefreshStats();
        }

    }

    // --- YENİ: SİHİRLİ GÖRSEL DEĞİŞTİRİCİ ---
    private void ToggleMesh(string meshName, bool state)
    {
        if (string.IsNullOrEmpty(meshName)) return;

        // Karakterin altındaki tüm objeleri (kapalılar dahil) tarar ve ismi eşleşeni bulur
        Transform[] allChildren = GetComponentsInChildren<Transform>(true); 
        foreach (Transform child in allChildren)
        {
            if (child.name == meshName)
            {
                child.gameObject.SetActive(state);
                return; // Bulduk, işlemi bitir
            }
        }
        
        Debug.LogWarning($"<color=yellow>DİKKAT:</color> Karakterin içinde '{meshName}' adında bir model bulunamadı!");
    }
    // ------------------------------------------

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
        GetComponent<Gladiator>().RefreshStats();
    }

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