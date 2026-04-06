using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ConfirmationManager : MonoBehaviour
{
    public static ConfirmationManager Instance;

    [Header("UI Bileşenleri")]
    public GameObject confirmationPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button yesButton;
    public Button noButton;

    void Awake()
    {
        Instance = this;
        // Oyun başlarken paneli kapalı tut
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
    }

    // İstenilen her yerden çağrılabilen sihirli fonksiyon
    public void Show(string title, string message, Action onConfirm)
    {
        titleText.text = title;
        messageText.text = message;
        
        confirmationPanel.SetActive(true);

        // Eski tıklama olaylarını temizle
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        // EVET butonuna basılırsa
        yesButton.onClick.AddListener(() => {
            onConfirm?.Invoke(); // Gönderilen asıl kodu çalıştır
            confirmationPanel.SetActive(false); // Paneli kapat
        });

        // HAYIR butonuna basılırsa
        noButton.onClick.AddListener(() => {
            confirmationPanel.SetActive(false); // Sadece paneli kapat, hiçbir şey yapma
        });
    }
}