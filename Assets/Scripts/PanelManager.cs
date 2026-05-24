using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tüm panellerin tek bir noktadan yönetilmesini sağlar.
/// Yeni bir panel açılmadan önce mevcut açık panel kapanır.
/// 
/// Kullanım:
///   // Panel açmak için:
///   PanelManager.Instance.OpenPanel(demirciPanel, "Demirci");
///   
///   // Panel kapatmak için:
///   PanelManager.Instance.CloseCurrentPanel();
///   // veya direkt:
///   PanelManager.Instance.ClosePanel(demirciPanel);
/// </summary>
public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    // Şu an açık olan panel ve adı
    private GameObject _currentPanel;
    private string     _currentPanelName;

    // Üst üste açılabilecek istisnai paneller (örn: tooltip, notification)
    private readonly HashSet<string> _overlayExceptions = new HashSet<string>
    {
        "Tooltip", "Notification", "Confirmation", "GaziFeedback",
        "RelicSelection", "ExpeditionSummary", "ExilePanel", "VictoryPanel"
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ── PANEL AÇ ──────────────────────────────────────────────────────────
    /// <summary>
    /// Yeni panel aç. Eğer başka bir panel açıksa önce onu kapat.
    /// panelName: istisna listesi için kullanılır.
    /// </summary>
    public void OpenPanel(GameObject panel, string panelName = "")
    {
        if (panel == null) return;

        // Zaten açık olan aynı panel ise kapat (toggle)
        if (_currentPanel == panel)
        {
            CloseCurrentPanel();
            return;
        }

        // İstisna paneli değilse mevcut açık olanı kapat
        if (!_overlayExceptions.Contains(panelName) && _currentPanel != null)
        {
            CloseCurrentPanel();
        }

        _currentPanel     = panel;
        _currentPanelName = panelName;
        panel.SetActive(true);

        Debug.Log($"[PanelManager] Açıldı: {panelName}");
    }

    // ── PANEL KAPAT ───────────────────────────────────────────────────────
    public void CloseCurrentPanel()
    {
        if (_currentPanel == null) return;

        _currentPanel.SetActive(false);
        Debug.Log($"[PanelManager] Kapandı: {_currentPanelName}");

        _currentPanel     = null;
        _currentPanelName = "";
    }

    public void ClosePanel(GameObject panel)
    {
        if (panel == null) return;
        panel.SetActive(false);

        if (_currentPanel == panel)
        {
            _currentPanel     = null;
            _currentPanelName = "";
        }
    }

    // ── TÜM PANELLERİ KAPAT ───────────────────────────────────────────────
    /// <summary>
    /// Harita açılırken veya savaş başlarken tüm panelleri temizle.
    /// </summary>
    public void CloseAll()
    {
        CloseCurrentPanel();
    }

    // ── DURUM SORGULAMA ───────────────────────────────────────────────────
    public bool IsAnyPanelOpen() => _currentPanel != null && _currentPanel.activeSelf;

    public bool IsPanelOpen(GameObject panel) => panel != null && panel.activeSelf;
}