using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public int savedGold;
    public int savedDay;
    public int savedFood;

    // Yeni eklenenler
    public int savedReputation;
    public int savedMorale;
    public int savedNasip;

    // Askerlerin Listesi
    public List<SoldierSaveData> soldiers = new List<SoldierSaveData>();

    // Binaların Listesi
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
}

[System.Serializable]
public class SoldierSaveData
{
    public string name;
    public int strength;
    public int stamina;
    public int defense;
    public int speed;
    public int morale;
    public int level;
    public string weaponID;
    public string armorID;
    public string helmetID;
    public string shieldID;

    // Yeni eklenenler
    public bool   isGazi;
    public int    trait;           // SoldierTrait enum → int
    public float  currentHealth;
    public float  maxHealth;
    public int    dailyWage;
    public int    currentActivity; // SoldierActivity enum → int (her zaman Idling yüklenir)
}

// Bina verisi
[System.Serializable]
public class BuildingSaveData
{
    public string buildingID;                  // buildingName ile eşleşir
    public int    level;
    // Yeni eklenenler
    public int    state;                       // BuildingState enum → int
    public int    currentRemainingEncounters;
}