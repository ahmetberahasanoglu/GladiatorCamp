using TMPro;
using UnityEngine;

public class WarTableInteract : MonoBehaviour
{
    [Header("Görsel Geri Bildirim")]
    public GameObject hoverTextObj; 
    public GameObject BgImage;
    public Vector3 hoverScale = new Vector3(1.5f, 1.5f, 1.5f); 
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseEnter()
    {
        // EĞER EĞİTİMDEYSEK VE SIRA MASADA DEĞİLSE HOVER BİLE ÇALIŞMASIN
        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
        {
            if (TutorialManager.Instance.currentStep != TutorialStep.Camp_GoToWar) return;
        }

        transform.localScale = hoverScale;

        if (hoverTextObj != null) 
        {
            hoverTextObj.SetActive(true);
            hoverTextObj.GetComponentInChildren<TextMeshPro>().text = "Haritayı aç";
        }
    }

    void OnMouseExit()
    {
        transform.localScale = originalScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseDown()
    {
        // --- YENİ EĞİTİM (TUTORIAL) KONTROLÜ ---
        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
        {
            // Eğer sıra Harita aşamasında DEĞİLSE tıklamayı tamamen iptal et
            if (TutorialManager.Instance.currentStep != TutorialStep.Camp_GoToWar)
            {
                return; // Hiçbir şey yapma!
            }
            else
            {
                // Sıra haritadaysa, diyalog panelini kapat ve eğitimi bir sonraki aşamaya geçir
                TutorialManager.Instance.AdvanceTutorial();
            }
        }

        // Oyunun normal çalışma mantığı
        if (BgImage != null) BgImage.SetActive(true); 
        MapManager.Instance.ShowMap();
        //AudioManager.Instance.PlayPaper();
    }
}