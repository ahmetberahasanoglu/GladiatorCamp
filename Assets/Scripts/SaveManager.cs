using UnityEngine;
using System.IO; // Dosya işlemleri için şart

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

        // 1. GLOBAL VERİLERİ TOPLA
        data.savedGold = MoneyManager.Instance.gold;
        data.savedFood = SupplyManager.Instance.currentFood;
        if (DayManager.Instance != null) data.savedDay = DayManager.Instance.currentDay;

        // 2. ASKERLERİ TOPLA
        Gladiator[] allSoldiers = FindObjectsOfType<Gladiator>();
        foreach (var soldier in allSoldiers)
        {
            SoldierSaveData sData = new SoldierSaveData();
            sData.name = soldier.data.gladiatorName;
            sData.strength = soldier.data.strength;
            sData.stamina = soldier.data.stamina;
            sData.defense = soldier.data.defense;
            sData.speed = soldier.data.speed;
            sData.morale = soldier.data.morale;
            sData.level = soldier.data.level;
            
            data.soldiers.Add(sData);
        }

        // 3. BİNALARI TOPLA
        if (CampManager.Instance != null)
        {
            foreach (var b in CampManager.Instance.buildings)
            {
                BuildingSaveData bData = new BuildingSaveData();
                bData.buildingID = b.id;
                bData.level = b.level;
                data.buildings.Add(bData);
            }
        }

        // 4. JSON'A ÇEVİR VE YAZ
        string json = JsonUtility.ToJson(data, true); // true = okunaklı format
        File.WriteAllText(savePath, json);

        Debug.Log("Oyun Kaydedildi! Yol: " + savePath);
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("Kayıt dosyası bulunamadı.");
            return;
        }

        // 1. DOSYAYI OKU
        string json = File.ReadAllText(savePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        // 2. GLOBAL VERİLERİ YÜKLE
        MoneyManager.Instance.gold = 0; // Önce sıfırla, sonra ekle (Eventleri tetiklemek için)
        MoneyManager.Instance.Add(data.savedGold);
        
        SupplyManager.Instance.currentFood = data.savedFood;
        // SupplyManager UI yenilemesini tetikleyebilirsin
        
        if (DayManager.Instance != null) DayManager.Instance.currentDay = data.savedDay;

        // 3. ASKERLERİ YÜKLE (En Zor Kısım)
        // Önce sahnedeki mevcut askerleri temizlemeliyiz (Duplicate olmasın)
        foreach(var g in FindObjectsOfType<Gladiator>()) Destroy(g.gameObject);

        // Kayıttaki her asker için yeni bir obje yarat
        foreach (var sData in data.soldiers)
        {
            // RecruitManager'daki prefabı ve spawn pointi kullanabiliriz
            // Ama burası için RecruitManager'a "LoadSoldier" diye bir fonksiyon yazmak daha temiz olur.
            RecruitManager.Instance.LoadSoldierFromSave(sData);
        }

        // 4. BİNALARI YÜKLE
        if (CampManager.Instance != null)
        {
            foreach (var bData in data.buildings)
            {
                // İlgili binayı bul ve seviyesini eşitle
                var building = CampManager.Instance.buildings.Find(x => x.id == bData.buildingID);
                if (building != null)
                {
                    building.level = bData.level;
                }
            }
        }

        Debug.Log("Oyun Yüklendi!");
    }
}