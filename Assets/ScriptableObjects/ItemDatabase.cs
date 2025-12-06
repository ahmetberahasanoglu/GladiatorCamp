using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    public List<ItemData> allGameItems;

    void Awake()
    {
        Instance = this;
    }

    public ItemData GetItemByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        // Listeden ID'si eşleşen eşyayı bul
        return allGameItems.FirstOrDefault(i => i.itemID == id);
    }
}