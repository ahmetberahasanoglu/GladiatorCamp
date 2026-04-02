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
  
    public void OnInventoryButtonClicked()
    {
        // Sahnede bir Selector varsa ona "Seçili olanı aç" komutunu yolla
        if (GladiatorSelector.Instance != null)
        {
            GladiatorSelector.Instance.OpenSelectedInventory();
            AudioManager.Instance.PlayClick();
        }
        else
        {
            Debug.LogWarning("Sahnede GladiatorSelector bulunamadı!");
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
        
        string gaziTitle = data.isGazi ? "<color=#FFD700>[GAZİ]</color> " : "";

        int currentHp = Mathf.RoundToInt(_currentGladiator.currentHealth);
        int maxHp = Mathf.RoundToInt(_currentGladiator.maxHealth);
        
        string hpColor = "green"; 
        if (currentHp < maxHp * 0.3f) hpColor = "red";        
        else if (currentHp < maxHp * 0.7f) hpColor = "orange"; 

        // --- 3. MEŞGULİYET (DURUM) KONTROLÜ (GÜNCELLENDİ) ---
        string statusText = "<color=white>Boşta</color>";
        
        if (_currentGladiator.isOnMission) 
        {
            statusText = "<color=red>Seferde</color>";
        }
        // --- YENİ EKLENEN: ÇALIŞMA KONTROLÜ ---
        else if (data != null && data.currentActivity == SoldierActivity.Working)
        {
            statusText = "<color=red>Çalışıyor</color>"; // Kahverengi/Turuncu tonu
        }
        else if (data != null && data.currentActivity == SoldierActivity.Praying)
        {
             statusText = "<color=green>Dua ediyor</color>";
        }
        // --------------------------------------
        else 
        {
            var training = _currentGladiator.GetComponent<GladiatorTraining>();
            var healing = _currentGladiator.GetComponent<GladiatorHealing>();
            
            if (training != null && training.IsTraining) 
                statusText = "<color=yellow>Talimde</color>";
            else if (healing != null && healing.IsHealing) 
                statusText = "<color=green>Şifahanede</color>";
        }
        int dailyWage = data.level * 5;
        // --- 4. YAZIYI OLUŞTURMA ---
        infoText.text =
            $"DURUM: {statusText}\n" +
            $"CAN: <color={hpColor}>{currentHp} / {maxHp}</color>\n" +
            $"STR: {data.strength}\n" +
            $"DEF: {data.defense}\n" +
            $"SPD: {data.speed}\n" +
            $"STA: {data.stamina}\n" +
            $"Seviye: {data.level}"+  $"<size=120%>{gaziTitle}</size>" +$"\t<color=#FFD700>Ulufe: {dailyWage}</color>";
    }
}