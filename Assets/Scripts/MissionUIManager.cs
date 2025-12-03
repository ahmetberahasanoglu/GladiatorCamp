using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MissionUIManager : MonoBehaviour
{
    public static MissionUIManager Instance;

    [Header("Veriler")]
    public List<MissionData> availableMissions; // ScriptableObject'leri buraya sürükle
    public List<JanissaryData> availableSoldiers; // Sahnedeki askerleri buraya çekeceğiz

    [Header("Sol Panel (Görev Listesi)")]
    public Transform missionListParent;
    public MissionItemUI missionItemPrefab;

    [Header("Sağ Panel (Detaylar)")]
    public GameObject detailsPanel; // Başlangıçta kapalı olsun
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI missionDescText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI winChanceText; // "Kazanma Şansı: %80" gibi
    public Button startButton;

    [Header("Asker Seçimi")]
    public Transform soldierListParent; // Scroll View Content
    public SquadSlotUI soldierSlotPrefab;

    // Şu an seçili olanlar
    private MissionData _selectedMission;
    private List<JanissaryData> _selectedSquad = new List<JanissaryData>();

    void Awake()
    {
        Instance = this;
        detailsPanel.SetActive(false);
    }

    void Start()
    {
        RefreshMissionList();
        // Normalde asker listesini GameManger'dan çekersin. 
        // Şimdilik test için elle doldurabilir veya FindObjects ile bulabilirsin.
    }

    // Sol taraftaki listeyi oluşturur
    void RefreshMissionList()
    {
        foreach (Transform child in missionListParent) Destroy(child.gameObject);

        foreach (var mission in availableMissions)
        {
            var ui = Instantiate(missionItemPrefab, missionListParent);
            ui.Setup(mission, OnMissionSelected);
        }
    }

    // Göreve tıklayınca sağ tarafı doldurur
    void OnMissionSelected(MissionData mission)
    {
        _selectedMission = mission;
        _selectedSquad.Clear(); // Yeni görev seçince takımı sıfırla

        detailsPanel.SetActive(true);

        missionTitleText.text = mission.missionName;
        missionDescText.text = mission.description;
        rewardText.text = $"{mission.goldReward} Akçe";
        
        RefreshSoldierList(); // Asker listesini oluştur
        UpdateWinChance();    // Şansı hesapla
    }

    // Sağ alttaki asker listesini oluşturur
    void RefreshSoldierList()
    {
        foreach (Transform child in soldierListParent) Destroy(child.gameObject);

        // SAHNEDEKİ TÜM ASKERLERİ BULALIM (Geçici Yöntem)
        // Gerçek projede: GameManager.Instance.MySoldiers listesini kullanmalısın.
        var allGladiators = FindObjectsOfType<Gladiator>(); 
        
        foreach (var glad in allGladiators)
        {
            // Sadece boşta olanları listele (Eğitimde olmayanları)
            // if (glad.GetComponent<GladiatorTraining>().IsTraining) continue;

            var slot = Instantiate(soldierSlotPrefab, soldierListParent);
            slot.Setup(glad.data, OnSoldierToggled);
        }
    }

    // Asker kutucuğuna tıklanınca çalışır
    void OnSoldierToggled(JanissaryData data, bool isSelected)
    {
        if (isSelected)
            _selectedSquad.Add(data);
        else
            _selectedSquad.Remove(data);

        UpdateWinChance();
    }

    void UpdateWinChance()
    {
        if (_selectedMission == null) return;

        int totalPower = 0;
        foreach (var s in _selectedSquad) totalPower += s.GetTotalStats();

        // Basit bir kazanma şansı hesabı
        // Eğer güç == zorluk ise şans %50 olsun.
        float ratio = (float)totalPower / _selectedMission.difficulty;
        int percentage = Mathf.Clamp(Mathf.RoundToInt(ratio * 50), 0, 100); 

        winChanceText.text = $"Ordu Gücü: {totalPower} / {_selectedMission.difficulty}\nKazanma Şansı: %{percentage}";
        
        // Eğer hiç asker seçilmediyse butonu kapat
        startButton.interactable = _selectedSquad.Count > 0;
    }

    public void OnStartButtonClicked()
    {
        if (_selectedMission == null || _selectedSquad.Count == 0) return;

        Debug.Log("Sefer Başladı! Gidilen yer: " + _selectedMission.missionName);
        
        // BURADA MISSION MANAGER'I ÇAĞIRACAKSIN (Önceki mesajdaki kod)
        // MissionManager.Instance.SendOnMission(_selectedMission, _selectedSquad);
        
        detailsPanel.SetActive(false); // Pencereyi kapat
    }
}