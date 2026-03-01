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
    private Renderer _renderer; 
    private Color _originalColor;

    // --- YENİ EKLENEN KISIM: EFEKT ---
    [Header("Efektler (Juiciness)")]
    public GameObject buildEffectPrefab; // İnşaat toz bulutu / parlaması
    public Vector3 effectOffset = new Vector3(0, 2f, 0); // Efektin yerden ne kadar yukarıda çıkacağı

    [Header("Olaylar")]
    public UnityEvent OnBuiltClick;   
    public UnityEvent<BuildingClickable> OnRepairClick; 

    void Start()
    {
        UpdateVisuals();
    }

    // --- MOUSE ETKİLEŞİMLERİ ---
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
        if (EventSystem.current.IsPointerOverGameObject()) return;
        NotificationManager.Instance.Show($"{buildingName} tıklandı. ", NotificationType.Info);

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
             NotificationManager.Instance.Show("Bu bina henüz kilitli!", NotificationType.Info);
                break;
        }
    }

    public void UpdateVisuals()
    {
        if (ruinedModel != null) ruinedModel.SetActive(currentState == BuildingState.Ruined);
        if (builtModel != null) builtModel.SetActive(currentState == BuildingState.Built);
        
        GameObject activeModel = null;
        if (currentState == BuildingState.Built) activeModel = builtModel;
        else if (currentState == BuildingState.Ruined) activeModel = ruinedModel;

        if (activeModel != null)
        {
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
        if (MoneyManager.Instance != null && MoneyManager.Instance.gold >= repairCost)
        {
            MoneyManager.Instance.Spend(repairCost); 
            
            currentState = BuildingState.Built;
            UpdateVisuals(); 

            // --- YENİ EKLENEN KISIM: EFEKTİ YARAT ---
            if (buildEffectPrefab != null)
            {
                // Efekti binanın merkezinden biraz yukarıda oluştur (Offset ile)
                Vector3 spawnPos = transform.position + effectOffset;
                GameObject vfx = Instantiate(buildEffectPrefab, spawnPos, Quaternion.identity);
                
                // Oyunu kastırmaması için 3 saniye sonra sil
                Destroy(vfx, 3f);
            }
            // ----------------------------------------

            NotificationManager.Instance.Show("Bina tamir edildi!", NotificationType.Info);
        }
        else
        {
            NotificationManager.Instance.Show("Hazine tam takır! Yeterli akçe yok.", NotificationType.Error);
        }
    }
}