using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class WanderingStranger : MonoBehaviour
{
   private NavMeshAgent agent;
    private Animator anim; // YENİ: Animator referansı
    private Transform exitPoint;
    
    [Header("Durum")]
    public float waitTimeInCamp = 30f; 
    public float standUpTime = 2.0f; // YENİ: Ayağa kalkma animasyonu kaç saniye sürüyor?
    private bool isInteracting = false;
    private bool isLeaving = false;

    [Header("Teklif Detayları")]
    public string strangerName;
    [TextArea] public string offerText;
    public int cost;
    public int eventType; 

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 2.5f; 
        anim = GetComponentInChildren<Animator>(); // Animator'ı bul (Alt objede olma ihtimaline karşı)
    }

    public void Setup(Transform campPoint, Transform exit, int randomType)
    {
        exitPoint = exit;
        eventType = randomType;

        if (eventType == 0)
        {
            strangerName = "Gizemli Tüccar";
            offerText = "Uzak diyarlardan geldim. 50 Akçe karşılığında orduna geçici bir güç şerbeti verebilirim. İster misin?";
            cost = 50;
        }
        else
        {
            strangerName = "Yaralı Savaşçı";
            offerText = "Eski birliğim pusuya düştü. Yaralarımı sarman için 100 Akçe verirsen, kılıcım senindir Uç Beyi!";
            cost = 100;
        }

        // --- 1. YÜRÜYÜŞ BAŞLIYOR ---
        if (anim != null) anim.SetBool("isWalking", true);
        agent.SetDestination(campPoint.position);
        StartCoroutine(CampRoutine());
    }

    IEnumerator CampRoutine()
    {
        while (agent.pathPending || agent.remainingDistance > 0.5f) yield return null;

        // --- 2. KAMPA VARDI, OTURMA SEKANSINA GEÇ ---
        agent.isStopped = true; // Kaymayı engellemek için ajanı çivile!
        
        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isSitting", true);
        }

        float timer = 0;
        while (timer < waitTimeInCamp)
        {
            if (!isInteracting) timer += Time.deltaTime;
            
            if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle)
            {
                LeaveCamp();
                yield break;
            }
            yield return null;
        }

        LeaveCamp();
    }

    public void LeaveCamp()
    {
        if (isLeaving) return;
        isLeaving = true;
        isInteracting = false;
        
        if (NotificationManager.Instance != null) 
            NotificationManager.Instance.Show($"{strangerName} kamptan ayrıldı.", NotificationType.Info);

        // Gitme işlemini direkt yapmak yerine, önce ayağa kalkması için Coroutine başlatıyoruz
        StartCoroutine(LeaveRoutine());
    }

    // --- YENİ: AYAĞA KALKMA VE GİTME SEKANSINI YÖNETEN COROUTINE ---
    IEnumerator LeaveRoutine()
    {
        // 1. Oturmayı bırak, "Sit to Stand" animasyonu başlasın
        if (anim != null) anim.SetBool("isSitting", false);

        // 2. Animasyonun bitmesini bekle (Eğer adam çok yavaş kalkıyorsa standUpTime süresini Inspector'dan artır)
        yield return new WaitForSeconds(standUpTime);

        // 3. Artık ayakta! Yürümeye başla ve kapıya yönel
        if (anim != null) anim.SetBool("isWalking", true);
        agent.isStopped = false; // Çivileri sök
        agent.SetDestination(exitPoint.position);
        
        StartCoroutine(DestroyWhenArrived());
    }

    IEnumerator DestroyWhenArrived()
    {
        while (agent.pathPending || agent.remainingDistance > 0.5f) yield return null;
        Destroy(gameObject);
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject() || isLeaving) return;
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle) return;

        isInteracting = true;
        StrangerUIManager.Instance.OpenOfferPanel(this);
    }

    // UI'dan "Kabul Et" butonuna basıldığında tetiklenecek
   // UI'dan "Kabul Et" butonuna basıldığında tetiklenecek
    public void AcceptOffer()
    {
        // 1. ÖNCE KAPASİTE KONTROLÜ (Eğer gelen kişi Asker ise)
        if (eventType == 1)
        {
            // Sahnede bize ait kaç asker var sayalım (Senin RecruitManager mantığıyla)
            Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
            int mySoldierCount = 0;
            foreach (var soldier in allSoldiers)
            {
                if (soldier.CompareTag("MySoldier")) mySoldierCount++;
            }

            int maxCapacity = CampManager.Instance != null ? CampManager.Instance.GetMaxSoldierCapacity() : 3;

            if (mySoldierCount >= maxCapacity)
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show($"Kışlada boş yatak yok! ({mySoldierCount}/{maxCapacity})", NotificationType.Warning);
                
                StrangerUIManager.Instance.ClosePanel();
                isInteracting = false; // Tekrar tıklayabilsin
                return; 
            }
        }

        // 2. PARAYI KES VE ÖDÜLÜ VER
        if (MoneyManager.Instance.Spend(cost))
        {
            if (eventType == 0) // --- TÜCCAR EFEKTİ ---
            {
                if (CampMoraleManager.Instance != null)
                {
                    CampMoraleManager.Instance.ChangeMorale(15); 
                }
                
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show("Tüccarın iksiri işe yaradı! Ordunun morali arttı (+15).", NotificationType.Success);
            }
            else // --- YARALI ASKER EFEKTİ ---
            {
                if (RecruitManager.Instance != null)
                {
                    // RecruitManager'ın prefab'ını ve noktasını kullanarak askeri doğrudan yaratıyoruz!
                    GameObject newObj = Instantiate(RecruitManager.Instance.soldierPrefab, RecruitManager.Instance.soldierSpawnPoint.position, Quaternion.identity);
                    Gladiator glad = newObj.GetComponent<Gladiator>();
                    
                    JanissaryData newData = ScriptableObject.CreateInstance<JanissaryData>();
                    newData.gladiatorName = "Gezgin Savaşçı";
                    newData.strength = Random.Range(3, 6); // Acemilerden biraz daha güçlü
                    newData.stamina = Random.Range(3, 6);
                    newData.defense = 2; // Zırh kullanmayı biliyor
                    newData.speed = 2;
                    newData.level = 2; // Tecrübeli

                    glad.InitializeData(newData);
                    
                    // Asker sayısını üst panellerde güncellemek için senin yazdığın RefreshUI fonksiyonunu tetikliyoruz
                    RecruitManager.Instance.RefreshUI();
                }

                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show("Aramıza hoş geldin! Kışlaya tecrübeli bir savaşçı katıldı.", NotificationType.Success);
            }
            
            StrangerUIManager.Instance.ClosePanel();
            LeaveCamp(); // İşi bitti, gitsin.
        }
        else
        {
            // Parası yetmedi
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Yeterli Akçen yok!", NotificationType.Error);
            
            isInteracting = false; 
        }
    }

    // UI'dan "Reddet" butonuna basıldığında tetiklenecek
    public void DeclineOffer()
    {
        StrangerUIManager.Instance.ClosePanel();
        LeaveCamp(); // Reddedildi, küsüp gitsin
    }
}