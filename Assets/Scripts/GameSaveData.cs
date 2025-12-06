using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // Global Veriler
    public int savedGold;
    public int savedDay;
    public int savedFood;

    // Askerlerin Listesi
    public List<SoldierSaveData> soldiers = new List<SoldierSaveData>();

    // Binaların Listesi
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
}

// Askerin tekil verisi (GladiatorData ScriptableObject'ini direkt kaydedemeyiz, değerleri tutuyoruz)
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
    // İleride buraya: public string weaponID; gibi şeyler eklenecek
}

// Bina verisi
[System.Serializable]
public class BuildingSaveData
{
    public string buildingID; // "kogus"
    public int level;
}