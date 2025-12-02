using UnityEngine;
using System.Collections.Generic;

public class InventoryStorage : MonoBehaviour
{
    public static InventoryStorage Instance;

    public List<ItemData> storedItems = new List<ItemData>();

    void Awake()
    {
        Instance = this;
    }

    public void AddItem(ItemData item)
    {
        storedItems.Add(item);
    }

    public void RemoveItem(ItemData item)
    {
        storedItems.Remove(item);
    }
}
