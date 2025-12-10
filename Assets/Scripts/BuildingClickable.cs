using UnityEngine;
using UnityEngine.Events; // Unity Eventlerini kullanacağız
using UnityEngine.EventSystems;

public class BuildingClickable : MonoBehaviour
{
    [Header("Ayarlar")]
    public string buildingName; 
    public Color highlightColor = Color.yellow;
    
    public UnityEvent OnClick; 

    private Renderer _renderer;
    private Color _originalColor;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null) _originalColor = _renderer.material.color;
    }
    void OnMouseEnter()
    {
        if (_renderer != null) _renderer.material.color = highlightColor;
    }

    void OnMouseExit()
    {
        if (_renderer != null) _renderer.material.color = _originalColor;
    }


    public void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        Debug.Log(buildingName + " binasına tıklandı.");
        OnClick?.Invoke(); 
    }
}