using UnityEngine;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("UI Referansları")]
    public Transform activeMissionsParent;
    public ActiveMissionUI activeMissionPrefab;
    
    [Header("Sonuç Ekranı")]
    public MissionResultUI resultPopupPrefab;
    public Transform canvasParent;

    private List<OngoingMission> currentMissions = new List<OngoingMission>();

    void Awake()
    {
        Instance = this;
    }

    // Event Aboneliği: Oyun açılınca abone ol, kapanınca çık
    void Start()
    {
        // DayManager'ın var olduğundan emin olalım
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay += OnNewDayArrived;
        }
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay -= OnNewDayArrived;
        }
    }

    // --- KRİTİK DEĞİŞİKLİK: ARTIK UPDATE YOK ---
    // Bu fonksiyon sadece DayManager "NextDay" dediğinde çalışacak
    void OnNewDayArrived()
    {
        // Tersten döngü kuruyoruz ki listeden eleman silersek hata vermesin
        for (int i = currentMissions.Count - 1; i >= 0; i--)
        {
            var mission = currentMissions[i];
            
            // 1. Günü azalt
            mission.daysRemaining--;

            // 2. UI'ı güncelle
            if (mission.uiReference != null)
            {
                mission.uiReference.UpdateVisuals(mission.daysRemaining, mission.totalDuration);
            }

            // 3. Gün bitti mi?
            if (mission.daysRemaining <= 0)
            {
                CompleteMission(mission);
            }
        }
    }

    public void StartMission(MissionData data, List<Gladiator> squad)
    {
        OngoingMission newMission = new OngoingMission();
        newMission.originalData = data;
        newMission.squadComponents = new List<Gladiator>(squad);
        
        // Saniye hesabı yerine direkt gün sayısını alıyoruz
        newMission.totalDuration = data.durationDays; 
        newMission.daysRemaining = data.durationDays;

        // Askerleri Kilitle
        foreach (var soldier in squad) soldier.isOnMission = true;

        // UI Oluştur
        var uiObj = Instantiate(activeMissionPrefab, activeMissionsParent);
        // İlk kurulumda (Kalan Gün, Toplam Gün) gönderiyoruz
        uiObj.Setup(data.missionName, newMission.totalDuration);
        
        newMission.uiReference = uiObj;
        currentMissions.Add(newMission);

        Debug.Log($"{data.missionName} başladı. Süre: {data.durationDays} Gün.");
    }

    void CompleteMission(OngoingMission mission)
    {
        Destroy(mission.uiReference.gameObject);
        currentMissions.Remove(mission);

        // Asker kilidini aç
        foreach (var soldier in mission.squadComponents)
        {
            if (soldier != null) soldier.isOnMission = false;
        }

        // --- SAVAŞ SONUCU (Aynı mantık) ---
        int totalPower = 0;
        foreach (var s in mission.squadComponents) totalPower += s.data.GetTotalStats();

        int roll = Random.Range(0, 50);
        bool success = (totalPower + roll) >= mission.originalData.difficulty;

        MissionResultUI popup = Instantiate(resultPopupPrefab, canvasParent);
        popup.Setup(success, mission.originalData);

        if (success) MoneyManager.Instance.Add(mission.originalData.goldReward);
    }

    [System.Serializable]
    public class OngoingMission
    {
        public MissionData originalData;
        public List<Gladiator> squadComponents;
        public ActiveMissionUI uiReference;
        
        // Değişkenler int (Tamsayı) oldu
        public int totalDuration; 
        public int daysRemaining;
    }
}