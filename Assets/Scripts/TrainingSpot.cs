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

    // Fareyle objeye TIKLANDIĞINDA çalışır
    void OnMouseDown()
    {
        if (isBusy) 
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Bu alan şu an dolu!", NotificationType.Warning);
            return;
        }

        // 1. KONTROL: Seçili bir gladyatör var mı?
        GladiatorTraining currentGladiator = TrainingUIManager.Instance.current;
        
        // Eğer hafızada seçili asker yoksa direkt uyarı ver ve işlemi durdur
        if (currentGladiator == null)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Önce bir asker seçmelisin!", NotificationType.Warning);
            return;
        }

      // Asker eğitimde mi?
        if (currentGladiator.IsTraining)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Bu asker zaten eğitimde!", NotificationType.Warning);
            return;
        }


        if (currentGladiator.GetComponent<GladiatorHealing>() != null && currentGladiator.GetComponent<GladiatorHealing>().IsHealing)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Bu asker şu an şifahanede, eğitim yapamaz!", NotificationType.Warning);
            return;
        }


        // 2. KONTROL: Paramız yetiyor mu?
        if (MoneyManager.Instance.Spend(MoneyManager.Instance.trainingCost))
        {
            // Askeri hedefe gönder
            currentGladiator.StartTraining(this);
            
            // --- KRİTİK ÇÖZÜM: SEÇİMİ TEMİZLE ---
            // Askeri göreve gönderdikten sonra hafızadan siliyoruz ki
            // askeri seçmeden başka bir hedefe tıkladığımızda eski asker gelmesin!
            TrainingUIManager.Instance.SetCurrentGladiator(null);
            // ------------------------------------

            OnMouseExit(); // Tıklayınca yazıyı ve büyümeyi kapat
        }
        else
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Eğitim için yeterli akçen yok!", NotificationType.Error);
        }
    }
}