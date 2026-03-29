using UnityEngine;

public enum ItemType
{
    Weapon,
    Armor,
    Shield,
    Helmet
}

[CreateAssetMenu(menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public ItemType type;
    public Sprite icon;
    public int price;

    [Header("Skinned Mesh Ayarı (YENİ)")]
    [Tooltip("Karakterin içindeki açılacak olan mesh objesinin tam adı (Örn: 'Sword_Iron')")]
    public string targetMeshName; 

    [Header("Stats")]
    public int bonusStrength;
    public int bonusDefense;
    public int bonusSpeed;
    public int bonusStamina;
}