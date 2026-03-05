using UnityEngine;
using UnityEngine.EventSystems;

public class GladiatorSelector : MonoBehaviour
{
    public static GladiatorSelector Instance;
    public Camera cam;
    
    private GladiatorInventory selectedInventory;
    private GameObject currentlyOpenLocalUI;

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
    }

    public void DeselectIfDead(GameObject deadSoldier)
    {
        if (selectedInventory != null && selectedInventory.gameObject == deadSoldier)
        {
            ClearSelection();
        }
    }

    public void OpenSelectedInventory()
    {
        if (selectedInventory != null)
        {
            InventoryUIManager.Instance.OpenInventoryFor(selectedInventory);
        }
        else
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Önce bir asker seçmelisin!", NotificationType.Warning);
        }
    }

    void Update()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle)
        {
            return;
        }
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out Gladiator gladiator))
                {
                    // 1. Eğitim yöneticisine askeri bildir
                    var training = gladiator.GetComponent<GladiatorTraining>();
                    if (TrainingUIManager.Instance != null)
                    {
                        TrainingUIManager.Instance.SetCurrentGladiator(training);
                    }

                    // 2. Envanteri hafızaya al
                    selectedInventory = gladiator.GetComponent<GladiatorInventory>();

                    // 3. KART SİSTEMİ
                    if (currentlyOpenLocalUI != null)
                    {
                        currentlyOpenLocalUI.SetActive(false);
                    }

                    GladiatorPanel localPanel = gladiator.GetComponentInChildren<GladiatorPanel>(true);
                    
                    if (localPanel != null)
                    {
                        localPanel.gameObject.SetActive(true); 
                        currentlyOpenLocalUI = localPanel.gameObject; 
                        localPanel.ShowInfo(gladiator); 
                    }
                    else
                    {
                        Debug.LogWarning($"{gladiator.name} içinde 'GladiatorPanel' scripti olan bir Panel bulunamadı!");
                    }
                }
                else
                {
                    // --- YENİ: Yere, binalara veya asker olmayan herhangi bir objeye tıklandıysa! ---
                    ClearSelection();
                }
            }
            else
            {
                // --- GÖKYÜZÜNE (Tamamen boşluğa) tıklandıysa ---
                ClearSelection();
            }
        }
    }

    // Kod tekrarını önlemek için temizlik işini tek bir fonksiyona topladık
    public void ClearSelection()
    {
        selectedInventory = null;

        if (currentlyOpenLocalUI != null)
        {
            currentlyOpenLocalUI.SetActive(false);
            currentlyOpenLocalUI = null;
        }
    }
}