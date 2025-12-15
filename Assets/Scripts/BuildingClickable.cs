using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// Binanın hangi durumda olduğunu belirleyen seçenekler
public enum BuildingState { Locked, Ruined, Built }

public class BuildingClickable : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    public string buildingName;
    public BuildingState currentState = BuildingState.Built; // Varsayılan: Yapılı
    public int repairCost = 1000; // Tamir ücreti

    [Header("Görseller (Modeller)")]
    public GameObject ruinedModel; // Yıkık halinin 3D objesi
    public GameObject builtModel;  // Sağlam halinin 3D objesi
    // public GameObject lockedIcon; // İstersen kilit ikonu da ekleyebilirsin

    [Header("Renk Değişimi (Eski Kodun)")]
    public Color highlightColor = Color.yellow;
    private Renderer _renderer;
    private Color _originalColor;

    [Header("Olaylar")]
    public UnityEvent OnBuiltClick;   // Bina sağlamsa çalışacak (Örn: Asker panelini aç)
    public UnityEvent<BuildingClickable> OnRepairClick; // Yıkıksa çalışacak (Tamir panelini aç)

    void Start()
    {
        // 1. Senin eski renk kodların
        _renderer = GetComponentInChildren<Renderer>(); // Children yaptık ki model değişince renderer kaybolmasın
        if (_renderer != null) _originalColor = _renderer.material.color;

        // 2. Başlangıçta binanın şeklini ayarla
        UpdateVisuals();
    }

    // --- SENİN ESKİ MOUSE KODLARIN (Aynen duruyor) ---
    void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (_renderer != null) _renderer.material.color = highlightColor;
    }

    void OnMouseExit()
    {
        if (_renderer != null) _renderer.material.color = _originalColor;
    }

    public void OnMouseDown()
    {
        // UI'a tıklıyorsak işlemi iptal et (Senin kodun)
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Debug.Log($"{buildingName} binasına tıklandı. Durum: {currentState}");

        // --- YENİ MANTIK ---
        switch (currentState)
        {
            case BuildingState.Ruined:
                // Bina yıkıksa "Beni Tamir Et" panelini açması için Manager'a haber ver
                // Kendisini (this) parametre olarak gönderiyor ki hangi bina olduğunu bilelim
                OnRepairClick?.Invoke(this); 
                break;

            case BuildingState.Built:
                // Bina sağlamsa senin eski OnClick eventini çalıştır
                OnBuiltClick?.Invoke();
                break;
            
            case BuildingState.Locked:
                Debug.Log("Bu bina henüz kilitli!");
                break;
        }
    }

    // --- YENİ EKLENEN FONKSİYONLAR ---

    // Duruma göre 3D modelleri açıp kapatan fonksiyon
    public void UpdateVisuals()
    {
        if (ruinedModel != null) ruinedModel.SetActive(currentState == BuildingState.Ruined);
        if (builtModel != null) builtModel.SetActive(currentState == BuildingState.Built);
        
        // Model değiştiği için renderer'ı yeniden bulmamız gerekebilir
        // (Renk değişimi doğru çalışsın diye)
        var activeModel = (currentState == BuildingState.Built) ? builtModel : ruinedModel;
        if (activeModel != null)
        {
            _renderer = activeModel.GetComponent<Renderer>();
            if (_renderer != null) _originalColor = _renderer.material.color;
        }
    }

    // Dışarıdan (UI butonundan) çağrılacak tamir fonksiyonu
    public void RepairBuilding()
    {
        // Parayı kontrol et (MoneyManager örneği)
        if (MoneyManager.Instance != null && MoneyManager.Instance.gold >= repairCost)
        {
            MoneyManager.Instance.Spend(repairCost);
            
            // Durumu değiştir
            currentState = BuildingState.Built;
            
            // Görseli güncelle
            UpdateVisuals();
            
            Debug.Log("Bina tamir edildi!");
        }
        else
        {
            Debug.Log("Para yetersiz!");
        }
    }
}