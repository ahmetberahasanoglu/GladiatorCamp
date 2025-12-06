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
   
    public void LoadSoldierFromSave(SoldierSaveData savedData)
    {
       GameObject newObj = Instantiate(soldierPrefab, soldierSpawnPoint.position, Quaternion.identity);

        newObj.name = savedData.name;

        // 3. ZORLA AKTİF ET (Eğer prefab kapalıysa bile açılsın)
        newObj.SetActive(true);

        // 4. BİLEŞENLERİ KONTROL ET VE AÇ
        var gladComponent = newObj.GetComponent<Gladiator>();
        var aiComponent = newObj.GetComponent<GladiatorAI>();
        var animatorComponent = newObj.GetComponent<Animator>();
        var selectorComponent = newObj.GetComponent<GladiatorSelector>();
        var inventoryComponent = newObj.GetComponent<GladiatorInventory>();
        var trainingComponent = newObj.GetComponent<GladiatorTraining>();
        var agentComponent = newObj.GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (gladComponent != null) gladComponent.enabled = true;
        if (aiComponent != null) aiComponent.enabled = true;
        if (animatorComponent != null) animatorComponent.enabled = true;
        if (selectorComponent != null) selectorComponent.enabled = true;
        if (inventoryComponent != null) inventoryComponent.enabled = true;
        if (trainingComponent != null) trainingComponent.enabled = true;
        
        
        // Güvenli yerleştirme için Warp 
        if (agentComponent != null)
        {
            agentComponent.enabled = true;
            agentComponent.Warp(soldierSpawnPoint.position);
        }


        JanissaryData newData = ScriptableObject.CreateInstance<JanissaryData>();
        newData.gladiatorName = savedData.name;
        newData.strength = savedData.strength;
        newData.stamina = savedData.stamina;
        newData.defense = savedData.defense;
        newData.speed = savedData.speed;
        newData.morale = savedData.morale;
        newData.level = savedData.level;

        gladComponent.data = newData;
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
 
        MoneyManager.Instance.Spend(candidate.cost);

        GameObject newObj = Instantiate(soldierPrefab, soldierSpawnPoint.position, Quaternion.identity);
        Gladiator glad = newObj.GetComponent<Gladiator>();
        JanissaryData newData = ScriptableObject.CreateInstance<JanissaryData>();
        newData.gladiatorName = candidate.candidateName;
        newData.strength = candidate.potentialStrength;
        newData.stamina = candidate.potentialStamina;
        newData.defense = 1; 
        newData.speed = 1;
        newData.morale = 100;
        newData.level = 1;

    
        glad.data = newData;
        dailyCandidates.Remove(candidate);

        Debug.Log("Yeni bir Acemi Oğlanı ocağa katıldı: " + candidate.candidateName);
    }
}