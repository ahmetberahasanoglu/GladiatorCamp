using UnityEngine;
using UnityEngine.UI;

public class TutorialHighlighter : MonoBehaviour
{
    public static TutorialHighlighter Instance;

    [Header("Karanlık Perde (UI)")]
    public GameObject darkScreen;

    [Header("3D Odak (Focus) Objeleri")]
    public GameObject focusMarkerUI; 

    private GameObject currentUIElement;
    private Transform currentTarget3D; // YENİ: 3D Objeyi takip etmek için

    private bool addedCanvas = false;
    private bool addedRaycaster = false;
    private int originalSortingOrder;
    private bool originalOverrideSorting;

    void Awake()
    {
        Instance = this;
        if (focusMarkerUI != null) focusMarkerUI.SetActive(false);
    }

    void Update()
    {
        // YENİ: Eğer bir 3D obje işaretliyse, kameranın hareketine karşı çemberi sürekli objenin üstünde tut!
        if (currentTarget3D != null && focusMarkerUI != null && focusMarkerUI.activeInHierarchy)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(currentTarget3D.position);
            focusMarkerUI.transform.position = screenPos;
        }
    }

    public void HighlightUI(GameObject targetElement)
    {
        RemoveHighlight();
        if (targetElement == null) return;
        
        if (darkScreen != null) darkScreen.SetActive(true);
        currentUIElement = targetElement;

        Canvas canvas = currentUIElement.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = currentUIElement.AddComponent<Canvas>();
            addedCanvas = true;
        }
        else
        {
            addedCanvas = false;
            originalSortingOrder = canvas.sortingOrder;
            originalOverrideSorting = canvas.overrideSorting;
        }
        
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100; 

        GraphicRaycaster raycaster = currentUIElement.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = currentUIElement.AddComponent<GraphicRaycaster>();
            addedRaycaster = true;
        }
        else addedRaycaster = false;
    }

   
    public void Highlight3D(Transform target3D, float scaleMultiplier = 1f)
    {
        RemoveHighlight();
        if (target3D == null || focusMarkerUI == null) return;

        if (darkScreen != null) darkScreen.SetActive(true);
        focusMarkerUI.SetActive(true); 
        
        currentTarget3D = target3D; // Takibe başla
        
        // --- YENİ: ÇEMBERİN BOYUTUNU AYARLA ---
        focusMarkerUI.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
        
        focusMarkerUI.transform.SetAsLastSibling(); 
    }

    public void RemoveHighlight()
    {
        if (darkScreen != null) darkScreen.SetActive(false);
        if (focusMarkerUI != null) focusMarkerUI.SetActive(false);

        currentTarget3D = null; // Takibi bırak

        if (currentUIElement != null)
        {
            if (addedRaycaster) Destroy(currentUIElement.GetComponent<GraphicRaycaster>());
            
            if (addedCanvas) 
                Destroy(currentUIElement.GetComponent<Canvas>());
            else
            {
                Canvas canvas = currentUIElement.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.overrideSorting = originalOverrideSorting;
                    canvas.sortingOrder = originalSortingOrder;
                }
            }
            
            addedRaycaster = false;
            addedCanvas = false;
            currentUIElement = null;
        }
    }
}