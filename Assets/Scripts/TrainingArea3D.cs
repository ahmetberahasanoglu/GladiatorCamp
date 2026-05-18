using UnityEngine;
using TMPro;

public class TrainingArea3D : MonoBehaviour
{
    [Header("Eğitim Ayarları")]
    public string trainingName = "Okçuluk Talimi";
    public int cost = 50; // Eğitim ücreti
    public int timeCost = 1; // Kaç gün sürecek?

    [Header("Görsel Geri Bildirim")]
    public GameObject hoverTextObj; // Üzerine gelince çıkacak olan yazı ("Okçuluk Talimi (30 Akçe)")
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f); // Fareyle üzerine gelince ne kadar büyüsün?
    private Vector3 originalScale;

    // Eğitim tipleri için (Eski TrainingUIManager'daki fonksiyonlarını çağırmak için)
    public enum TrainingType { Archery, Melee, HorseRiding }
    public TrainingType trainingType;

    void Start()
    {
        originalScale = transform.localScale;
        
        // Başlangıçta yazıyı gizle
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    // Fare objenin ÜZERİNE GELDİĞİNDE çalışır
    void OnMouseEnter()
    {
        // 1. Obje hafifçe büyüsün (Tıklanabilir hissi)
        transform.localScale = hoverScale;

        // 2. Bilgi yazısı açılsın
        if (hoverTextObj != null) 
        {
            hoverTextObj.SetActive(true);
            hoverTextObj.GetComponentInChildren<TextMeshPro>().text = $"{trainingName}\n({cost} Akçe)";
        }
    }

    // Fare objenin ÜZERİNDEN ÇIKTIĞINDA çalışır
    void OnMouseExit()
    {
        // Obje ve yazılar eski haline dönsün
        transform.localScale = originalScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    // Fareyle objeye TIKLANDIĞINDA çalışır
    void OnMouseDown()
    {   
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;
        // KONTROL: Paramız yetiyor mu?
        if (MoneyManager.Instance.gold >= cost)
        {
            // 1. Parayı kes
            MoneyManager.Instance.Spend(cost);
            
            // 2. Zamanı geçir
            if(DayManager.Instance != null) DayManager.Instance.NextDay(timeCost);

            // 3. İlgili eğitime gönder
            ExecuteTraining();
        }
        else
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Talim için yeterli akçen yok!", NotificationType.Error);
        }
    }

    void ExecuteTraining()
    {
        // Seçilen eğitim tipine göre sahneye veya fonksiyona yolla
        switch (trainingType)
        {
            case TrainingType.Archery:
                Debug.Log("Okçuluk eğitimine gidiliyor...");
               // UnityEngine.SceneManagement.SceneManager.LoadScene("ArcheryMiniGame"); 
                break;
            case TrainingType.Melee:
                Debug.Log("Kılıç eğitimine gidiliyor...");
                // Kılıç talimi kodunu buraya yaz
                break;
            case TrainingType.HorseRiding:
                Debug.Log("At binme eğitimine gidiliyor...");
                // At yarışı kodunu buraya yaz
                break;
        }
    }
}