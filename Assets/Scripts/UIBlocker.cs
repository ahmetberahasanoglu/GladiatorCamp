using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Tüm OnMouseDown kullanan 3D scriptler bu sınıfı çağırarak
/// önce UI'ın tıklamayı engelleyip engellemediğini kontrol eder.
/// 
/// Kullanım:
///   void OnMouseDown()
///   {
///       if (UIBlocker.IsPointerOverUI()) return;
///       // Normal işlemler...
///   }
/// </summary>
public static class UIBlocker
{
    /// <summary>
    /// Fare imleci bir UI elemanının üzerindeyse true döner.
    /// Mobil için dokunma desteği de vardır.
    /// </summary>
    public static bool IsPointerOverUI()
    {
        // Editör ve Standalone (fare)
        if (EventSystem.current == null) return false;

        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // Mobil — ilk dokunuş
        if (Input.touchCount > 0 &&
            EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return true;

        // Harita açıksa her zaman engelle
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen)
            return true;

        return false;
    }
}