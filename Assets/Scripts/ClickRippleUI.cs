using UnityEngine;
using UnityEngine.UI;

public class ClickRippleUI : MonoBehaviour
{
    public float expandSpeed = 5f; // Halkanın büyüme hızı
    public float fadeSpeed = 3f;   // Yok olma hızı

    private Image _image;
    private Color _color;

    void Start()
    {
        _image = GetComponent<Image>();
        if (_image != null) _color = _image.color;
        
        // Yarım saniye sonra objeyi tamamen silerek RAM'i temizle
        Destroy(gameObject, 0.5f); 
    }

    void Update()
    {
        // Sürekli büyüt
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        // Giderek saydamlaştır (Fade out)
        if (_image != null)
        {
            _color.a -= fadeSpeed * Time.deltaTime;
            _image.color = _color;
        }
    }
}