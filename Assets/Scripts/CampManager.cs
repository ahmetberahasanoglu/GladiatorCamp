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
    public int maxLevel = 3;    
    public int baseCost = 200;  
    public int baseValue = 3;   
    public int increasePerLevel = 2; 

    [Header("Görsel Objeler")]
    public GameObject[] stageVisuals; 

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

    public void UpdateVisuals()
    {
        if (stageVisuals == null || stageVisuals.Length == 0) return;

        for (int i = 0; i < stageVisuals.Length; i++)
        {
            if (stageVisuals[i] != null)
            {
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

    [Header("Bina Görselleri (Sırayla Lvl1, Lvl2, Lvl3...)")]
    public GameObject[] kogusVisuals; 
    public GameObject[] demirciVisuals;

    void Awake()
    {
        Instance = this;
        InitializeBuildings(); 
    }

    void InitializeBuildings()
    {
        // 1. KOĞUŞU EKLE
        if (!buildings.Any(b => b.id == "kogus"))
        {
            Building kogus = new Building {
                id = "kogus", displayName = "Acemi Koğuşu",
                baseCost = 250, baseValue = 3, increasePerLevel = 2, maxLevel = 3,
                stageVisuals = kogusVisuals
            };
            buildings.Add(kogus);
        }
        
        if (!buildings.Any(b => b.id == "demirci"))
        {
            Building demirci = new Building {
                id = "demirci", displayName = "Demirci Atölyesi",
                baseCost = 200, baseValue = 3, increasePerLevel = 2, maxLevel = 3, 
                stageVisuals = demirciVisuals
            };
            buildings.Add(demirci);
        }

        if (!buildings.Any(b => b.id == "talimhane"))
        {
            Building talimhane = new Building {
                id = "talimhane", 
                displayName = "Enderun Talimhanesi",
                baseCost = 300, 
                baseValue = 15, 
                increasePerLevel = 10, 
                maxLevel = 3
               
            };
            buildings.Add(talimhane);
        }

        foreach (var building in buildings)
        {
            building.UpdateVisuals();
        }
    }

    public int GetBuildingValue(string buildingId)
    {
        var b = buildings.FirstOrDefault(x => x.id == buildingId);
        return b != null ? b.GetValue() : 3; 
    }

    public int GetMaxSoldierCapacity()
    {
        return GetBuildingValue("kogus"); 
    }

    public void UpgradeBuilding(string id)
    {
        var b = buildings.FirstOrDefault(x => x.id == id);
        if (b == null) return;

        if (b.level >= b.maxLevel)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show($"{b.displayName} maksimum seviyede!", NotificationType.Warning);
            return;
        }

        int cost = b.GetCost();

        if (MoneyManager.Instance.gold >= cost)
        {
            MoneyManager.Instance.Spend(cost);
            b.level++;
            
            b.UpdateVisuals(); 
            OnCampUpdated?.Invoke();
            AudioManager.Instance.PlayUpgrade();
            
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show($"{b.displayName} Geliştirildi! Yeni Seviye: {b.level}", NotificationType.Success);
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Yetersiz Bakiye!", NotificationType.Error);
        }
    }

    public void RefreshUI()
    {
        OnCampUpdated?.Invoke();
    }
}