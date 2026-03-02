using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingUI : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;    
    public TextMeshProUGUI infoText;      
    public TextMeshProUGUI costText;      
    public Button upgradeButton;

    private Building _myBuilding;

    public void Setup(Building building)
    {
        _myBuilding = building;
        RefreshVisuals(); 

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    void OnUpgradeClicked()
    {
        CampManager.Instance.UpgradeBuilding(_myBuilding.id);
        RefreshVisuals();
    }

    void RefreshVisuals()
    {
        if (_myBuilding == null) return;

        nameText.text = _myBuilding.displayName;
        levelText.text = "Seviye: " + _myBuilding.level;
        
        // --- YENİ EKLENEN: MAKSİMUM SEVİYE KONTROLÜ ---
        if (_myBuilding.level >= _myBuilding.maxLevel)
        {
            infoText.text = $"Kapasite: {_myBuilding.GetValue()} (MAKS)";
            costText.text = "SON SEVİYE";
            upgradeButton.interactable = false; // Butonu sönük yap
        }
        else
        {
            infoText.text = $"Kapasite: {_myBuilding.GetValue()} <color=green>-> {_myBuilding.GetNextValue()}</color>";
            costText.text = _myBuilding.GetCost() + " Akçe";
            
            // Eğer istersen paran yetmediğinde de butonu sönük yapabilirsin:
            // upgradeButton.interactable = MoneyManager.Instance.gold >= _myBuilding.GetCost();
        }
    }
}