using UnityEngine;
using System.Collections.Generic;

public class RecruitManager : MonoBehaviour
{
    public static RecruitManager Instance;

    [Header("Ayarlar")]
    public GameObject soldierPrefab; // Sahneye yaratılacak asker (Gladiator scriptli)
    public Transform soldierSpawnPoint; // Satın alınınca nerede doğsun? (Kampın ortası)
    
    [Header("Aday Havuzu")]
    public List<RecruitCandidate> dailyCandidates = new List<RecruitCandidate>();
    public int dailyCandidateCount = 3; // Her gün kaç yeni çocuk gelsin?

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateCandidates(); // Oyun başında adayları oluştur
        
        // Eğer DayManager varsa, her yeni günde listeyi yenilemek için abone ol
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay += GenerateCandidates;
    }

    // Her gün vitrini yeniler
   // RecruitManager.cs içindeki GenerateCandidates fonksiyonunu şu şekilde değiştir:

    public void GenerateCandidates()
    {
        dailyCandidates.Clear();
        for (int i = 0; i < dailyCandidateCount; i++)
        {
            // 1. Boş bir aday oluştur
            RecruitCandidate newGuy = new RecruitCandidate();
            
            // 2. Rastgele özelliklerini ŞİMDİ ata (Oyun başladığı için güvenli)
            newGuy.InitializeRandomly();
            
            // 3. Listeye ekle
            dailyCandidates.Add(newGuy);
        }
        
        // Eğer UI açıksa anlık yenilemesi için (Opsiyonel, UI kodun hata vermez)
        // FindObjectOfType<RecruitUIManager>()?.RefreshList();
    }
    public void RecruitSoldier(RecruitCandidate candidate)
    {
        // 1. KONTROL: Para var mı?
        if (MoneyManager.Instance.gold < candidate.cost)
        {
            Debug.Log("<color=red>Para yetersiz!</color>");
            return;
        }

        // 2. KONTROL: Kışlada yer var mı? (CampManager entegrasyonu)
        // Sahnedeki mevcut askerleri sayıyoruz
        int currentCount = FindObjectsOfType<Gladiator>().Length;
        int maxCap = CampManager.Instance.GetMaxSoldierCapacity();

        if (currentCount >= maxCap)
        {
            Debug.Log($"<color=red>KIŞLA DOLU! ({currentCount}/{maxCap})</color> Önce koğuşu genişlet!");
            return;
        }

        // --- İŞLEM BAŞLIYOR ---

        // A. Parayı düş
        MoneyManager.Instance.Spend(candidate.cost);

        // B. Askeri Sahnede Yarat
        GameObject newObj = Instantiate(soldierPrefab, soldierSpawnPoint.position, Quaternion.identity);
        
        // C. Askerin verilerini (Stats) adaya göre ayarla
        Gladiator glad = newObj.GetComponent<Gladiator>();
        
        // DİKKAT: ScriptableObject'i kopyalamazsak hepsi aynı veriyi kullanır!
        // Bu yüzden yeni bir instance (kopya) oluşturuyoruz.
        glad.data = Instantiate(glad.data); 
        
        glad.data.gladiatorName = candidate.candidateName;
        glad.data.strength = candidate.potentialStrength;
        glad.data.stamina = candidate.potentialStamina;
        glad.data.level = 1; // Herkes acemi başlar

        // D. Listeden çıkar (Artık alındı)
        dailyCandidates.Remove(candidate);

        Debug.Log("Yeni bir Acemi Oğlanı ocağa katıldı: " + candidate.candidateName);
    }
}