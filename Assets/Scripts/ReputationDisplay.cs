using UnityEngine;
using TMPro;

public class ReputationDisplay : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;

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
        _textMesh.text = $"İtibar: {amount}";

        // Renklendirme (İsteğe bağlı)
        if (amount >= 80) _textMesh.color = Color.green;       
        else if (amount >= 50) _textMesh.color = Color.white;
        else _textMesh.color = Color.red;                    
    }
}