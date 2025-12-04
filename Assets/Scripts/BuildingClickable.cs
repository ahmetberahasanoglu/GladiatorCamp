using UnityEngine;
using UnityEngine.Events; // Unity Eventlerini kullanacağız

public class BuildingClickable : MonoBehaviour
{
    [Header("Ayarlar")]
    public string buildingName; // Örn: "Silahhane"
    public Color highlightColor = Color.yellow; // Üzerine gelince parlasın
    
    // Tıklanınca ne olsun? (Inspector'dan seçeceğiz)
    public UnityEvent OnClick; 

    private Renderer _renderer;
    private Color _originalColor;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null) _originalColor = _renderer.material.color;
    }

    // Mouse üzerine gelince (İsteğe bağlı, PC için güzel detay)
    void OnMouseEnter()
    {
        if (_renderer != null) _renderer.material.color = highlightColor;
    }

    void OnMouseExit()
    {
        if (_renderer != null) _renderer.material.color = _originalColor;
    }

    // Tıklanınca
    public void OnMouseDown()
    {
        Debug.Log(buildingName + " binasına tıklandı.");
        OnClick?.Invoke(); // Tanımladığımız fonksiyonu çalıştır
    }
}