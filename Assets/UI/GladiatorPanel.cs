using UnityEngine;
using TMPro;

public class GladiatorPanel : MonoBehaviour
{
    public TextMeshProUGUI infoText;

    private Gladiator _currentGladiator; // Şu an kimi gösteriyoruz?

    void OnDisable()
    {
        if (_currentGladiator != null)
        {
            _currentGladiator.OnStatsChanged -= UpdateUI;
        }
        if (TrainingUIManager.Instance != null)
        {
            TrainingUIManager.Instance.SetCurrentGladiator(null);
        }
    }

    public void ShowInfo(Gladiator gladiator)
    {
        if (_currentGladiator != null)
        {
            _currentGladiator.OnStatsChanged -= UpdateUI;
        }

        _currentGladiator = gladiator;
        gameObject.SetActive(true);

        _currentGladiator.OnStatsChanged += UpdateUI;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_currentGladiator == null) return;
        
        JanissaryData data = _currentGladiator.data;
        
        // --- 1. GAZİ ÜNVANI (Daha parlak altın sarısı) ---
        string gaziTitle = data.isGazi ? "<color=#FFD700>[GAZİ]</color> " : "";

        // --- 2. CAN GÖSTERİMİ VE RENKLENDİRME ---
        int currentHp = Mathf.RoundToInt(_currentGladiator.currentHealth);
        int maxHp = Mathf.RoundToInt(_currentGladiator.maxHealth);
        
        string hpColor = "green"; // Varsayılan sağlıklı
        if (currentHp < maxHp * 0.3f) hpColor = "red";        // %30 altı (Ağır yaralı)
        else if (currentHp < maxHp * 0.7f) hpColor = "orange"; // %70 altı (Hafif yaralı)

        // --- 3. MEŞGULİYET (DURUM) KONTROLÜ ---
        string statusText = "<color=white>Boşta</color>";
        
        if (_currentGladiator.isOnMission) 
        {
            statusText = "<color=red>Seferde</color>";
        }
        else 
        {
            // Eğitim ve Şifa scriptlerine bakarak ne yaptığını anlıyoruz
            var training = _currentGladiator.GetComponent<GladiatorTraining>();
            var healing = _currentGladiator.GetComponent<GladiatorHealing>();
            
            if (training != null && training.IsTraining) 
                statusText = "<color=yellow>Talimde</color>";
            else if (healing != null && healing.IsHealing) 
                statusText = "<color=green>Şifahanede</color>";
        }

        // --- 4. YAZIYI OLUŞTURMA ---
        infoText.text =
            $"<size=120%>{gaziTitle}{data.gladiatorName}</size>\n" + 
            $"DURUM: {statusText}\n" +
            $"CAN: <color={hpColor}>{currentHp} / {maxHp}</color>\n" +
            "-----------------\n" +
            $"STR: {data.strength}\n" +
            $"DEF: {data.defense}\n" +
            $"SPD: {data.speed}\n" +
            $"MOR: {data.morale}\n" +
            $"STA: {data.stamina}\n" +
            $"Seviye: {data.level}";
    }
}