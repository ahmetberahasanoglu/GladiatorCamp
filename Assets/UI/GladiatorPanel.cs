using UnityEngine;
using TMPro;

public class GladiatorPanel : MonoBehaviour
{
    public TextMeshProUGUI infoText;

    private Gladiator _currentGladiator; // Şu an kimi gösteriyoruz?


    // Event sistemlerinde, obje kapanırken aboneliği iptal etmek çok önemlidir!
    // Yoksa "MissingReferenceException" hatası alırsın.
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

    // ARTIK PARAMETRE OLARAK 'JanissaryData' YERİNE 'Gladiator' ALIYORUZ
    public void ShowInfo(Gladiator gladiator)
    {
        // 1. Eğer daha önce başkasına bakıyorsak, ondan aboneliğimizi çekelim
        if (_currentGladiator != null)
        {
            _currentGladiator.OnStatsChanged -= UpdateUI;
        }

        // 2. Yeni askeri kaydet ve panelini aç
        _currentGladiator = gladiator;
        gameObject.SetActive(true);

        // 3. Yeni askerin değişim olayına abone ol
        _currentGladiator.OnStatsChanged += UpdateUI;

        // 4. İlk açılışta verileri ekrana yaz
        UpdateUI();
    }

    // Bu fonksiyon hem ilk açılışta hem de event tetiklendiğinde çalışır
    private void UpdateUI()
    {
        if (_currentGladiator == null) return;
        
        // Veriye _currentGladiator.data üzerinden ulaşıyoruz
        JanissaryData data = _currentGladiator.data;
        string gaziTitle = data.isGazi ? "<color=yellow>[GAZİ]</color> " : "";

        infoText.text =
               $"<size=120%>{gaziTitle}{data.gladiatorName}</size>\n" + 
            "---------\n" +
            $"STR: {data.strength}\n" +
            $"DEF: {data.defense}\n" +
            $"SPD: {data.speed}\n" +
            $"MOR: {data.morale}\n" +
            $"STA: {data.stamina}\n" +
            $"Level: {data.level}\n" +
            $"Değer: {data.GetSellValue()} Akçe";
    }
}