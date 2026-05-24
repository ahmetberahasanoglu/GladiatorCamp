using UnityEngine;

/// <summary>
/// Bir düşman tipinin tüm özelliklerini tanımlar.
/// Inspector'da oluştur: Create → Data → EnemyLoadout
/// Örnekler: Bizans_Kılıçlı_T1, Haydut_Zehirli_T2, Ceneviz_Ateşli_T3
/// </summary>
[CreateAssetMenu(menuName = "Data/EnemyLoadout")]
public class EnemyLoadout : ScriptableObject
{
    [Header("Kimlik")]
    public string      displayName = "Düşman";
    public Sprite      portrait;           // TFT ekranında gösterilecek

    [Header("Element")]
    public ElementType elementType = ElementType.Normal;

    [Header("Temel Statlar")]
    public int baseStrength  = 8;
    public int baseDefense   = 5;
    public int baseSpeed     = 5;
    public int baseStamina   = 8;
    public int baseLevel     = 1;

    [Header("Ekipman Bonus (eşyasız stat eklentisi)")]
    [Tooltip("Silah benzeri bonus — saldırı gücüne eklenir")]
    public int weaponBonus   = 0;
    [Tooltip("Zırh benzeri bonus — savunmaya eklenir")]
    public int armorBonus    = 0;

    [Header("Görsel — 3D Sahnede")]
    [Tooltip("Hangi mesh'ler aktif olacak (targetMeshName listesi)")]
    public string[] activeMeshNames;

    [Header("Ödül")]
    [Tooltip("Bu tip düşman öldürülünce ekstra altın. BattleManager formülüne eklenir.")]
    public int bonusGoldOnKill = 0;
}