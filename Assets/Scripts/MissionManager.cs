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
        
        newMission.totalDuration = data.durationDays; 
        newMission.daysRemaining = data.durationDays;

        // --- YENİ DURUM KİLİDİ: Askerleri "Seferde" olarak işaretle ---
        foreach (var soldier in squad)
        {
            soldier.isOnMission = true; // (Bunu eski sistemler kırılmasın diye tutuyoruz)
            soldier.SetActivity(SoldierActivity.OnMission); // YENİ AAA KİLİDİMİZ
            
            // Eğer istersen seferdeki askerleri kampta görünmez yapabilirsin
            // soldier.gameObject.SetActive(false); 
        }

        // UI Oluştur
        var uiObj = Instantiate(activeMissionPrefab, activeMissionsParent);
        uiObj.Setup(data.missionName, newMission.totalDuration);
        
        newMission.uiReference = uiObj;
        currentMissions.Add(newMission);
        NotificationManager.Instance.Show($"{data.missionName} başladı. Süre: {data.durationDays} Gün.", NotificationType.Info);
    }

    void CompleteMission(OngoingMission mission)
    {
        Destroy(mission.uiReference.gameObject);
        currentMissions.Remove(mission);

        // --- YENİ DURUM ÇÖZÜCÜ: Asker kilidini aç ---
        foreach (var soldier in mission.squadComponents)
        {
            if (soldier != null)
            {
                soldier.isOnMission = false;
                
                // Geri döndükleri için tekrar BOŞTA (Idle) oldular
                soldier.SetActivity(SoldierActivity.Idling);
                
                // Eğer görünmez yaptıysan burada görünür yapmalısın:
                // soldier.gameObject.SetActive(tr ue);
            }
        }

        int totalPower = 0;
        foreach (var s in mission.squadComponents) totalPower += s.data.GetTotalStats();

        int roll = Random.Range(0, 50);
        bool success = (totalPower + roll) >= mission.originalData.difficulty;

        MissionResultUI popup = Instantiate(resultPopupPrefab, canvasParent);
        popup.Setup(success, mission.originalData);

        if (success) 
        {
            if (MoneyManager.Instance != null) 
                MoneyManager.Instance.Add(mission.originalData.goldReward);
                
            if (ResourceManager.Instance != null && mission.originalData.woodReward > 0) 
                ResourceManager.Instance.AddWood(mission.originalData.woodReward);
                
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show($"Görev Başarılı! +{mission.originalData.goldReward} Akçe, +{mission.originalData.woodReward} Odun", NotificationType.Success);
        }
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