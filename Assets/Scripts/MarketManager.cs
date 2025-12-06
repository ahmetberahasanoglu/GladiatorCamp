using UnityEngine;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance;

    [Header("Sat�lacak �r�nler")]
    public List<ItemData> itemsForSale; // ScriptableObject'leri buraya s�r�kleyeceksin

    [Header("UI Ayarlar�")]
    public MarketItemUI itemUIPrefab;   // Haz�rlad���m�z Prefab
    public Transform contentParent;     // Scroll View'in 'Content' objesi

    void Awake()
    {
        // Singleton yap�s�
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        RefreshMarket();
    }

    public void RefreshMarket()
    {
        // 1. �nce eski listeyi temizle (��p kalmas�n)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Listendeki her e�ya i�in bir kutucuk yarat
        foreach (var item in itemsForSale)
        {
            // Prefab'� Content'in i�ine yarat
            MarketItemUI uiScript = Instantiate(itemUIPrefab, contentParent);

            // Kutucu�a verisini teslim et
            uiScript.Setup(item);
        }
    }

    public void Buy(ItemData item)
    {
        // Para kontrol�
        if (MoneyManager.Instance.gold < item.price)
        {
            Debug.Log($"<color=red>Yetersiz Bakiye!</color> Gereken: {item.price}, Olan: {MoneyManager.Instance.gold}");
            return;
        }

        // 1. Paray� d��
        MoneyManager.Instance.Spend(item.price);

        // 2. Envantere ekle (Depoya g�nder)
        InventoryStorage.Instance.AddItem(item);

        Debug.Log($"<color=green>SATIN ALINDI:</color> {item.itemID}");
    }
}