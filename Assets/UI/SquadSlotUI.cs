using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SquadSlotUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI powerText; // Askerin gücü (Seçim için önemli)
    public Image backgroundImage;     // Seçilince rengi değişsin diye
    public Button toggleButton;

    public JanissaryData gladiator;   // Hangi asker?
    private bool isSelected = false;
    private System.Action<JanissaryData, bool> onToggleCallback;

    public void Setup(JanissaryData data, System.Action<JanissaryData, bool> onToggle)
    {
        gladiator = data;
        onToggleCallback = onToggle;

        nameText.text = data.gladiatorName;
        // Askerin toplam gücünü gösterelim ki oyuncu strateji yapsın
        powerText.text = "Güç: " + data.GetTotalStats(); 

        UpdateVisuals();

        toggleButton.onClick.RemoveAllListeners();
        toggleButton.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        isSelected = !isSelected;
        UpdateVisuals();
        // Yöneticiye haber ver: "Bu asker durumu değiştirdi"
        onToggleCallback?.Invoke(gladiator, isSelected);
    }

    void UpdateVisuals()
    {
        // Seçiliyse Yeşil, değilse Gri/Beyaz yap
        backgroundImage.color = isSelected ? Color.green : Color.white;
    }
}