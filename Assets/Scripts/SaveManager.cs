using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string saveFileName = "janissary_save.json";
    private string savePath;

    void Awake()
    {
        Instance = this;
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
    }

    // ── OTOMATİK KAYIT ───────────────────────────────────────────────────
    void OnEnable()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay += SaveGame;
    }

    void OnDisable()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay -= SaveGame;
    }

    void OnApplicationQuit()               => SaveGame();
    void OnApplicationPause(bool pausing)  { if (pausing) SaveGame(); }

    // ════════════════════════════════════════════════════════════════════
    //  KAYDET
    // ════════════════════════════════════════════════════════════════════
    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();

        // Global veriler
        data.savedGold       = MoneyManager.Instance != null ? MoneyManager.Instance.gold : 0;
        data.savedFood       = SupplyManager.Instance != null ? SupplyManager.Instance.currentFood : 0;
        if (DayManager.Instance        != null) data.savedDay        = DayManager.Instance.currentDay;
        if (ReputationManager.Instance != null) data.savedReputation = ReputationManager.Instance.GetReputation();
        if (CampMoraleManager.Instance != null) data.savedMorale     = CampMoraleManager.Instance.currentMorale;
        if (NasipManager.Instance      != null) data.savedNasip      = NasipManager.Instance.currentNasip;

        // ── SEFer ÇANTASI ─────────────────────────────────────────────
        if (ExpeditionManager.Instance != null)
        {
            data.savedIsExpeditionActive = ExpeditionManager.Instance.isExpeditionActive;
            data.savedExpeditionGold     = ExpeditionManager.Instance.tempGold;
            data.savedExpeditionRep      = ExpeditionManager.Instance.tempReputation;
            data.savedEncounterCount     = ExpeditionManager.Instance.currentEncounterCount;

            data.savedExpeditionItemIDs.Clear();
            foreach (var item in ExpeditionManager.Instance.tempItems)
                if (item != null && !string.IsNullOrEmpty(item.itemID))
                    data.savedExpeditionItemIDs.Add(item.itemID);
        }
        // ─────────────────────────────────────────────────────────────

        // Askerler
        Gladiator[] allSoldiers = FindObjectsOfType<Gladiator>();
        foreach (var soldier in allSoldiers)
        {
            if (soldier.data == null) continue;

            SoldierSaveData sData = new SoldierSaveData
            {
                name            = soldier.data.gladiatorName,
                strength        = soldier.data.strength,
                stamina         = soldier.data.stamina,
                defense         = soldier.data.defense,
                speed           = soldier.data.speed,
                level           = soldier.data.level,
                isGazi          = soldier.data.isGazi,
                trait           = (int)soldier.data.trait,
                currentHealth   = soldier.data.currentHealth,
                maxHealth       = soldier.data.maxHealth,
                dailyWage       = soldier.data.dailyWage,
                currentActivity = (int)soldier.data.currentActivity
            };

            var inventory = soldier.GetComponent<GladiatorInventory>();
            if (inventory != null)
            {
                sData.weaponID = inventory.weapon != null ? inventory.weapon.itemID : "";
                sData.armorID  = inventory.armor  != null ? inventory.armor.itemID  : "";
                sData.helmetID = inventory.helmet != null ? inventory.helmet.itemID : "";
                sData.shieldID = inventory.shield != null ? inventory.shield.itemID : "";
            }

            data.soldiers.Add(sData);
        }

        // Binalar
        BuildingClickable[] allBuildings = FindObjectsOfType<BuildingClickable>();
        foreach (var b in allBuildings)
        {
            data.buildings.Add(new BuildingSaveData
            {
                buildingID                 = b.buildingName,
                state                      = (int)b.currentState,
                currentRemainingEncounters = b.currentRemainingEncounters,
                level                      = 1,
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show("Oyun kaydedildi.", NotificationType.Info);

        Debug.Log($"[Save] Kaydedildi → {savePath}");
    }

    // ════════════════════════════════════════════════════════════════════
    //  YÜKLE
    // ════════════════════════════════════════════════════════════════════
    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("[Save] Kayıt dosyası bulunamadı.");
            return;
        }

        string json       = File.ReadAllText(savePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        if (data == null) { Debug.LogError("[Save] JSON parse hatası."); return; }

        // Global veriler
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.gold = 0;
            MoneyManager.Instance.Add(data.savedGold);
        }

        if (SupplyManager.Instance != null)
            SupplyManager.Instance.currentFood = data.savedFood;

        if (DayManager.Instance != null)
        {
            DayManager.Instance.currentDay = data.savedDay;
            DayManager.Instance.RefreshUI();
        }

        if (ReputationManager.Instance != null)
        {
            int diff = data.savedReputation - ReputationManager.Instance.GetReputation();
            ReputationManager.Instance.ChangeReputation(diff);
        }

        if (CampMoraleManager.Instance != null)
        {
            int diff = data.savedMorale - CampMoraleManager.Instance.currentMorale;
            CampMoraleManager.Instance.ChangeMorale(diff);
        }

        if (NasipManager.Instance != null)
        {
            NasipManager.Instance.SpendNasip(NasipManager.Instance.currentNasip);
            if (data.savedNasip > 0) NasipManager.Instance.AddNasip(data.savedNasip);
        }

        // ── SEFer ÇANTASI RESTORE ─────────────────────────────────────
        if (ExpeditionManager.Instance != null && data.savedIsExpeditionActive)
        {
            ExpeditionManager.Instance.isExpeditionActive  = true;
            ExpeditionManager.Instance.tempGold             = data.savedExpeditionGold;
            ExpeditionManager.Instance.tempReputation       = data.savedExpeditionRep;
            ExpeditionManager.Instance.currentEncounterCount = data.savedEncounterCount;

            ExpeditionManager.Instance.tempItems.Clear();
            if (ItemDatabase.Instance != null)
            {
                foreach (var itemID in data.savedExpeditionItemIDs)
                {
                    var item = ItemDatabase.Instance.GetItemByID(itemID);
                    if (item != null) ExpeditionManager.Instance.tempItems.Add(item);
                }
            }

            Debug.Log($"[Save] Sefer çantası yüklendi: {data.savedExpeditionGold} Akçe, " +
                      $"{data.savedExpeditionRep} İtibar, {data.savedEncounterCount} Adım");
        }
        else if (ExpeditionManager.Instance != null)
        {
            // Seferde değilken kaydedilmişse çantayı temizle
            ExpeditionManager.Instance.isExpeditionActive = false;
            ExpeditionManager.Instance.tempGold           = 0;
            ExpeditionManager.Instance.tempReputation     = 0;
            ExpeditionManager.Instance.tempItems.Clear();
            ExpeditionManager.Instance.currentEncounterCount = 0;
        }
        // ─────────────────────────────────────────────────────────────

        // Askerleri yükle
        foreach (var g in FindObjectsOfType<Gladiator>())
            DestroyImmediate(g.gameObject);

        foreach (var sData in data.soldiers)
        {
            RecruitManager.Instance.LoadSoldierFromSave(sData);

            var allG = FindObjectsOfType<Gladiator>();
            foreach (var g in allG)
            {
                if (g.data == null || g.data.gladiatorName != sData.name) continue;
                g.data.isGazi          = sData.isGazi;
                g.data.trait           = (SoldierTrait)sData.trait;
                g.data.maxHealth       = sData.maxHealth > 0 ? sData.maxHealth : g.data.maxHealth;
                g.data.currentHealth   = sData.currentHealth > 0 ? sData.currentHealth : g.data.maxHealth;
                g.data.dailyWage       = sData.dailyWage > 0 ? sData.dailyWage : g.data.dailyWage;
                g.data.currentActivity = SoldierActivity.Idling;
                break;
            }
        }

        // Binaları yükle
        BuildingClickable[] allBuildings = FindObjectsOfType<BuildingClickable>();
        foreach (var bData in data.buildings)
        {
            foreach (var b in allBuildings)
            {
                if (b.buildingName != bData.buildingID) continue;
                b.currentState               = (BuildingState)bData.state;
                b.currentRemainingEncounters = bData.currentRemainingEncounters;
                b.UpdateVisuals();
                break;
            }
        }

        if (CampManager.Instance != null)
        {
            foreach (var bData in data.buildings)
            {
                var building = CampManager.Instance.buildings.Find(x => x.id == bData.buildingID);
                if (building != null) building.level = bData.level;
            }
        }

        var topBar = FindObjectOfType<TopInfoBarUI>();
        if (topBar != null) topBar.ForceUpdateAll();

        Debug.Log("[Save] Yüklendi!");
    }

    public void StartGame() => SceneManager.LoadScene("CampScene");

    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("[Save] Kayıt silindi.");
        }
    }

    public bool HasSave() => File.Exists(savePath);
}
