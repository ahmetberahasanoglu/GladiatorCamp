using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameQuest {
    public string questID;
    public string description;
    public int targetCount;
    public int currentCount;
    public bool isCompleted;
    public int rewardGold;
}

public class QuestManager : MonoBehaviour {
    public static QuestManager Instance;
    public List<GameQuest> activeQuests = new List<GameQuest>();

    void Awake() { Instance = this; }

    public void TrackProgress(string id, int amount) {
        var quest = activeQuests.Find(q => q.questID == id && !q.isCompleted);
        if (quest != null) {
            quest.currentCount += amount;
            if (quest.currentCount >= quest.targetCount) {
                CompleteQuest(quest);
            }
        }
    }

    void CompleteQuest(GameQuest q) {
        q.isCompleted = true;
        MoneyManager.Instance.Add(q.rewardGold);
        NotificationManager.Instance.Show($"GÖREV TAMAM: {q.description} (+{q.rewardGold} Akçe)", NotificationType.Success);
    }
}