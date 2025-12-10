using UnityEngine;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class Building
{
    public string id;           // Kodun tanıması için kimlik (örn: kogus)
    public string displayName;  // Ekranda yazan isim (örn: Acemi Koğuşu)
    public int level = 1;       // Kaçıncı seviye?
    public int baseCost = 200;  // İlk ücret
    public int baseValue = 3;   // İlk kapasite
    public int increasePerLevel = 2; // Her seviyede kaç artıyor?

    // ŞU ANKİ ÜCRET
    public int GetCost() 
    {
        return baseCost * level; // Seviye 1: 200, Seviye 2: 400...
    }

    // ŞU ANKİ KAPASİTE
    public int GetValue()
    {
        return baseValue + ((level - 1) * increasePerLevel); 
        // Lvl 1: 3 + 0 = 3
        // Lvl 2: 3 + 2 = 5
    }

    // SONRAKİ KAPASİTE (Önizleme için)
    public int GetNextValue()
    {
        return GetValue() + increasePerLevel;
    }
}

public class CampManager : MonoBehaviour
{
    public static CampManager Instance;
    
    // Binaları burada tutuyoruz
    public List<Building> buildings = new List<Building>();

    void Awake()
    {
        Instance = this;
        InitializeBuildings(); // Binaları oluştur
    }

    void InitializeBuildings()
    {
        // Eğer listede Koğuş yoksa, otomatik ekle
        if (!buildings.Any(b => b.id == "kogus"))
        {
            Building kogus = new Building();
            kogus.id = "kogus";
            kogus.displayName = "Acemi Koğuşu";
            kogus.baseCost = 250;  // Geliştirme ücreti
            kogus.baseValue = 3;   // Başlangıç kapasitesi (3 asker)
            kogus.increasePerLevel = 2; // Her levelde +2 kapasite

            buildings.Add(kogus);
        }
        
        // İleride buraya "Talimhane", "Aşevi" vb. ekleyeceğiz.
    }

    // Kapasiteyi soran fonksiyon
    public int GetMaxSoldierCapacity()
    {
        var b = buildings.FirstOrDefault(x => x.id == "kogus");
        if (b != null) return b.GetValue();
        return 3; // Hata olursa varsayılan 3
    }

    public void UpgradeBuilding(string id)
    {
        var b = buildings.FirstOrDefault(x => x.id == id);
        if (b == null) return;

        int cost = b.GetCost();

        if (MoneyManager.Instance.gold >= cost)
        {
            MoneyManager.Instance.Spend(cost);
            b.level++;
            Debug.Log($"{b.displayName} seviye atladı! Yeni Seviye: {b.level}, Kapasite: {b.GetValue()}");
        }
        else
        {
            Debug.Log("Yetersiz Bakiye!");
        }
    }
}