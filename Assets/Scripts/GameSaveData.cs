using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public int savedGold;
    public int savedDay;
    public int savedFood;
    public int savedReputation;
    public int savedMorale;
    public int savedNasip;

    // ── Sefer Çantası ────────────────────────────────────────────────────
    // Oyun seferdeyken kapanırsa bu veriler korunur
    public bool   savedIsExpeditionActive   = false;
    public int    savedExpeditionGold       = 0;
    public int    savedExpeditionRep        = 0;
    public int    savedEncounterCount       = 0;
    // Item'lar itemID listesi olarak kaydedilir
    public List<string> savedExpeditionItemIDs = new List<string>();
    // ─────────────────────────────────────────────────────────────────────

    public List<SoldierSaveData>   soldiers  = new List<SoldierSaveData>();
    public List<BuildingSaveData>  buildings = new List<BuildingSaveData>();
}

[System.Serializable]
public class SoldierSaveData
{
    public string name;
    public int    strength;
    public int    stamina;
    public int    defense;
    public int    speed;
    public int    morale;
    public int    level;
    public string weaponID;
    public string armorID;
    public string helmetID;
    public string shieldID;

    public bool   isGazi;
    public int    trait;
    public float  currentHealth;
    public float  maxHealth;
    public int    dailyWage;
    public int    currentActivity;
}

[System.Serializable]
public class BuildingSaveData
{
    public string buildingID;
    public int    level;
    public int    state;
    public int    currentRemainingEncounters;
}
