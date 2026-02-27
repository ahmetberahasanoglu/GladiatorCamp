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
            hoverTextObj.GetComponentInChildren<TextMeshPro>().text = "Cenk Oyna\n(Moral Kazan)";
        }
    }

    void OnMouseExit()
    {
        transform.localScale = originalScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseDown()
    {
        // Masaya tıklandığında mini oyun panelini aç ve oyunu başlat!
        if (CenkGameManager.Instance != null)
        {
            CenkGameManager.Instance.OpenMinigame();
            OnMouseExit(); // Yazıyı gizle
        }
    }
}