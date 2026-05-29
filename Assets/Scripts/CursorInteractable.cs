using UnityEngine;
using UnityEngine.EventSystems; // UI Tıklamalarını kontrol etmek için gerekli

public class CursorInteractable : MonoBehaviour
{
    [Header("Bu obje bir düşman mı?")]
    public bool isEnemy = false;

    void OnMouseEnter()
    {

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;
        // ───────────────────────────────────────────────────

        if (CursorManager.Instance == null) return;

        if (isEnemy)
            CursorManager.Instance.SetAttackCursor();
        else
            CursorManager.Instance.SetInteractCursor();
    }

    void OnMouseExit()
    {
        // Fare objeden çıkınca imleci normale döndür
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormalCursor();
    }
}