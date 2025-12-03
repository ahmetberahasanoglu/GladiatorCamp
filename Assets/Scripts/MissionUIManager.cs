using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MissionUIManager : MonoBehaviour
{
    public static MissionUIManager Instance;

    // ... Diğer değişkenlerin aynı kalsın ...
    [Header("Veriler")]
    public List<MissionData> availableMissions;
    
    [Header("UI Referansları")]
    public Transform missionListParent;
    public MissionItemUI missionItemPrefab;
    public GameObject detailsPanel;
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI missionDescText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI winChanceText;
    public Button startButton;
    public Transform soldierListParent;
    public SquadSlotUI soldierSlotPrefab;

    private MissionData _selectedMission;
    
    // DEĞİŞİKLİK: Listemiz artık Component tutuyor
    private List<Gladiator> _selectedSquad = new List<Gladiator>();

    void Awake()
    {
        Instance = this;
        detailsPanel.SetActive(false);
    }

    void Start()
    {
        RefreshMissionList();
    }

    void RefreshMissionList()
    {
        foreach (Transform child in missionListParent) Destroy(child.gameObject);
        foreach (var mission in availableMissions)
        {
            var ui = Instantiate(missionItemPrefab, missionListParent);
            ui.Setup(mission, OnMissionSelected);
        }
    }

    void OnMissionSelected(MissionData mission)
    {
        _selectedMission = mission;
        _selectedSquad.Clear();
        detailsPanel.SetActive(true);

        missionTitleText.text = mission.missionName;
        missionDescText.text = mission.description;
        rewardText.text = $"{mission.goldReward} Akçe";

        RefreshSoldierList();
        UpdateWinChance();
    }

    // --- KRİTİK DEĞİŞİKLİK BURADA ---
    void RefreshSoldierList()
    {
        foreach (Transform child in soldierListParent) Destroy(child.gameObject);

        var allGladiators = FindObjectsOfType<Gladiator>();

        foreach (var glad in allGladiators)
        {
            // FİLTRE: Eğer asker meşgulse (Seferde veya Eğitimde) listeye ekleme!
            if (!glad.IsAvailable) continue;

            var slot = Instantiate(soldierSlotPrefab, soldierListParent);
            
            // Setup'a artık Component gönderiyoruz
            slot.Setup(glad, OnSoldierToggled);
        }
    }
    // --------------------------------

    void OnSoldierToggled(Gladiator glad, bool isSelected)
    {
        if (isSelected) _selectedSquad.Add(glad);
        else _selectedSquad.Remove(glad);

        UpdateWinChance();
    }

    void UpdateWinChance()
    {
        if (_selectedMission == null) return;

        int totalPower = 0;
        // Güç verisine ulaşmak için .data kullanıyoruz
        foreach (var s in _selectedSquad) totalPower += s.data.GetTotalStats();

        float ratio = (float)totalPower / _selectedMission.difficulty;
        int percentage = Mathf.Clamp(Mathf.RoundToInt(ratio * 50), 0, 100);

        winChanceText.text = $"Ordu Gücü: {totalPower} / {_selectedMission.difficulty}\nKazanma Şansı: %{percentage}";
        startButton.interactable = _selectedSquad.Count > 0;
    }

    public void OnStartButtonClicked()
    {
        if (_selectedMission == null || _selectedSquad.Count == 0) return;

        // MissionManager artık List<Gladiator> bekliyor, elimizdeki de o.
        MissionManager.Instance.StartMission(_selectedMission, _selectedSquad);
        
        detailsPanel.SetActive(false);
    }
}