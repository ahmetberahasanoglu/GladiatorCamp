using UnityEngine;

[CreateAssetMenu(menuName = "Data/Mission")]
public class MissionData : ScriptableObject
{
    public string missionName; // Örn: "Mohaç Seferi"
    [TextArea] public string description; // Örn: "Macar ordusu ovada karşılanacak..."
    
    [Header("Gereksinimler")]
    public int difficulty;     // Örn: 150 (Askerlerin toplam gücü bunu geçmeli)
    public int durationDays;   // Örn: 5 gün sürecek
    
    [Header("Ödüller")]
    public int goldReward;     // Örn: 500 Akçe
}