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
            default: return villageIcon;
        }
    }
}