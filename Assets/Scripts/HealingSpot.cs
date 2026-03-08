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

        // Seçili askeri bul (Aynı yöneticiyi kullanıyoruz, çok pratik!)
        GladiatorTraining currentGladiator = TrainingUIManager.Instance.current;
        
        if (currentGladiator == null)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Önce yaralı bir asker seçmelisin!", NotificationType.Warning);
            return;
        }

        // İSTEĞE BAĞLI: Askerin canı zaten full ise engelleme eklenebilir.
        // if (currentGladiator.GetComponent<Gladiator>().data.currentHp >= currentGladiator.GetComponent<Gladiator>().data.maxHp) ...

        if (currentGladiator.IsTraining)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Bu asker şu an başka bir talimde!", NotificationType.Warning);
            return;
        }

        // Parayı Kontrol Et ve Kes
        if (MoneyManager.Instance.Spend(cost))
        {
            // Askerin üstündeki Healing componentini çağır
            GladiatorHealing healingComp = currentGladiator.GetComponent<GladiatorHealing>();
            if (healingComp != null)
            {
                healingComp.StartHealing(this);
                
                // Seçimi temizle (Başka yere tıklamasın diye)
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