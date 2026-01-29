using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum BuildingState { Locked, Ruined, Built }

public class BuildingClickable : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    public string buildingName;
    [TextArea] public string description;
    public BuildingState currentState = BuildingState.Built;
    public int repairCost = 1000;

    [Header("Görseller (Modeller)")]
    public GameObject ruinedModel; 
    public GameObject builtModel;  

    [Header("Renk Değişimi")]
    public Color highlightColor = Color.yellow;
    private Renderer _renderer; // O an aktif olan renderer
    private Color _originalColor;

    [Header("Olaylar")]
    public UnityEvent OnBuiltClick;   
    public UnityEvent<BuildingClickable> OnRepairClick; 

    void Start()
    {
        // Start içinde renderer aramaya gerek yok, UpdateVisuals zaten bunu yapacak.
        UpdateVisuals();
    }

    // --- MOUSE ETKİLEŞİMLERİ ---
    void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        // Renderer null değilse rengi değiştir
        if (_renderer != null) _renderer.material.color = highlightColor;
    }

    void OnMouseExit()
    {
        // Renderer null değilse eski rengine döndür
        if (_renderer != null) _renderer.material.color = _originalColor;
    }

    public void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Debug.Log($"{buildingName} tıklandı. Durum: {currentState}");

        switch (currentState)
        {
            case BuildingState.Ruined:
                if (RepairPanelManager.Instance != null)
                {
                    RepairPanelManager.Instance.OpenPanel(this);
                }
                break;

            case BuildingState.Built:
                OnBuiltClick?.Invoke();
                break;
            
            case BuildingState.Locked:
                Debug.Log("Bu bina henüz kilitli!");
                break;
        }
    }

    // --- KRİTİK DÜZELTME BURADA ---
    public void UpdateVisuals()
    {
        // 1. Önce modelleri aç/kapat
        if (ruinedModel != null) ruinedModel.SetActive(currentState == BuildingState.Ruined);
        if (builtModel != null) builtModel.SetActive(currentState == BuildingState.Built);
        
        // 2. Şu an hangi modelin aktif olduğunu bul
        GameObject activeModel = null;
        if (currentState == BuildingState.Built) activeModel = builtModel;
        else if (currentState == BuildingState.Ruined) activeModel = ruinedModel;

        // 3. Aktif modelin Renderer'ını bul (Highlight için şart)
        if (activeModel != null)
        {
            // DÜZELTME: GetComponent yerine GetComponentInChildren kullanıyoruz.
            // Çünkü modelin MeshRenderer'ı genelde ana objede değil, alt objelerindedir.
            _renderer = activeModel.GetComponentInChildren<Renderer>();

            if (_renderer != null)
            {
                _originalColor = _renderer.material.color;
            }
            else
            {
                Debug.LogWarning($"{buildingName} objesinin aktif modelinde Renderer bulunamadı!");
            }
        }
    }

    public void RepairBuilding()
    {
        // NOT: Senin projende fonksiyon adı 'SpendGold' ise onu kullan, kodda 'Spend' yazmışsın.
        if (MoneyManager.Instance != null && MoneyManager.Instance.gold >= repairCost)
        {
            MoneyManager.Instance.Spend(repairCost); // Fonksiyon adını kontrol et
            
            currentState = BuildingState.Built;
            UpdateVisuals(); // Bu çağrılınca renderer Built modele geçecek
            
            Debug.Log("Bina tamir edildi!");
        }
        else
        {
            Debug.Log("Para yetersiz!");
        }
    }
}