using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WorkSlotUI : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TextMeshProUGUI nameText;     // "Kara Murat"
    public TextMeshProUGUI wageText;     // "Günlük: 50 Akçe"
    public Button toggleButton;          // Tıklanacak buton
    public TextMeshProUGUI statusText;   // Butonun içindeki yazı ("TALİMDE" / "ÇALIŞIYOR")
    public Image statusImage;            // Butonun rengi (Yeşil / Turuncu)

    private Gladiator _soldier;

    // Renkler
    private Color trainingColor = new Color(0.2f, 0.6f, 0.2f); // Koyu Yeşil
    private Color workingColor = new Color(0.8f, 0.5f, 0f);    // Turuncu

    public void Setup(Gladiator soldier)
    {
        _soldier = soldier;

        nameText.text = soldier.name; // Senin değişkene göre düzenle
        wageText.text = $"Getiri: {_soldier.dailyWage} Akçe";

        // Butonu temizle ve dinle
        toggleButton.onClick.RemoveAllListeners();
        toggleButton.onClick.AddListener(OnToggleClick);

        UpdateVisuals();
    }

    void OnToggleClick()
    {
        // Durumu tam tersine çevir
        if (_soldier.currentActivity == SoldierActivity.Training)
        {
            _soldier.SetActivity(SoldierActivity.Working);
        }
        else
        {
            _soldier.SetActivity(SoldierActivity.Training);
        }

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (_soldier.currentActivity == SoldierActivity.Training)
        {
            statusText.text = "TALİM YAPIYOR";
            statusImage.color = trainingColor; // Yeşil
        }
        else
        {
            statusText.text = "ÇALIŞIYOR";
            statusImage.color = workingColor; // Turuncu
        }
    }
}