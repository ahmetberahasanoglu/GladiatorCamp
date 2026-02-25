using UnityEngine;
using UnityEngine.EventSystems;
public class GladiatorSelector : MonoBehaviour
{
    public static GladiatorSelector Instance;
    public Camera cam;
    public GladiatorPanel panel;
    private GladiatorInventory selectedInventory;
void Awake()
    {
        Instance = this;
    }

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
    public void OpenSelectedInventory()
    {
        if (selectedInventory != null)
        {
            // Eğer bir asker seçiliyse onun envanterini aç
            InventoryUIManager.Instance.OpenInventoryFor(selectedInventory);
        }
        else
        {
            // Asker seçili değilken butona basılırsa uyarı ver
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.Show("Önce bir asker seçmelisin!", NotificationType.Warning);
            }
            else
            {
                Debug.LogWarning("Asker seçili değil!");
            }
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Eğer UI (Panel, Buton vb.) üzerine tıkladıysak 3D Raycast atma
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out Gladiator gladiator))
                {
                    // 1. Bilgi panelini aç
                    panel.ShowInfo(gladiator);

                    // 2. Eğitim yöneticisine askeri bildir
                    var training = gladiator.GetComponent<GladiatorTraining>();
                    if (TrainingUIManager.Instance != null)
                    {
                        TrainingUIManager.Instance.SetCurrentGladiator(training);
                    }

                    // 3. ENVANTERİ AÇMA, SADECE HAFIZAYA AL
                    selectedInventory = gladiator.GetComponent<GladiatorInventory>();
                }
            }
            else
            {
                // Eğer boşluğa tıkladıysak seçili envanteri hafızadan sil
                selectedInventory = null;
            }
        }
    }
}
