    using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Paralı asker kartının arayüz kodları (Aynı bırakıldı, sadece uyum sağlandı)
/// </summary>
public class MercenaryCardUI : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI totalPowerText;
    public Image           traitBadgeBg;
    public TextMeshProUGUI traitText;
    public TextMeshProUGUI traitDescText;
    public Button          hireButton;
    public TextMeshProUGUI hireButtonText;

    [Header("Trait Renkleri")]
    public Color colorSiradan   = new Color(0.6f, 0.6f, 0.6f);
    public Color colorObur      = new Color(0.9f, 0.4f, 0.1f);
    public Color colorDindar    = new Color(0.9f, 0.8f, 0.2f);
    public Color colorYetenekli = new Color(0.2f, 0.7f, 1.0f);

    private MercenaryData _data;
    private MercenaryCamp _camp;

    public void Setup(MercenaryData data, MercenaryCamp camp, bool campFull)
    {
        _data = data;
        _camp = camp;

       // İsim ve Statlar
        if (nameText != null)       nameText.text = data.soldierName;
        if (statsText != null)      statsText.text = $"STR: {data.strength}   DEF: {data.defense}\nSPD: {data.speed}   STA: {data.stamina}";
        
        // DEĞİŞEN KISIM: Toplam güç text objesini tamamen gizliyoruz
        if (totalPowerText != null) 
        {
            totalPowerText.gameObject.SetActive(false); 
        }

        Color traitColor = data.trait switch
        {
            SoldierTrait.Obur      => colorObur,
            SoldierTrait.Dindar    => colorDindar,
            SoldierTrait.Yetenekli => colorYetenekli,
            _                      => colorSiradan
        };
        
        if (traitBadgeBg  != null) traitBadgeBg.color = traitColor;
        if (traitText     != null) traitText.text      = data.GetTraitDisplayName();
        if (traitDescText != null) traitDescText.text  = data.GetTraitDescription();

        bool canAfford = MoneyManager.Instance != null && MoneyManager.Instance.gold >= data.cost;
        bool canHire   = !campFull && canAfford;

        if (hireButton != null)
        {
            hireButton.interactable = canHire;
            hireButton.onClick.RemoveAllListeners();
            hireButton.onClick.AddListener(OnHireClicked);
        }

        if (hireButtonText != null)
        {
            if (campFull)           hireButtonText.text = "<color=red>Koğuş Dolu</color>";
            else if (!canAfford)    hireButtonText.text = $"<color=red>{data.cost} Akçe</color>";
            else                    hireButtonText.text = $"{data.cost} Akçe\n(Satın Al)";
        }
    }

    void OnHireClicked()
    {
        if (_camp == null || _data == null) return;
        _camp.TryHire(_data);
    }
}

// ── Veri Taşıyıcı ────────────────────────────────────────────────────────
[System.Serializable]
public class MercenaryData
{
    public string       soldierName;
    public int          strength;
    public int          defense;
    public int          speed;
    public int          stamina;
    public SoldierTrait trait;
    public int          cost;

    public int TotalPower => strength + defense + speed + stamina;

    public string GetTraitDisplayName()
    {
        return trait switch
        {
            SoldierTrait.Obur      => "Obur",
            SoldierTrait.Dindar    => "Dindar",
            SoldierTrait.Yetenekli => "Yetenekli",
            _                      => "Sıradan"
        };
    }

    public string GetTraitDescription()
    {
        return trait switch
        {
            SoldierTrait.Obur      => "Günlük erzak tüketimi +1, ama canı yüksek.",
            SoldierTrait.Dindar    => "Camide 2 Nasip kazanır, savaşta moral kaybetmez.",
            SoldierTrait.Yetenekli => "Eğitimden daha hızlı yararlanır.",
            _                      => "Temel eğitimli standart piyade."
        };
    }
}