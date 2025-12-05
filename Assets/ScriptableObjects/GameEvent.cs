using UnityEngine;

[CreateAssetMenu(menuName = "Data/GameEvent")]
public class GameEvent : ScriptableObject
{
    public string title;        // Başlık: "İstanbul'da Yangın!"
    [TextArea] public string description; // Açıklama: "Kışlaya sıçrayan ateş..."
    
    [Header("Etkileri")]
    public int moneyChange;     // -500 (Para kaybı) veya +200
    public int foodChange;      // -50 (Erzak kaybı)
    public int moraleChange;    // -10 (Moral kaybı)
    
    // İleride buraya "Asker Ölümü" veya "Bina Yıkılması" gibi etkiler de eklenebilir.
}