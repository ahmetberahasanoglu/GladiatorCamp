using UnityEngine;

public enum ItemType
{
    Weapon,
    Armor,
    Shield,
    Helmet,
    Nusha
}

public enum ItemSetType
{
    None,
    Fire,       // Ateş  (Yakma)
    Poison,     // Zehir (Zamanla Hasar)
    Faith       // İnanç (Şifa / Kutsal Alan)
}

[CreateAssetMenu(menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    [Tooltip("Kod içi tanımlayıcı — boşluksuz, küçük harf. Örn: iron_sword")]
    public string itemID;

    [Tooltip("Oyuncuya gösterilen isim. Örn: Demir Kılıç")]
    public string itemName;

    public ItemType type;

    [Header("Set ve Sinerji")]
    [Tooltip("Bu eşyanın hangi efsanevi sete ait olduğunu belirler.")]
    public ItemSetType setType = ItemSetType.None;

    [Header("Görsel ve Fiyat")]
    public Sprite icon;
    public int price;

    [Header("Skinned Mesh")]
    [Tooltip("Karakterin içindeki açılacak mesh objesinin tam adı. Örn: Sword_Iron")]
    public string targetMeshName;
    
    [Header("Eğer Eşya Nüsha (Büyü) ise Doldurulacak:")]
    [Tooltip("Bu kağıt/ferman savaşta hangi yeteneği tetikleyecek?")]
    public CommanderSkillData spellData;

    [Header("Stats")]
    public int bonusStrength;
    public int bonusDefense;
    public int bonusSpeed;
    public int bonusStamina;
   public WeaponClass weaponClass = WeaponClass.Sword; 
    public float weaponRange = 2.0f;
    public bool isRanged = false;

    // --- Yardımcı Metodlar ---

    /// <summary>Görünen isim; itemName doluysa onu, yoksa itemID'yi döner.</summary>
    public string DisplayName => string.IsNullOrEmpty(itemName) ? itemID : itemName;

    /// <summary>Sete ait set adını Türkçe döner.</summary>
    public string GetSetDisplayName()
    {
        return setType switch
        {
            ItemSetType.Fire   => "Ateş",
            ItemSetType.Poison => "Zehir",
            ItemSetType.Faith  => "İnanç",
            _                  => ""
        };
    }

    /// <summary>Set rengi (UI'da badge için).</summary>
    public Color GetSetColor()
    {
        return setType switch
        {
            ItemSetType.Fire   => new Color(1f, 0.35f, 0.1f),   // Turuncu-Kırmızı
            ItemSetType.Poison => new Color(0.4f, 0.85f, 0.2f), // Zehir Yeşili
            ItemSetType.Faith  => new Color(0.9f, 0.8f, 0.2f),  // Altın Sarısı
            _                  => Color.gray
        };
    }
}
