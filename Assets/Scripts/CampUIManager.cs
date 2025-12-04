using UnityEngine;

public class CampUIManager : MonoBehaviour
{
    public Transform contentParent;
    public BuildingUI buildingPrefab;
    public GameObject campPanel; // Paneli açıp kapamak için

    void Start()
    {
        RefreshList();
        campPanel.SetActive(false); // Başta kapalı olsun
    }

    public void TogglePanel()
    {
        campPanel.SetActive(!campPanel.activeSelf);
        if(campPanel.activeSelf) RefreshList(); // Açılınca verileri tazele
    }

    void RefreshList()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        foreach (var b in CampManager.Instance.buildings)
        {
            var ui = Instantiate(buildingPrefab, contentParent);
            ui.Setup(b);
        }
    }
}