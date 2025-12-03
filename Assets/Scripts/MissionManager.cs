using UnityEngine;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("Ayarlar")]
    public float secondsPerDay = 2.0f;
    public Transform activeMissionsParent;
    public ActiveMissionUI activeMissionPrefab;

    [Header("Sonuç Ekranı")]
    public MissionResultUI resultPopupPrefab; // Az önce yazdığımız scriptin olduğu prefab
    public Transform canvasParent;            // Popup'ın yaratılacağı yer (Genelde Main Canvas)

    private List<OngoingMission> currentMissions = new List<OngoingMission>();

    void Awake() { Instance = this; }

    void Update()
    {
        for (int i = currentMissions.Count - 1; i >= 0; i--)
        {
            var mission = currentMissions[i];
            mission.timer += Time.deltaTime;

            if (mission.uiReference != null)
                mission.uiReference.UpdateProgress(Time.deltaTime);

            if (mission.timer >= mission.totalDuration)
                CompleteMission(mission);
        }
    }

    // DİKKAT: Artık GladiatorData değil, Gladiator (Component) alıyor
    public void StartMission(MissionData data, List<Gladiator> squad)
    {
        OngoingMission newMission = new OngoingMission();
        newMission.originalData = data;
        newMission.squadComponents = new List<Gladiator>(squad); // Objeleri kaydet
        newMission.totalDuration = data.durationDays * secondsPerDay;
        newMission.timer = 0;

        // --- ASKERLERİ KİLİTLE ---
        foreach (var soldier in squad)
        {
            soldier.isOnMission = true;
            // İstersen burada asker objesini görünmez yapabilirsin:
            // soldier.gameObject.SetActive(false); 
        }
        // -------------------------

        var uiObj = Instantiate(activeMissionPrefab, activeMissionsParent);
        uiObj.Setup(data.missionName, newMission.totalDuration);
        newMission.uiReference = uiObj;

        currentMissions.Add(newMission);
        Debug.Log($"Ordu yola çıktı! Hedef: {data.missionName}");
    }

    void CompleteMission(OngoingMission mission)
    {
        Destroy(mission.uiReference.gameObject);
        currentMissions.Remove(mission);

        // --- ASKERLERİ SERBEST BIRAK ---
        foreach (var soldier in mission.squadComponents)
        {
            if (soldier != null) // Obje silinmiş olabilir, kontrol et
            {
                soldier.isOnMission = false;
                // soldier.gameObject.SetActive(true); // Görünmez yaptıysan geri aç
            }
        }
        // -------------------------------

        // Sonuç hesaplama (Aynı mantık, sadece veriye ulaşmak için .data diyoruz)
        int totalPower = 0;
        foreach (var s in mission.squadComponents) totalPower += s.data.GetTotalStats();

        // Basit Savaş Mantığı
        int roll = Random.Range(0, 50);
        bool success = (totalPower + roll) >= mission.originalData.difficulty;

       MissionResultUI popup = Instantiate(resultPopupPrefab, canvasParent);
        
        if (success)
        {
            // Para ver
            MoneyManager.Instance.Add(mission.originalData.goldReward);
            // Popup'ı kur (Zafer)
            popup.Setup(true, mission.originalData);
        }
        else
        {
            // Popup'ı kur (Yenilgi)
            popup.Setup(false, mission.originalData);
        }
    }

    [System.Serializable]
    public class OngoingMission
    {
        public MissionData originalData;
        public List<Gladiator> squadComponents; // ARTIK OBJE TUTUYORUZ
        public ActiveMissionUI uiReference;
        public float totalDuration;
        public float timer;
    }
}