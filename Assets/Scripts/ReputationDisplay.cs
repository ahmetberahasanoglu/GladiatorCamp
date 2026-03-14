using UnityEngine;
using TMPro;
using UnityEngine.UI; // YENİ: Image ve bar işlemleri için gerekli

public class ReputationDisplay : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;
    private bool isInitialized = false;

    [Header("Bar Ayarları")]
    public Image fillBar; // Inspector'dan yeşil bar imajını buraya sürükle
    public float maxReputation = 100f; // Maksimum itibar sınırı

    void Awake()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        if (ReputationManager.Instance != null)
        {
            UpdateText(ReputationManager.Instance.GetReputation());
            ReputationManager.Instance.OnReputationChanged += UpdateText;
            isInitialized = true;
        }
    }

    void OnDestroy()
    {
        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.OnReputationChanged -= UpdateText;
        }
    }

    void UpdateText(int amount)
    {
        _textMesh.text = $" {amount}";
        
        if (amount <= 50) _textMesh.color = Color.red;    
        else _textMesh.color = Color.black; 

        // YENİ: Bar Doluluk Oranı (Float dönüşümü ile kusursuz hesaplama)
        if (fillBar != null)
        {
            fillBar.fillAmount = Mathf.Clamp01((float)amount / maxReputation);
        }

        if (isInitialized && TopInfoBarUI.Instance != null)
        {
            TopInfoBarUI.Instance.FlashUI(_textMesh);
        } 
    }
}