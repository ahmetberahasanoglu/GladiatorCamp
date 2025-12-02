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
    public string itemName;
    public ItemType type;
    public int price;
   // [TextArea(3, 10)] public string description;

    [Header("Stats")]
    public int bonusStrength;
    public int bonusDefense;
    public int bonusSpeed;
    public int bonusStamina;
}
