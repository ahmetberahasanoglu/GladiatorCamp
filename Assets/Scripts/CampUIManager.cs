using UnityEngine;

public class CampUIManager : MonoBehaviour
{
    public GameObject campPanel;      // Açılıp kapanan panel
    public Transform contentParent;   // Scroll View -> Content
    public BuildingUI buildingPrefab; // Az önce yazdığımız kartın prefabı

    // Butona basınca bu çalışacak
    public void TogglePanel()
    {
        bool isOpen = !campPanel.activeSelf;
        campPanel.SetActive(isOpen);

        if (isOpen)
        {
            RefreshList();
        }
    }

    void RefreshList()
    {
        // Eskileri temizle
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        // Manager'daki binaları listele
        foreach (var building in CampManager.Instance.buildings)
        {
            var ui = Instantiate(buildingPrefab, contentParent);
            ui.Setup(building);
        }
    }
}