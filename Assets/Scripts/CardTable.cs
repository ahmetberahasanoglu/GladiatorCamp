using UnityEngine;
using TMPro;

public class CardTable : MonoBehaviour
{
    [Header("UI Etkileşim")]
    public GameObject hoverTextObj;
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseEnter()
    {
        transform.localScale = hoverScale;
        if (hoverTextObj != null) 
        {
            hoverTextObj.SetActive(true);
            
            // Eğer bugün oynadıysa farklı, oynamadıysa farklı yazı çıksın
            if (CenkGameManager.Instance != null && CenkGameManager.Instance.hasPlayedToday)
            {
                hoverTextObj.GetComponentInChildren<TextMeshPro>().text = "<color=red>Bugünlük Yeter</color>\n(Yarın Gel)";
            }
            else
            {
                hoverTextObj.GetComponentInChildren<TextMeshPro>().text = "Cenk Oyna\n(Moral Kazan)";
            }
        }
    }

    void OnMouseExit()
    {
        transform.localScale = originalScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseDown()
    {
        if (CenkGameManager.Instance != null)
        {
            // 1. KONTROL: Bugün oynanmış mı?
            if (CenkGameManager.Instance.hasPlayedToday)
            {
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.Show("Cenk oynamak için yarını beklemelisin!", NotificationType.Warning);
                }
                return; // Kodu burada kes, oyunu AÇMA!
            }

            // 2. Oynanmadıysa oyunu aç
            CenkGameManager.Instance.OpenMinigame();
            OnMouseExit(); // Yazıyı gizle
        }
    }
}