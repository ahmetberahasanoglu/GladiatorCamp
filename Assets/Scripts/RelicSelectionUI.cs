using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RelicSelectionUI : MonoBehaviour
{
    public GameObject relicPanel; 
    
    public Button[] relicButtons; 
    public TextMeshProUGUI[] relicTitleTexts;
    public TextMeshProUGUI[] relicDescTexts;

    void Awake()
    {
        if (relicPanel != null) relicPanel.SetActive(false);
    }

    public void ShowOptions(List<RelicType> options)
    {
        relicPanel.SetActive(true);

        for (int i = 0; i < relicButtons.Length; i++)
        {
            relicButtons[i].gameObject.SetActive(false);
            relicButtons[i].onClick.RemoveAllListeners(); 
        }

        for (int i = 0; i < options.Count; i++)
        {
            relicButtons[i].gameObject.SetActive(true);
            RelicType currentRelic = options[i];

            relicTitleTexts[i].text = GetRelicName(currentRelic);
            relicDescTexts[i].text = GetRelicDescription(currentRelic);

            relicButtons[i].onClick.AddListener(() => OnRelicSelected(currentRelic));
        }
    }

    private void OnRelicSelected(RelicType chosenRelic)
    {
        // Seçimi yöneticiye bildir. O sıradakine geçip geçmeyeceğine karar verir.
        MetaProgressionManager.Instance.UnlockRelic(chosenRelic);
    }

    // Eğer kuyrukta bir şey kalmadıysa Yönetici bu metodu çağırıp paneli kapatır
    public void ClosePanel()
    {
        relicPanel.SetActive(false);
    }
    private string GetRelicName(RelicType type)
    {
        // MetaProgressionManager'da zaten tanımlı — oradan al
        if (MetaProgressionManager.Instance != null)
            return MetaProgressionManager.Instance.GetRelicDisplayName(type);

        // Fallback (Manager yoksa)
        return type.ToString();
    }

    private string GetRelicDescription(RelicType type)
    {
        if (MetaProgressionManager.Instance != null)
            return MetaProgressionManager.Instance.GetRelicDescription(type);

        return "Atalarından kalma gizemli bir güç.";
    }
}