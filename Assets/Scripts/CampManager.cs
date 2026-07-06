using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class Building
{
    public string id;
    public string displayName;
    public int level    = 1;
    public int maxLevel = 3;
    public int baseCost = 200;
    public int baseValue = 3;
    public int increasePerLevel = 2;

    [Header("Görsel Objeler")]
    public GameObject[] stageVisuals;

    public int GetCost()      => baseCost * level;
    public int GetValue()     => baseValue + ((level - 1) * increasePerLevel);
    public int GetNextValue() => GetValue() + increasePerLevel;

    public void UpdateVisuals()
    {
        if (stageVisuals == null || stageVisuals.Length == 0) return;
        int activeIndex = Mathf.Clamp(level - 1, 0, stageVisuals.Length - 1);
        for (int i = 0; i < stageVisuals.Length; i++)
            if (stageVisuals[i] != null)
                stageVisuals[i].SetActive(i == activeIndex);
    }
}

public class CampManager : MonoBehaviour
{
    public static CampManager Instance;
    public event Action OnCampUpdated;

    public List<Building> buildings = new List<Building>();

    [Header("Bina Görselleri")]
    public GameObject[] kogusVisuals;
    public GameObject[] demirciVisuals;

    // ── ASKER CACHE — FindObjectsOfType yerine bu kullanılacak ──────────
    private readonly List<Gladiator> _soldiers = new List<Gladiator>();
    /////xd
    [Header("Sefer Verileri (Sahneler Arası Taşınır)")]
    public List<JanissaryData> activeMissionArmy = new List<JanissaryData>();

    /// <summary>Kayıtlı asker listesini döner (ölüler dahil).</summary>
    public IReadOnlyList<Gladiator> AllSoldiers => _soldiers;

    /// <summary>Sadece sağ ve "benim" askerleri döner.</summary>
    public List<Gladiator> GetLivingSoldiers()
    {
        _soldiers.RemoveAll(s => s == null);
        return _soldiers.FindAll(s =>
        {
            var ai = s.GetComponent<GladiatorAI>();
            return s.CompareTag("MySoldier") && (ai == null || !ai.isDead);
        });
    }

    /// <summary>Yeni asker sahneye eklenince çağır.</summary>
    public void RegisterSoldier(Gladiator g)
    {
        if (g != null && !_soldiers.Contains(g))
        {
            _soldiers.Add(g);
            OnCampUpdated?.Invoke();
        }
    }

    /// <summary>Asker ölünce veya ordudan atılınca çağır.</summary>
    public void UnregisterSoldier(Gladiator g)
    {
        if (_soldiers.Remove(g))
            OnCampUpdated?.Invoke();
    }

    // ── Bina Yönetimi ────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
         DontDestroyOnLoad(gameObject);
        InitializeBuildings();
    }

    void InitializeBuildings()
    {
        AddBuilding("kogus",     "Acemi Koğuşu",        250, 3, 2, 3, kogusVisuals);
        AddBuilding("demirci",   "Demirci Atölyesi",    200, 5, 2, 3, demirciVisuals);
        AddBuilding("talimhane", "Enderun Talimhanesi", 300, 15, 10, 3, null);

        foreach (var b in buildings) b.UpdateVisuals();
    }

    void AddBuilding(string id, string displayName, int baseCost, int baseValue,
                     int increasePerLevel, int maxLevel, GameObject[] visuals)
    {
        if (buildings.Any(b => b.id == id)) return;
        buildings.Add(new Building
        {
            id = id, displayName = displayName,
            baseCost = baseCost, baseValue = baseValue,
            increasePerLevel = increasePerLevel, maxLevel = maxLevel,
            stageVisuals = visuals
        });
    }

    public int GetBuildingValue(string buildingId)
    {
        var b = buildings.FirstOrDefault(x => x.id == buildingId);
        return b != null ? b.GetValue() : 3;
    }

    public int GetMaxSoldierCapacity() => GetBuildingValue("kogus");

    public void UpgradeBuilding(string id)
    {
        var b = buildings.FirstOrDefault(x => x.id == id);
        if (b == null) return;

        if (b.level >= b.maxLevel)
        {
            NotificationManager.Instance?.Show($"{b.displayName} maksimum seviyede!", NotificationType.Warning);
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
            NotificationManager.Instance?.Show($"{b.displayName} Geliştirildi! Yeni Seviye: {b.level}", NotificationType.Success);
        }
        else
        {
            AudioManager.Instance?.PlayError();
            NotificationManager.Instance?.Show("Yetersiz Bakiye!", NotificationType.Error);
        }
    }

    public void RefreshUI() => OnCampUpdated?.Invoke();
}
