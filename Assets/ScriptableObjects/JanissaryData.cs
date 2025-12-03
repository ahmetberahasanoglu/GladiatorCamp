using UnityEngine;

[CreateAssetMenu(menuName = "Data/Gladiator")]
public class JanissaryData : ScriptableObject
{
    public string gladiatorName;
    public int strength;
    public int defense;
    public int speed;
    public int morale;
    public int stamina;
    public int level = 1;     
   // public int baseTrainingTime = 3;

    public int price = 100;

  public int GetTotalStats()
    {
        return strength + defense + stamina+speed+morale;
    }
  
    public int GetSellValue()
    {
        return price + (GetTotalStats() * 5) + (level * 20);
    }

}
