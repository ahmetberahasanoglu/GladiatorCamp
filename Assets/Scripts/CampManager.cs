using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]

public class Building
{
    public string id;           
    public string displayName;  
    public int level = 1;       
    public int baseCost = 200;  
    public int baseValue = 3;   
    public int increasePerLevel = 2; 

    // --- YENİ: GÖRSEL SEVİYELERİ TUTACAK DİZİ ---
    [Header("Görsel Objeler")]
    public GameObject[] stageVisuals; // Level 1, Level 2, Level 3 vb. görselleri

    public int GetCost() 
    {
        return baseCost * level; 
    }

    public int GetValue()
    {
        return baseValue + ((level - 1) * increasePerLevel); 
    }

    public int GetNextValue()
    {
        return GetValue() + increasePerLevel;
    }

    // --- YENİ: GÖRSELLERİ GÜNCELLEYEN FONKSİYON ---
    public void UpdateVisuals()
    {
        // Eğer görsel atanmamışsa hata vermemesi için güvenlik kontrolü
        if (stageVisuals == null || stageVisuals.Length == 0) return;

        for (int i = 0; i < stageVisuals.Length; i++)
        {
            // Eğer objenin sırası (i), binanın mevcut seviyesinin (level-1) indeksine eşitse objeyi aç, değilse kapat.
            // Örn: Seviye 1'deyken 0. index açılır, diğerleri kapanır. Seviye 2'deyken 1. index açılır.
            if (stageVisuals[i] != null)
            {
                // Son seviyeye ulaşıldığında index dışına çıkmamak için ufak bir sınırlandırma
                int activeIndex = Mathf.Clamp(level - 1, 0, stageVisuals.Length - 1);
                stageVisuals[i].SetActive(i == activeIndex);
            }
        }
    }
}

public class CampManager : MonoBehaviour
{
    public static CampManager Instance;
    public event Action OnCampUpdated;
    
    public List<Building> buildings = new List<Building>();

    // --- YENİ EKLENEN KISIM: Yöneticinin elinde tutacağı modeller ---
    [Header("Bina Görselleri (Sırayla Lvl1, Lvl2, Lvl3...)")]
    public GameObject[] kogusVisuals; 
    // İleride Talimhane eklediğinde buraya public GameObject[] talimhaneVisuals; yazabilirsin.

    void Awake()
    {
        Instance = this;
        InitializeBuildings(); 
    }

    void InitializeBuildings()
    {
        // Eğer listede Koğuş yoksa, otomatik ekle
        if (!buildings.Any(b => b.id == "kogus"))
        {
            Building kogus = new Building();
            kogus.id = "kogus";
            kogus.displayName = "Acemi Koğuşu";
            kogus.baseCost = 250;  
            kogus.baseValue = 3;   
            kogus.increasePerLevel = 2; 

            // YENİ: Editörden atadığımız modelleri, kodla yaratılan binaya teslim ediyoruz
            kogus.stageVisuals = kogusVisuals;

            buildings.Add(kogus);
        }
        
        // YENİ: Oyun başladığında herkesin görselini mevcut seviyesine göre ayarla (Lvl 1 çadırı açılsın)
        foreach (var building in buildings)
        {
            building.UpdateVisuals();
        }
    }

    public int GetMaxSoldierCapacity()
    {
        var b = buildings.FirstOrDefault(x => x.id == "kogus");
        if (b != null) return b.GetValue();
        return 3; 
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
            
            // YENİ: Seviye atlayınca görselleri güncelle (Örn: Lvl 1 kapansın, Lvl 2 açılsın)
            b.UpdateVisuals(); 

            OnCampUpdated?.Invoke();
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show($"{b.displayName} seviye atladı! Yeni Seviye: {b.level}, Kapasite: {b.GetValue()}", NotificationType.Success);
        }
        else
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Yetersiz Bakiye!", NotificationType.Error);
        }
    }

    public void RefreshUI()
    {
        OnCampUpdated?.Invoke();
    }
}