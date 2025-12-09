using UnityEngine;

public class GladiatorSelector : MonoBehaviour
{
    public Camera cam;
    public GladiatorPanel panel;
void Start()
    {
        // 1. KAMERAYI BUL
        // Eğer kamera atanmamışsa, sahnedeki "MainCamera" etiketli kamerayı bulur.
        if (cam == null)
        {
            cam = Camera.main;
        }

        // 2. PANELİ BUL
        // Eğer panel atanmamışsa, sahnedeki GladiatorPanel scriptini arar.
        // 'true' parametresini ekliyoruz ki panel kapalı (inactive) olsa bile bulsun.
        if (panel == null)
        {
            panel = FindObjectOfType<GladiatorPanel>(true);
        }
        
        // Hata ayıklama: Hala bulamadıysa uyar
        if (cam == null) Debug.LogError("GladiatorSelector: Kamera bulunamadı! Kameranın tag'i 'MainCamera' mı?");
        if (panel == null) Debug.LogError("GladiatorSelector: GladiatorPanel sahnede bulunamadı!");
    }
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
