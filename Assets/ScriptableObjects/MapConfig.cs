using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapIconsConfig", menuName = "Game/Map Icons Config")]
public class MapConfig : ScriptableObject
{
    [Header("İkonlar")]
      
    public Sprite villageIcon;
    public Sprite battleIcon;
    public Sprite bossIcon;
    public Sprite restIcon;
    public Sprite startIcon; 
    public Sprite treasureIcon; 
    public Sprite guresIcon; 
    public Sprite horseIcon; 
    public Sprite arrowIcon; 
    public Sprite cenkIcon; 
    public Sprite kervanIcon; 
    public Sprite dervishIcon; 
    public Sprite nasibIcon; 
    public Sprite diceIcon; 
    public Sprite shieldIcon; 
    public Sprite caravanIcon; 
    public Sprite wildIcon; 
    public Sprite tuccarIcon; 
    public Sprite dungeonIcon; 
    public Sprite towerIcon; 


     

    // Tipe göre doğru ikonu veren yardımcı fonksiyon
    public Sprite GetIcon(NodeType type)
    {
        switch (type)
        {
            case NodeType.Village: return villageIcon;
            case NodeType.Battle: return battleIcon;
            case NodeType.Boss: return bossIcon;
            case NodeType.RestArea: return restIcon;
            case NodeType.StartPoint: return startIcon;
            case NodeType.Archery: return arrowIcon;
            case NodeType.CenkOyunu: return cenkIcon;
            case NodeType.Atyarisi: return horseIcon;
            case NodeType.CaravanEncounter: return caravanIcon;
            case NodeType.DervishEncounter: return dervishIcon;
            case NodeType.Dice: return diceIcon;
            case NodeType.FirstBattle: return battleIcon;
            case NodeType.Kalkan: return shieldIcon;
            case NodeType.KervanEncounter: return kervanIcon;
            case NodeType.KuleSavas: return towerIcon;
            case NodeType.NasibEncounter: return nasibIcon;
            case NodeType.Treasure: return treasureIcon;
            case NodeType.Tuccar: return tuccarIcon;
            case NodeType.Vahsi: return wildIcon;
            case NodeType.Zindan: return dungeonIcon;
            default: return villageIcon;
        }
    }
}