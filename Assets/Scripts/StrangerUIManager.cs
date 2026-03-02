using UnityEngine;
using TMPro;

public class StrangerUIManager : MonoBehaviour
{
    public static StrangerUIManager Instance;

    public GameObject offerPanel; // Açılıp kapanacak ana UI paneli
    public TextMeshProUGUI titleText; // Örn: "Gizemli Tüccar"
    public TextMeshProUGUI descText;  // Örn: "Bana 50 altın ver..."
    
    private WanderingStranger currentStranger;

    void Awake()
    {
        Instance = this;
        if (offerPanel != null) offerPanel.SetActive(false);
    }

    public void OpenOfferPanel(WanderingStranger stranger)
    {
        currentStranger = stranger;
        titleText.text = stranger.strangerName;
        descText.text = stranger.offerText;
        
        offerPanel.SetActive(true);
    }

    public void OnAcceptButtonClicked()
    {
        if (currentStranger != null) currentStranger.AcceptOffer();
    }

    public void OnDeclineButtonClicked()
    {
        if (currentStranger != null) currentStranger.DeclineOffer();
    }

    public void ClosePanel()
    {
        offerPanel.SetActive(false);
        currentStranger = null;
    }
}