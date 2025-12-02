using UnityEngine;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance;

    [Header("Satýlacak Ürünler")]
    public List<ItemData> itemsForSale; // ScriptableObject'leri buraya sürükleyeceksin

    [Header("UI Ayarlarý")]
    public MarketItemUI itemUIPrefab;   // Hazýrladýðýmýz Prefab
    public Transform contentParent;     // Scroll View'in 'Content' objesi

    void Awake()
    {
        // Singleton yapýsý
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        RefreshMarket();
    }

    public void RefreshMarket()
    {
        // 1. Önce eski listeyi temizle (Çöp kalmasýn)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Listendeki her eþya için bir kutucuk yarat
        foreach (var item in itemsForSale)
        {
            // Prefab'ý Content'in içine yarat
            MarketItemUI uiScript = Instantiate(itemUIPrefab, contentParent);

            // Kutucuða verisini teslim et
            uiScript.Setup(item);
        }
    }

    public void Buy(ItemData item)
    {
        // Para kontrolü
        if (MoneyManager.Instance.gold < item.price)
        {
            Debug.Log($"<color=red>Yetersiz Bakiye!</color> Gereken: {item.price}, Olan: {MoneyManager.Instance.gold}");
            return;
        }

        // 1. Parayý düþ
        MoneyManager.Instance.Spend(item.price);

        // 2. Envantere ekle (Depoya gönder)
        InventoryStorage.Instance.AddItem(item);

        Debug.Log($"<color=green>SATIN ALINDI:</color> {item.itemName}");
    }
}