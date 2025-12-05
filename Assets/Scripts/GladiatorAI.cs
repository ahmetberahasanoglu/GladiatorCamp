using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GladiatorAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Gladiator gladiator; // Durum kontrolü için (Seferde mi?)
    private GladiatorTraining training; // Eğitimde mi?

    private ActivityPoint currentPoint; // Şu an hedeflediğim nokta
    private float activityTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
        training = GetComponent<GladiatorTraining>();
    }

    void Start()
    {
        // Başlangıçta hemen bir iş bulmasın, biraz beklesin
        StartCoroutine(LifeCycleRoutine());
    }

    IEnumerator LifeCycleRoutine()
    {
        while (true)
        {
            // 1. KONTROL: Müsait miyim?
            if (gladiator.isOnMission || (training != null && training.IsTraining))
            {
                // Eğer meşgulse bekle ve noktayı bırak
                if (currentPoint != null) LeavePoint();
                yield return new WaitForSeconds(2f);
                continue;
            }

            // 2. KONTROL: Şu an bir aktivite yapıyor muyum?
            if (currentPoint != null)
            {
                // Hedefe vardım mı?
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    // Animasyon oynatılabilir (Örn: Otur, Yemek Ye)
                    // animator.SetBool("Eating", true);
                    
                    // Süreyi azalt
                    activityTimer -= Time.deltaTime;

                    // Yüzünü noktaya dön (Örn: Ateşe dön)
                    Vector3 lookPos = currentPoint.transform.position + currentPoint.transform.forward;
                    // Basit rotasyon kodu eklenebilir
                }

                // Süre bitti mi?
                if (activityTimer <= 0)
                {
                    LeavePoint();
                }
            }
            else
            {
                // 3. EYLEM: Yeni bir aktivite bul
                FindNewActivity();
            }

            yield return null; // Her frame bekle
        }
    }

    void FindNewActivity()
    {
        // Örn: %30 ihtimalle yemek yesin, %70 ihtimalle boş takılsın
        ActivityPoint.PointType desiredType = ActivityPoint.PointType.Idle;
        float roll = Random.value;
        
        if (roll < 0.3f) desiredType = ActivityPoint.PointType.Eating;
        else if (roll < 0.5f) desiredType = ActivityPoint.PointType.Praying;

        // Manager'dan yer iste
        ActivityPoint p = CampLifeManager.Instance.GetFreePoint(desiredType);

        if (p != null)
        {
            TakePoint(p);
        }
        else
        {
            // Yer yoksa kısa süre bekle (Idle animasyonunda kalsın)
            activityTimer = Random.Range(2f, 5f);
        }
    }

    void TakePoint(ActivityPoint p)
    {
        currentPoint = p;
        currentPoint.isOccupied = true; // REZERVE ET
        agent.SetDestination(currentPoint.transform.position);
        activityTimer = Random.Range(10f, 20f); // Orada ne kadar kalacak?
    }

    void LeavePoint()
    {
        if (currentPoint != null)
        {
            currentPoint.isOccupied = false; // YERİ BOŞALT
            currentPoint = null;
            // animator.SetBool("Eating", false);
        }
    }
    
   
    void OnDestroy()
    {
        LeavePoint();
    }
}