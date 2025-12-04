using UnityEngine;

public class GladiatorSelector : MonoBehaviour
{
    public Camera cam;
    public GladiatorPanel panel;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
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
