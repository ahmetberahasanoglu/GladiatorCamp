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


        transform.localScale = hoverScale;

        if (hoverTextObj != null) 
        {
            hoverTextObj.SetActive(true);
            hoverTextObj.GetComponentInChildren<TextMeshPro>().text = $"Haritayı aç)";
        }
    }

    void OnMouseExit()
    {
        transform.localScale = originalScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseDown()
    {
       BgImage.SetActive(true); 
       MapManager.Instance.ShowMap();
       //AudioManager.Instance.PlayPaper();
    }
}
