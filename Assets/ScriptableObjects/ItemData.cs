using UnityEngine;

public enum ItemType
{
    Weapon,
    Armor,
    Shield,
    Helmet
}


public enum ItemSetType
{
    None,      
    Fire,       // Ateş  (Yakma)
    Poison,     // Zehir (Zamanla Hasar)
    Faith       // İnanç (Şifa / Kutsal Alan)
}

[CreateAssetMenu(menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public ItemType type;
    
    [Header("Set ve Sinerji Ayarları (YENİ)")]
    [Tooltip("Bu eşyanın hangi efsanevi sete ait olduğunu belirler.")]
    public ItemSetType setType = ItemSetType.None; 

    [Header("Görsel ve Fiyat")]
    public Sprite icon;
    public int price;

    [Header("Skinned Mesh Ayarı")]
    [Tooltip("Karakterin içindeki açılacak olan mesh objesinin tam adı (Örn: 'Sword_Iron')")]
    public string targetMeshName; 

    [Header("Stats")]
    public int bonusStrength;
    public int bonusDefense;
    public int bonusSpeed;
    public int bonusStamina;
    public float weaponRange = 2.0f; // Kılıç 2, Mızrak 3.5, Yay 10 
    public bool isRanged = false; 
}