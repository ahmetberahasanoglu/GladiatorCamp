using UnityEngine;

[CreateAssetMenu(menuName = "Data/Relic")]
public class RelicData : ScriptableObject
{
    public string relicID;
    public string relicName;
    [TextArea] public string description; // Örn: "Tüm askerlerin saldırı gücü %10 artar."
    public Sprite icon;
}