using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecruitSlotUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI costText;
    public Button recruitButton;

    private RecruitCandidate _candidate;
    private System.Action _onRecruitSuccess; // UI'ı yenilemek için callback

    public void Setup(RecruitCandidate candidate, System.Action refreshCallback)
    {
        _candidate = candidate;
        _onRecruitSuccess = refreshCallback;

        nameText.text = candidate.candidateName;
        statsText.text = $"Güç: {candidate.potentialStrength} | Dayanıklılık: {candidate.potentialStamina}";
        costText.text = candidate.cost + " Akçe";

        recruitButton.onClick.RemoveAllListeners();
        recruitButton.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        // Manager'a "Bunu al" diyoruz
        RecruitManager.Instance.RecruitSoldier(_candidate);
        
        // Listeyi yenile (Kartın ekrandan gitmesi için)
        _onRecruitSuccess?.Invoke();
    }
}