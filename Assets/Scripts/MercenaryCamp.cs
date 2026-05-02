using UnityEngine;
using UnityEngine.EventSystems;

public class MercenaryCamp : MonoBehaviour
{
    public int mercenaryCost = 100;
    public GameObject mercenaryPrefab; 
    public Transform spawnPoint;

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Ekrana panel açabilirsin ama hızlıca yapalım:
        if (MoneyManager.Instance.gold >= mercenaryCost)
        {
            MoneyManager.Instance.Spend(mercenaryCost);
            
            GameObject newSoldier = Instantiate(mercenaryPrefab, spawnPoint.position, Quaternion.identity);
            Gladiator glad = newSoldier.GetComponent<Gladiator>();
            if (glad != null && glad.data != null)
            {
                glad.data.gladiatorName = "Başıbozuk Asker";
                glad.data.level = 1;
            }

            NotificationManager.Instance.Show("-100 Akçe. Başıbozuk bir paralı asker saflarına katıldı!", NotificationType.Success);
            
            if (TopInfoBarUI.Instance != null) TopInfoBarUI.Instance.UpdateCapacity();
        }
        else
        {
            NotificationManager.Instance.Show($"Paralı asker kiralamak için {mercenaryCost} Akçen yok!", NotificationType.Error);
        }
    }
}