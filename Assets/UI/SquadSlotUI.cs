using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action için

public class SquadSlotUI : MonoBehaviour
{
    // ... Eski değişkenlerin aynı ...
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI powerText;
    public Image backgroundImage;
    public Button toggleButton;

    // DEĞİŞİKLİK: Veri yerine Component tutuyoruz
    public Gladiator gladiatorComponent; 
    private bool isSelected = false;
    private Action<Gladiator, bool> onToggleCallback;

    public void Setup(Gladiator glad, Action<Gladiator, bool> onToggle)
    {
        gladiatorComponent = glad;
        onToggleCallback = onToggle;

        // Veriye component üzerinden ulaşıyoruz
        nameText.text = glad.data.gladiatorName;
        powerText.text = "Güç: " + glad.data.GetTotalStats();

        UpdateVisuals();

        toggleButton.onClick.RemoveAllListeners();
        toggleButton.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        isSelected = !isSelected;
        UpdateVisuals();
        onToggleCallback?.Invoke(gladiatorComponent, isSelected);
    }

    void UpdateVisuals()
    {
        backgroundImage.color = isSelected ? Color.green : Color.white;
    }
}