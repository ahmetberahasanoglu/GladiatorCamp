using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI effectText; // "Kapasite: 5 -> 7"
    public TextMeshProUGUI costText;
    public Button upgradeButton;

    private Building _building;

    public void Setup(Building building)
    {
        _building = building;
        RefreshVisuals();

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    void RefreshVisuals()
    {
        nameText.text = _building.displayName;
        levelText.text = $"Seviye {_building.level}";
        
        // Etkiyi dinamik göster (Şu an -> Sonraki)
        int currentVal = _building.GetCurrentValue();
        int nextVal = currentVal + _building.valuePerLevel;
        effectText.text = $"Kapasite: {currentVal} <color=green>➜ {nextVal}</color>";

        costText.text = _building.GetUpgradeCost() + " Akçe";
    }

    void OnUpgradeClicked()
    {
        // Manager üzerinden yükselt
        CampManager.Instance.UpgradeBuilding(_building.id);
        
        // Görüntüyü yenile
        RefreshVisuals();
    }
}