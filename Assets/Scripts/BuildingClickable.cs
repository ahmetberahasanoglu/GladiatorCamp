using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

// YENİ: UnderConstruction (İnşa Halinde) durumu eklendi!
public enum BuildingState { Locked, Ruined, UnderConstruction, Built }

public class BuildingClickable : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    public string buildingName;
    [TextArea] public string description;
    public BuildingState currentState = BuildingState.Built;
    public int repairCost = 1000;

    [Header("UI Bilgilendirme (3D Yazı)")]
    public TextMeshProUGUI statusText;

    [Header("İnşaat Süresi (Roguelite)")]
    public int requiredEncounters = 3; 
    public int currentRemainingEncounters = 0; 

    [Header("Görseller (Modeller)")]
    public GameObject ruinedModel; 
    public GameObject builtModel;  
    public GameObject constructionModel; // Opsiyonel: İnşaat halinde görünmesi için iskele modeli (Yoksa ruined kalır)

    [Header("Renk Değişimi")]
    public Color highlightColor = Color.yellow;
    private Renderer _renderer; 
    private Color _originalColor;

    [Header("Efektler (Juiciness)")]
    public GameObject buildEffectPrefab; 
    public Vector3 effectOffset = new Vector3(0, 2f, 0); 

    [Header("Olaylar")]
    public UnityEvent OnBuiltClick;   
    public UnityEvent<BuildingClickable> OnRepairClick; 

    void Start()
    {
        UpdateVisuals();
    }

    void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (_renderer != null) _renderer.material.color = highlightColor;
    }

    void OnMouseExit()
    {
        if (_renderer != null) _renderer.material.color = _originalColor;
    }
// BuildingClickable.cs içine ekle
    void OnEnable()
    {
        ExpeditionManager.OnEncounterAdvanced += AdvanceConstructionTimer;
    }

    void OnDisable()
    {
        ExpeditionManager.OnEncounterAdvanced -= AdvanceConstructionTimer;
    }

    // Fonksiyonu şu şekilde değiştir:
   
    public void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;
        
        switch (currentState)
        {
            case BuildingState.Ruined:
                if (RepairPanelManager.Instance != null)
                {
                    RepairPanelManager.Instance.OpenPanel(this);
                }
                break;

            case BuildingState.UnderConstruction:
                // YENİ: İnşaat halindeyken tıklanırsa kalan süreyi gösterir!
                NotificationManager.Instance.Show($"{buildingName} inşa ediliyor... Tamamlanmasına {currentRemainingEncounters} sefer (adım) kaldı.", NotificationType.Warning);
                break;

            case BuildingState.Built:
                NotificationManager.Instance.Show($"{buildingName} binasına girdin.", NotificationType.Info);
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
        if (constructionModel != null) constructionModel.SetActive(currentState == BuildingState.UnderConstruction);
        
        GameObject activeModel = null;
        if (currentState == BuildingState.Built) activeModel = builtModel;
        else if (currentState == BuildingState.Ruined) activeModel = ruinedModel;
        else if (currentState == BuildingState.UnderConstruction) 
        {
            activeModel = constructionModel != null ? constructionModel : ruinedModel; // İskele modeli yoksa harabeyi göster
        }

        if (activeModel != null)
        {
            _renderer = activeModel.GetComponentInChildren<Renderer>();
            if (_renderer != null) _originalColor = _renderer.material.color;
        }
        UpdateStatusText();
    }


    public void StartRepair()
    {
        if (MoneyManager.Instance != null && MoneyManager.Instance.gold >= repairCost)
        {
            MoneyManager.Instance.Spend(repairCost); 
            
            // İnşaatı Başlat
            currentState = BuildingState.UnderConstruction;
            currentRemainingEncounters = requiredEncounters;
            UpdateVisuals(); 

            NotificationManager.Instance.Show($"{buildingName} inşaatı başladı! {requiredEncounters} sefer adımından sonra hazır olacak.", NotificationType.Info);
        }
        else
        {
            NotificationManager.Instance.Show("Hazine tam takır! Yeterli akçe yok.", NotificationType.Error);
        }
    }
    private void UpdateStatusText()
    {
        if (statusText == null) return;

        switch (currentState)
        {
            case BuildingState.Locked:
                statusText.text = $"<color=#888888>{buildingName}\n<size=70%>(Kilitli)</size></color>";
                break;
            case BuildingState.Ruined:
                statusText.text = $"{buildingName}\n<size=70%>(Harabe)</size>";
                break;
            case BuildingState.UnderConstruction:
                statusText.text = $"<color=yellow>{buildingName}\n<size=70%>({currentRemainingEncounters} Sefer Kaldı)</size></color>";
                break;
            case BuildingState.Built:
                statusText.text = $"<color=white>{buildingName}</color>"; 
                break;
        }
    }

   
    public void AdvanceConstructionTimer()
    {
        if (currentState != BuildingState.UnderConstruction) return;

        currentRemainingEncounters--;

        if (currentRemainingEncounters <= 0)
        {
            currentState = BuildingState.Built;
            if (buildEffectPrefab != null)
            {
                Vector3 spawnPos = transform.position + effectOffset;
                Destroy(Instantiate(buildEffectPrefab, spawnPos, Quaternion.identity), 3f);
            }
            NotificationManager.Instance.Show($"Müjde! {buildingName} inşası tamamlandı!", NotificationType.Success);
        }
        
        UpdateVisuals(); 
    }
}