using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Günü bitir butonuna basılmadan önce maaş ödeyemeyecekse uyarı popup'ı açar.
/// DayManager.UITriggerNextDay() yerine bu scriptin TryNextDay()'ini bağla.
/// </summary>
public class WageWarningUI : MonoBehaviour
{
    public static WageWarningUI Instance;

    [Header("Panel")]
    public GameObject warningPanel;

    [Header("Metinler")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailText;

    [Header("Butonlar")]
    public Button confirmButton;   // "Devam Et — Ödeyemeyeceğimi biliyorum"
    public Button cancelButton;    // "Geri Dön"

    void Awake()
    {
        Instance = this;
        if (warningPanel != null) warningPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);
    }

    /// <summary>
    /// "Günü Bitir" butonuna bu fonksiyonu bağla (DayManager.UITriggerNextDay yerine).
    /// </summary>
    public void TryNextDay()
    {
        if (MoneyManager.Instance == null || DayManager.Instance == null) return;

        int gold      = MoneyManager.Instance.gold;
        int wageCost  = MoneyManager.Instance.GetExpectedDailyWageCost();

        if (gold < wageCost)
        {
            // Uyarıyı göster
            ShowWarning(gold, wageCost);
        }
        else
        {
            // Para yetiyorsa direkt devam
            DayManager.Instance.UITriggerNextDay();
        }
    }

    void ShowWarning(int currentGold, int wageCost)
    {
        if (warningPanel != null) warningPanel.SetActive(true);

        if (titleText != null)
            titleText.text = "<color=red>   </color>";

        if (detailText != null)
            detailText.text =
                $"Kasanda <color=yellow>{currentGold} Akçe</color> var.\n" +
                $"Yarın <color=red>{wageCost} Akçe</color> maaş gerekiyor.\n\n" +
                $"Ödeyemezsen askerler <color=red>-15 Moral</color> ve\n" +
                $"<color=red>-2 Nasip</color> kaybeder!";
    }

    void OnConfirm()
    {
        // Oyuncu uyarıya rağmen devam etmek istiyor
        if (warningPanel != null) warningPanel.SetActive(false);
        DayManager.Instance.UITriggerNextDay();
    }

    void OnCancel()
    {
        if (warningPanel != null) warningPanel.SetActive(false);
        // Panel kapandı, oyuncu para bulmaya çalışacak
    }
}
