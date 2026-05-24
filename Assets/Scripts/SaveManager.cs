using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement; // Dosya işlemleri için şart

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string saveFileName = "janissary_save.json";
    private string savePath;

    void Awake()
    {
        Instance = this;
        // Dosyanın kaydedileceği yol (PC, Android, iOS için otomatik ayarlanır)
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
    }

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();

        // Global veriler
        data.savedGold = MoneyManager.Instance.gold;
        data.savedFood = SupplyManager.Instance.currentFood;
        if (DayManager.Instance        != null) data.savedDay        = DayManager.Instance.currentDay;
        if (ReputationManager.Instance != null) data.savedReputation = ReputationManager.Instance.GetReputation();
        if (CampMoraleManager.Instance != null) data.savedMorale     = CampMoraleManager.Instance.currentMorale;
        if (NasipManager.Instance      != null) data.savedNasip      = NasipManager.Instance.currentNasip;

        // Askerler
        Gladiator[] allSoldiers = FindObjectsOfType<Gladiator>();
        foreach (var soldier in allSoldiers)
        {
            if (soldier.data == null) continue;

            SoldierSaveData sData = new SoldierSaveData();

            // Temel statlar
            sData.name     = soldier.data.gladiatorName;
            sData.strength = soldier.data.strength;
            sData.stamina  = soldier.data.stamina;
            sData.defense  = soldier.data.defense;
            sData.speed    = soldier.data.speed;
            sData.level    = soldier.data.level;

            // Yeni alanlar
            sData.isGazi          = soldier.data.isGazi;
            sData.trait           = (int)soldier.data.trait;
            sData.currentHealth   = soldier.data.currentHealth;
            sData.maxHealth       = soldier.data.maxHealth;
            sData.dailyWage       = soldier.data.dailyWage;
            sData.currentActivity = (int)soldier.data.currentActivity;

            // Ekipman
            var inventory = soldier.GetComponent<GladiatorInventory>();
            if (inventory != null)
            {
                sData.weaponID  = inventory.weapon != null ? inventory.weapon.itemID : "";
                sData.armorID   = inventory.armor  != null ? inventory.armor.itemID  : "";
                sData.helmetID  = inventory.helmet != null ? inventory.helmet.itemID : "";
                sData.shieldID  = inventory.shield != null ? inventory.shield.itemID : "";
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
                level                      = 1, // Bina level sistemi eklenince güncellenir
            });
        }

        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(savePath, json);
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show("Oyun kaydedildi.", NotificationType.Info);
        Debug.Log($"[Save] Kaydedildi → {savePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("Kayıt dosyası bulunamadı.");
            return;
        }

        string json       = File.ReadAllText(savePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        if (data == null) { Debug.LogError("[Save] JSON parse hatası."); return; }

        // Global veriler
        MoneyManager.Instance.gold = 0;
        MoneyManager.Instance.Add(data.savedGold);

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

        // Mevcut askerleri sil
        foreach (var g in FindObjectsOfType<Gladiator>())
            DestroyImmediate(g.gameObject);

        // Askerleri yükle — RecruitManager.LoadSoldierFromSave isGazi, trait ve health almıyor
        // bu yüzden yüklendikten sonra veriyi düzeltiyoruz
        foreach (var sData in data.soldiers)
        {
            RecruitManager.Instance.LoadSoldierFromSave(sData);

            // LoadSoldierFromSave spawn ettikten sonra son eklenen Gladiator'ı bul ve ekstra alanları yaz
            var allG = FindObjectsOfType<Gladiator>();
            foreach (var g in allG)
            {
                if (g.data == null || g.data.gladiatorName != sData.name) continue;
                g.data.isGazi        = sData.isGazi;
                g.data.trait         = (SoldierTrait)sData.trait;
                g.data.maxHealth     = sData.maxHealth > 0 ? sData.maxHealth : g.data.maxHealth;
                g.data.currentHealth = sData.currentHealth > 0 ? sData.currentHealth : g.data.maxHealth;
                g.data.dailyWage     = sData.dailyWage > 0 ? sData.dailyWage : g.data.dailyWage;
                // Activity: hep Idling başlasın (keşif/eğitim durumu çok karmaşık restore eder)
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
                b.currentState                = (BuildingState)bData.state;
                b.currentRemainingEncounters  = bData.currentRemainingEncounters;
                b.UpdateVisuals();
                break;
            }
        }

        // Eski CampManager bina level sistemi (geriye dönük uyum)
        if (CampManager.Instance != null)
        {
            foreach (var bData in data.buildings)
            {
                var building = CampManager.Instance.buildings.Find(x => x.id == bData.buildingID);
                if (building != null) building.level = bData.level;
            }
        }

        Debug.Log("[Save] Yüklendi!");
        Debug.Log(savePath);

        var topBar = FindObjectOfType<TopInfoBarUI>();
        if (topBar != null) topBar.ForceUpdateAll();
    }
    public void StartGame()
    {
        SceneManager.LoadScene("CampScene");
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("[Save] Kayıt silindi.");
        }
    }

    public bool HasSave() => File.Exists(savePath);

    // Gün geçince otomatik kaydet
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

    void OnApplicationQuit()  => SaveGame();
    void OnApplicationPause(bool p) { if (p) SaveGame(); } // Mobil
}