using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingUI : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;     // "Seviye: 1"
    public TextMeshProUGUI infoText;      // "Kapasite: 3 -> 5"
    public TextMeshProUGUI costText;      // "250 Akçe"
    public Button upgradeButton;

    private Building _myBuilding;

    public void Setup(Building building)
    {
        _myBuilding = building;
        RefreshVisuals(); // İlk açılışta verileri yaz

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    void OnUpgradeClicked()
    {
        // 1. Manager'a git ve yükselt
        CampManager.Instance.UpgradeBuilding(_myBuilding.id);

        // 2. Ekranı hemen yenile ki yeni fiyatı ve leveli görelim
        RefreshVisuals();
    }

    void RefreshVisuals()
    {
        if (_myBuilding == null) return;

        nameText.text = _myBuilding.displayName;
        levelText.text = "Seviye: " + _myBuilding.level;
        
        // ÖNEMLİ: Mevcut kapasite ve sonraki kapasiteyi gösteriyoruz
        infoText.text = $"Kapasite: {_myBuilding.GetValue()} <color=green>-> {_myBuilding.GetNextValue()}</color>";
        
        costText.text = _myBuilding.GetCost() + " Akçe";
        
        // Paramız yetmiyorsa butonu sönük yapabiliriz (Opsiyonel)
        // upgradeButton.interactable = MoneyManager.Instance.gold >= _myBuilding.GetCost();
    }
}