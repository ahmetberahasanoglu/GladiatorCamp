using UnityEngine;
public enum SoldierTrait { Siradan, Obur, Dindar, Yetenekli }
[CreateAssetMenu(menuName = "Data/Gladiator")]

public class JanissaryData : ScriptableObject
{
    public string gladiatorName;
    
    [Header("Statlar")]
    public int strength;
    public int defense;
    public int speed;
   // public int morale;
    public int stamina;
    public int level = 1;     
    public bool isGazi = false;
    [Header("Kişilik")]
    public SoldierTrait trait;
    public ElementType elementType = ElementType.Normal;

    [Header("Anlık Durumlar (Save İçin)")]
    public float currentHealth;
    public float maxHealth;
    public bool isOnMission = false;
    public SoldierActivity currentActivity = SoldierActivity.Idling;

    public int dailyWage = 50;

    public int GetTotalStats()
    {
        return strength + defense + stamina + speed ;
    }
    public string GetTraitDescription()
    {
        switch(trait)
        {
            case SoldierTrait.Obur: 
                return "<color=orange>Obur:</color> Çok erzak tüketir ama canı yüksektir.";
            case SoldierTrait.Dindar: 
                return "<color=yellow>Dindar:</color> İbadet sırasında daha fazla Nasip bulur.";
            case SoldierTrait.Yetenekli: 
                return "<color=#00FFFF>Yetenekli:</color> Eğitimlerin hepsinden 2 kat verim alır.";
            case SoldierTrait.Siradan:
            default: 
                return "<color=white>Sıradan:</color> Kendi halinde, sadık bir nefer.";
        }
    }
    public string GetTrait()
    {
        switch(trait)
        {
            case SoldierTrait.Obur: 
                return "<color=orange>Obur</color>";
            case SoldierTrait.Dindar: 
                return "<color=yellow>Dindar</color>";
            case SoldierTrait.Yetenekli: 
                return "<color=#00FFFF>Yetenekli</color>";
            case SoldierTrait.Siradan:
            default: 
                return "<color=white>Sıradan</color>";
        }
    }
}