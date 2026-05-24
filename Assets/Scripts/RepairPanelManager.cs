using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RepairPanelManager : MonoBehaviour
{
    public static RepairPanelManager Instance;

    [Header("Panel Bileşenleri")]
    public GameObject panelObj;          // Tüm paneli açıp kapatmak için
    public TextMeshProUGUI titleText;    // "Kışla (Harabe)"
    public TextMeshProUGUI descText;     // "Asker üretimi sağlar..."
    public TextMeshProUGUI costText;     // "1000 Akçe"
    
    [Header("Butonlar")]
    public Button repairButton;
    public Button closeButton;
    
    // Şu an hangi binayı tamir etmeye çalışıyoruz?
    private BuildingClickable _currentTarget;

    void Awake()
    {
        Instance = this;
        panelObj.SetActive(false); // Başlangıçta kapalı olsun
    }

    void Start()
    {
        // Butonlara görevlerini verelim
        repairButton.onClick.AddListener(OnRepairClicked);
        closeButton.onClick.AddListener(ClosePanel);
    }

    // Bu fonksiyonu BuildingClickable scriptinden çağıracağız
    public void OpenPanel(BuildingClickable building)
    {
        if (panelObj == null) return;

        // Önce mevcut açık paneli kapat
        PanelManager.Instance.OpenPanel(panelObj, "Repair");
        _currentTarget = building; // Hedefi hafızaya al

        // UI Yazılarını Güncelle
        titleText.text = building.buildingName + " (Harabe)";
        
        // Açıklama varsa yaz, yoksa varsayılan bir şey yaz
        if (!string.IsNullOrEmpty(building.description))
            descText.text = building.description;
        else
            descText.text = "Bu bina kullanılamaz durumda. Onarılması gerekiyor.";

        // Fiyatı Yaz (Renkli)
        costText.text = $"Onarım Bedeli: <color=yellow>{building.repairCost} Akçe</color>";

        // Eğer paramız yetmiyorsa butonu gri yapabiliriz (Opsiyonel)
        bool canAfford = MoneyManager.Instance.gold >= building.repairCost;
        repairButton.interactable = canAfford; 
        // PanelManager.OpenPanel zaten SetActive(true) yapıyor
    }

    void OnRepairClicked()
    {
        if (_currentTarget != null)
        {
            _currentTarget.StartRepair();
            ClosePanel();
        }
    }

    public void ClosePanel()
    {
        PanelManager.Instance.ClosePanel(panelObj);
        _currentTarget = null;
    }
}