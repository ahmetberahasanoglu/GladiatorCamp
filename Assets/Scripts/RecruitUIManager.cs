using UnityEngine;

public class RecruitUIManager : MonoBehaviour
{
    public Transform contentParent;
    public RecruitSlotUI slotPrefab;
    public GameObject recruitPanel;

    void Start()
    {
        recruitPanel.SetActive(false);
    }

    // Butona basınca paneli açan fonksiyon
    public void TogglePanel()
    {
        recruitPanel.SetActive(!recruitPanel.activeSelf);
        if (recruitPanel.activeSelf)
        {
            RefreshList();
        }
    }

    void RefreshList()
    {
        // Temizle
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        // Manager'daki listeyi çiz
        foreach (var candidate in RecruitManager.Instance.dailyCandidates)
        {
            var ui = Instantiate(slotPrefab, contentParent);
            // Setup içine "Yenileme" fonksiyonunu da gönderiyoruz
            ui.Setup(candidate, RefreshList); 
        }
    }
}