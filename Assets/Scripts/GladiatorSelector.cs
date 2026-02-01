using UnityEngine;
using UnityEngine.EventSystems;
public class GladiatorSelector : MonoBehaviour
{
    public Camera cam;
    public GladiatorPanel panel;
void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        if (panel == null)
        {
            panel = FindObjectOfType<GladiatorPanel>(true);
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out Gladiator gladiator))
                {
                    panel.ShowInfo(gladiator);

                    var training = gladiator.GetComponent<GladiatorTraining>();
                    TrainingUIManager.Instance.SetCurrentGladiator(training);

                    var inventory = gladiator.GetComponent<GladiatorInventory>();
                    InventoryUIManager.Instance.OpenInventoryFor(inventory);
                }
            }
        }
    }
}
