using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MissionItemUI : MonoBehaviour
{
    public TextMeshProUGUI missionNameText;
    public TextMeshProUGUI difficultyText;
    public Button selectButton;

    // Yönetici, "Bana tıklandığında ne yapayım?" bilgisini (action) buraya gönderecek
    public void Setup(MissionData mission, Action<MissionData> onSelect)
    {
        missionNameText.text = mission.missionName;
        difficultyText.text = $"Zorluk: {mission.difficulty}";

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelect(mission));
    }
}