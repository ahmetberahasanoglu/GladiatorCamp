using UnityEngine;

[CreateAssetMenu(menuName = "Data/Gladiator")]
public class JanissaryData : ScriptableObject
{
    public string gladiatorName;
    
    [Header("Statlar")]
    public int strength;
    public int defense;
    public int speed;
    public int morale;
    public int stamina;
    public int level = 1;     
    public bool isGazi = false;

    [Header("Anlık Durumlar (Save İçin)")]
    public float currentHealth;
    public float maxHealth;
    public bool isOnMission = false;
    public SoldierActivity currentActivity = SoldierActivity.Idling;

    public int dailyWage = 50;

    public int GetTotalStats()
    {
        return strength + defense + stamina + speed + morale;
    }
}