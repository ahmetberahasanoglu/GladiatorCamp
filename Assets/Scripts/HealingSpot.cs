using UnityEngine;
using TMPro;

public class HealingSpot : MonoBehaviour
{
    public Transform healingPoint; // Askerin yürüyeceği nokta
    
    [Header("Şifa Ayarları")]
    public int extraHealAmount = 50; // Pasif 20'nin üstüne eklenecek şifa
    public int cost = 15; // İyileştirme ücreti
    public int timeCost = 1; // Kaç gün sürecek?
    public bool isBusy = false;

    [Header("Görsel Geri Bildirim")]
    public GameObject hoverTextObj; 
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseEnter()
    {
        if (isBusy) return;

        transform.localScale = hoverScale;
        if (hoverTextObj != null) 
        {
            hoverTextObj.SetActive(true);
            hoverTextObj.GetComponentInChildren<TextMeshPro>().text = $"Şifacı\n({cost} Akçe)";
        }
    }

    void OnMouseExit()
    {
        transform.localScale = originalScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseDown()
    {
        if (isBusy) 
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Hekimbaşı şu an meşgul!", NotificationType.Warning);
            return;
        }

        GladiatorTraining currentGladiatorComp = TrainingUIManager.Instance.current;
        
        if (currentGladiatorComp == null)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Önce yaralı bir asker seçmelisin!", NotificationType.Warning);
            return;
        }

        // --- YENİ BARRİYER: TEK DOĞRU KAYNAK KONTROLÜ ---
        Gladiator gladiator = currentGladiatorComp.GetComponent<Gladiator>();
        
        if (!gladiator.IsAvailableForTask())
        {
            if (NotificationManager.Instance != null) 
                NotificationManager.Instance.Show("Bu asker şu an başka bir görevle meşgul!", NotificationType.Warning);
            return;
        }

        // Parayı Kontrol Et ve Kes
        if (MoneyManager.Instance.Spend(cost))
        {
            GladiatorHealing healingComp = currentGladiatorComp.GetComponent<GladiatorHealing>();
            if (healingComp != null)
            {
                // YENİ: Askerin durumunu "Şifahanede" olarak kilitle!
                gladiator.SetActivity(SoldierActivity.Healing);

                healingComp.StartHealing(this);
                
                TrainingUIManager.Instance.SetCurrentGladiator(null);
                OnMouseExit();
            }
        }
        else
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Tedavi için yeterli akçen yok!", NotificationType.Error);
        }
    }
}