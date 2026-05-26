using UnityEngine;

[CreateAssetMenu(fileName = "NewMission", menuName = "Game/MissionData")]
public class MissionData : ScriptableObject
{
    public string missionName;
    [TextArea] public string description;
    public int difficulty;
    public int durationDays;

    [Header("Ödüller")]
    public int goldReward;
    public int woodReward;
    public int foodReward;          // Geyik avı, avlanma vb. görevler için
    public int reputationReward;    
}
