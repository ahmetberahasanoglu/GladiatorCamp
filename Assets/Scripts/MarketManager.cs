using UnityEngine;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance;

    public List<ItemData> itemsForSale;
    public MarketItemUI itemUIPrefab;
    public Transform contentParent;
    void Start()
    {
        foreach (var item in itemsForSale)
        {
            var ui = Instantiate(itemUIPrefab, contentParent);
            ui.item = item;  // REFERANS BURADA VERÝLCEK
        }
    }
    void Awake()
    {
        Instance = this;
    }

    public void Buy(ItemData item)
    {
        if (MoneyManager.Instance.gold < item.price)
        {
            Debug.Log("Yeterli altýn yok!");
            return;
        }

        MoneyManager.Instance.Spend(item.price);

        InventoryStorage.Instance.AddItem(item);

        Debug.Log(item.itemName + " satýn alýndý!");
    }
}
