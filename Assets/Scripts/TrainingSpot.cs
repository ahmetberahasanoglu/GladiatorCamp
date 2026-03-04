using UnityEngine;
using TMPro;

public enum TrainingType { Strength, Speed, Defense, Morale, Stamina }

public class TrainingSpot : MonoBehaviour
{
    public Transform trainingPoint;
    public TrainingType trainingType = TrainingType.Strength;
    public int statGain = 1;
    public bool isBusy = false;

    [Header("Görsel Geri Bildirim")]
    public GameObject hoverTextObj; // Üzerine gelince çıkacak olan yazı ("Strength Talimi (30 Akçe)")
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f); // Fareyle üzerine gelince ne kadar büyüsün?
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        
        // Başlangıçta bilgi yazısını gizle
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    // Fare objenin ÜZERİNE GELDİĞİNDE çalışır
    void OnMouseEnter()
    {
        if (isBusy) return; // Obje doluysa büyümesin

        transform.localScale = hoverScale;

        if (hoverTextObj != null) 
        {
            hoverTextObj.SetActive(true);
            // Parayı sistemden çekip yazdırıyoruz
            hoverTextObj.GetComponentInChildren<TextMeshPro>().text = $"{trainingType} Eğitimi\n({MoneyManager.Instance.trainingCost} Akçe)";
        }
    }

    // Fare objenin ÜZERİNDEN ÇIKTIĞINDA çalışır
    void OnMouseExit()
    {
        transform.localScale = originalScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseDown()
    {
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;
        if (isBusy) 
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Bu alan şu an dolu!", NotificationType.Warning);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
            return;
        }

        // 1. KONTROL: Seçili bir gladyatör var mı?
        GladiatorTraining currentGladiator = TrainingUIManager.Instance.current;
        
        if (currentGladiator == null)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Önce bir asker seçmelisin!", NotificationType.Warning);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
            return;
        }

        if (currentGladiator.IsTraining)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Bu asker zaten eğitimde!", NotificationType.Warning);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
            return;
        }

        if (currentGladiator.GetComponent<GladiatorHealing>() != null && currentGladiator.GetComponent<GladiatorHealing>().IsHealing)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Bu asker şu an şifahanede, eğitim yapamaz!", NotificationType.Warning);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
            return;
        }

        // --- YENİ: BİNA SEVİYESİNE GÖRE MAKSİMUM STAT KONTROLÜ ---
        int maxAllowedStat = CampManager.Instance != null ? CampManager.Instance.GetBuildingValue("talimhane") : 15;
        int currentStatValue = 0;
        
        // Askerin mevcut stat değerini bul
        JanissaryData data = currentGladiator.GetComponent<Gladiator>().data;
        if (data != null)
        {
            switch (trainingType)
            {
                case TrainingType.Strength: currentStatValue = data.strength; break;
                case TrainingType.Defense: currentStatValue = data.defense; break;
                case TrainingType.Speed: currentStatValue = data.speed; break;
                case TrainingType.Stamina: currentStatValue = data.stamina; break;
                // Moral genellikle başka yollarla artar ama istersen onu da ekleyebilirsin
            }

            if (currentStatValue >= maxAllowedStat)
            {
                if (NotificationManager.Instance != null) 
                    NotificationManager.Instance.Show($"Asker sınırda! (Maks: {maxAllowedStat}). Daha fazlası için Talimhaneyi geliştirin.", NotificationType.Warning);
                
                if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
                return; // Eğitime gönderme, işlemi iptal et!
            }
        }
        // -----------------------------------------------------------

        // 2. KONTROL: Paramız yetiyor mu?
        if (MoneyManager.Instance.Spend(MoneyManager.Instance.trainingCost))
        {
            // Askeri hedefe gönder
            currentGladiator.StartTraining(this);
            
            // Eğitim başlama sesini çal
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
            
            TrainingUIManager.Instance.SetCurrentGladiator(null);

            OnMouseExit(); 
        }
        else
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Eğitim için yeterli akçen yok!", NotificationType.Error);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
        }
    }
}