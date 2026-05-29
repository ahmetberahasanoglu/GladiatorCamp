using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [Header("UI İmleç Bileşenleri")]
    public Image cursorImage; 
    public RectTransform cursorRect; 

    [Header("İmleç Görselleri")]
    public Sprite normalSprite;
    public Sprite attackSprite;
    public Sprite interactSprite;

    [Header("Hotspot Offset")]
    public Vector2 normalOffset = Vector2.zero;
    public Vector2 attackOffset = Vector2.zero;
    public Vector2 interactOffset = Vector2.zero;

    [Header("Animasyon Ayarları (Hover)")]
    public float normalScale = 1f;       // Normal boyutu
    public float interactScale = 1.25f;  // Üzerine gelince ne kadar büyüyecek
    public float scaleSpeed = 15f;       // Büyüme/Küçülme yumuşaklık hızı

    [Header("Tıklama Efekti")]
    public GameObject clickRipplePrefab; // Fare tıklandığında çıkacak efekt
    public Transform canvasParent;       // Efektin oluşacağı yer (CursorCanvas)

    private Vector2 _currentOffset;
    private float _targetScale = 1f;

    void Awake()
    {
        Instance = this;
        Cursor.visible = false; 
    }

    void Start()
    {
        SetNormalCursor(); 
    }

    void Update()
    {
        if (cursorRect != null)
        {
            // 1. Pozisyon Takibi
            Vector2 mousePos = Input.mousePosition;
            cursorRect.position = mousePos + _currentOffset;

            // 2. Yumuşak Büyüme/Küçülme (Hover Geri Bildirimi)
            float currentScale = Mathf.Lerp(cursorRect.localScale.x, _targetScale, Time.deltaTime * scaleSpeed);
            cursorRect.localScale = new Vector3(currentScale, currentScale, 1f);
        }

        // 3. Tıklama Kontrolü
        if (Input.GetMouseButtonDown(0))
        {
            PlayClickAnimation();
        }
    }

    // ── DURUM DEĞİŞTİRİCİLER ──────────────────────────────────────────────

    public void SetNormalCursor()
    {
        if (cursorImage != null) cursorImage.sprite = normalSprite;
        _currentOffset = normalOffset;
        _targetScale = normalScale; // Hedef boyutu normale çek
    }

    public void SetAttackCursor()
    {
        if (cursorImage != null) cursorImage.sprite = attackSprite;
        _currentOffset = attackOffset;
        _targetScale = interactScale; // Hedef boyutu büyüt
    }

    public void SetInteractCursor()
    {
        if (cursorImage != null) cursorImage.sprite = interactSprite;
        _currentOffset = interactOffset;
        _targetScale = interactScale; // Hedef boyutu büyüt
    }

    // ── TIKLAMA ANİMASYONU ────────────────────────────────────────────────

    void PlayClickAnimation()
    {
        // İmlecin kendisine ani bir "tıklanma" darbesi (Punch efekti) ver
        if (cursorRect != null)
        {
            cursorRect.localScale = new Vector3(normalScale * 0.7f, normalScale * 0.7f, 1f);
        }

        // Tıklama dalgası efektini oluştur ve YERİNİ ZORLA SABİTLE
        if (clickRipplePrefab != null && canvasParent != null)
        {
            GameObject ripple = Instantiate(clickRipplePrefab, canvasParent);
            
            // Farenin ucu (Input.mousePosition) neresiyse efekti tam oraya yerleştir
            ripple.transform.position = Input.mousePosition; 
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = !hasFocus;
    }
}