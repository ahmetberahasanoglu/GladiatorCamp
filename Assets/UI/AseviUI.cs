using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AseviUI : MonoBehaviour
{
    [Header("UI Elemanları")]
    public GameObject panelObj; // Panelin kendisi
    public TextMeshProUGUI foodText;  // "Erzak: 150"
    public TextMeshProUGUI infoText;  // "Günlük Tüketim: 12"
    
    // Satın Alma Butonları (Inspector'dan OnClick ile BuyFood çağıracağız)
    // Script içinden de bağlayabilirsin ama Inspector daha pratik olabilir.

    void Start()
    {
        panelObj.SetActive(false);
        
        // Manager'daki değişimleri dinle
        if (SupplyManager.Instance != null)
            SupplyManager.Instance.OnFoodChanged += RefreshUI;
    }

    void OnDestroy()
    {
        if (SupplyManager.Instance != null)
            SupplyManager.Instance.OnFoodChanged -= RefreshUI;
    }

    // Bina Tıklayınca Bu Çağırılacak
    public void TogglePanel()
    {
        bool isActive = !panelObj.activeSelf;
        panelObj.SetActive(isActive);

        if (isActive) RefreshUI();
    }

    void RefreshUI()
    {
        if (SupplyManager.Instance == null) return;

        int food = SupplyManager.Instance.currentFood;
        foodText.text = $"Kilerdeki Erzak: {food}";

        // Bilgilendirme: Oyuncuya kaç askeri olduğunu ve ne kadar yediklerini hatırlat
        int soldierCount = FindObjectsOfType<Gladiator>().Length;
        int dailyCost = soldierCount * SupplyManager.Instance.foodPerSoldier;
        
        if(food < dailyCost)
            infoText.text = $"<color=red>DİKKAT: Yarına yetecek yemek yok!\nGünlük Tüketim: {dailyCost}</color>";
        else
            infoText.text = $"Günlük Tüketim: {dailyCost} birim";
    }

    // Butonlara bağlamak için yardımcı fonksiyonlar
    public void Buy10Food() => SupplyManager.Instance.BuyFood(10);
    public void Buy50Food() => SupplyManager.Instance.BuyFood(50);
}