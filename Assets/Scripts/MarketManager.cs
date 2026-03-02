using UnityEngine;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance;

    [Header("Tüm Eşya Havuzu (Zayıftan Güçlüye Sırala)")]
    [Tooltip("Demirci seviye atladıkça bu listedeki eşyalar sırayla açılır.")]
    public List<ItemData> allAvailableItems; 

    [Header("UI Ayarları")]
    public MarketItemUI itemUIPrefab;   // Hazırladığımız Prefab
    public Transform contentParent;     // Scroll View'in 'Content' objesi

    void Awake()
    {
        // Singleton yapısı
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Demirci binası geliştiğinde dükkanın anında güncellenmesi için event'e abone ol
        if (CampManager.Instance != null)
        {
            CampManager.Instance.OnCampUpdated += RefreshMarket;
        }

        RefreshMarket();
    }

    void OnDestroy()
    {
        if (CampManager.Instance != null)
        {
            CampManager.Instance.OnCampUpdated -= RefreshMarket;
        }
    }

    public void RefreshMarket()
    {
        // 1. Önce eski listeyi temizle (Çöp kalmasın)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Demirci seviyesine göre kaç eşya satabileceğini CampManager'dan öğren
      int currentCapacity = CampManager.Instance != null ? CampManager.Instance.GetBuildingValue("demirci") : 3;

        // 3. Sadece izin verilen kapasite kadar eşyayı vitrine koy
        for (int i = 0; i < currentCapacity; i++)
        {
            // Güvenlik Kilidi: Eğer kapasite, listendeki eşya sayısından fazlaysa hata vermesin
            if (i >= allAvailableItems.Count) break;

            ItemData itemToShow = allAvailableItems[i];

            // Prefab'ı Content'in içine yarat
            MarketItemUI uiScript = Instantiate(itemUIPrefab, contentParent);

            // Kutucuğa verisini teslim et
            uiScript.Setup(itemToShow);
        }
    }

    public void Buy(ItemData item)
    {
        // Para kontrolü
        if (MoneyManager.Instance.gold < item.price)
        {
            NotificationManager.Instance.Show($"<color=red>Yetersiz Bakiye!</color> Gereken: {item.price}", NotificationType.Warning);
            return;
        }

        // 1. Parayı düş
        if (MoneyManager.Instance.Spend(item.price))
        {
            // 2. Envantere ekle (Depoya gönder)
            if (InventoryStorage.Instance != null)
            {
                InventoryStorage.Instance.AddItem(item);
                NotificationManager.Instance.Show($"<color=green>SATIN ALINDI:</color> {item.name}", NotificationType.Success);
            }
        }
    }
}