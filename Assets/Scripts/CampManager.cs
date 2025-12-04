using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Building
{
    public string id;           // Kodda tanımak için (Örn: "kogus")
    public string displayName;  // Ekranda görünecek (Örn: "Acemi Koğuşu")
    public int level = 1;       // Şu anki seviye
    public int baseCost = 500;  // İlk seviye masrafı
    public int baseValue = 5;   // Seviye 1'deki etkisi (Örn: 5 Asker Kapasitesi)
    public int valuePerLevel = 2; // Her seviyede ne kadar artacak?

    // Dinamik Masraf Hesabı (Her seviyede fiyat artar)
    public int GetUpgradeCost()
    {
        return baseCost * level; // Örn: 500 -> 1000 -> 1500
    }

    // Dinamik Etki Hesabı (Kapasite kaç?)
    public int GetCurrentValue()
    {
        return baseValue + ((level - 1) * valuePerLevel);
    }
}

public class CampManager : MonoBehaviour
{
    public static CampManager Instance;

    [Header("Binalar")]
    public List<Building> buildings; // Editörden dolduracağız

    void Awake()
    {
        Instance = this;
    }

    // Belirli bir binayı bulmak için
    public Building GetBuilding(string id)
    {
        return buildings.FirstOrDefault(b => b.id == id);
    }

    // --- KRİTİK FONKSİYON: Kapasiteyi Buradan Soracağız ---
    public int GetMaxSoldierCapacity()
    {
        var kogus = GetBuilding("kogus");
        if (kogus != null) return kogus.GetCurrentValue();
        return 5; // Hata olursa varsayılan 5
    }

    public void UpgradeBuilding(string id)
    {
        Building b = GetBuilding(id);
        if (b == null) return;

        int cost = b.GetUpgradeCost();

        if (MoneyManager.Instance.gold >= cost)
        {
            MoneyManager.Instance.Spend(cost);
            b.level++;
            Debug.Log($"{b.displayName} seviye atladı! Yeni Seviye: {b.level}");
            
            // Eğer UI varsa yenilemek için event fırlatılabilir
            // OnBuildingUpgraded?.Invoke();
        }
        else
        {
            Debug.Log("Yetersiz Akçe!");
        }
    }
}