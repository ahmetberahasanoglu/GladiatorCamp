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

    [Header("Özellikler")]
    public string enemyTag = "Enemy"; // Bizim askerler için "Enemy", Düşmanlar için "Player" olacak
    public float attackRange = 2.0f;  // Ne kadar yakından vurabilir?
    public float attackCooldown = 1.5f; // Kaç saniyede bir vurur?
    public int damage = 10;
    public int health = 100;

    [Header("Durum")]
    public Transform target;
    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
        training = GetComponent<GladiatorTraining>();
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed += Random.Range(-0.5f, 0.5f);
        // Başlangıçta hemen bir iş bulmasın, biraz beklesin
        StartCoroutine(LifeCycleRoutine());
    }
    void Update()
    {
        if (isDead || BattleManager.Instance.state != BattleState.Fighting) return;

        // 1. Hedefin yoksa veya öldüyse yeni hedef bul
        if (target == null)
        {
            FindNearestTarget();
            if(target == null) return; // Hala hedef yoksa (Savaş bitmiş olabilir) bekle
        }

        // 2. Mesafe Ölçümü
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            // --- SALDIRI MODU ---
            agent.isStopped = true; // Dur
            if (animator) animator.SetBool("isRunning", false); // Koşma animasyonunu durdur

            // Vurma zamanı geldi mi?
            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
            
            // Hedefe dön
            Vector3 lookPos = target.position - transform.position;
            lookPos.y = 0;
            if(lookPos != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookPos);
        }
        else
        {
            // --- KOVALAMA MODU ---
            agent.isStopped = false;
            agent.SetDestination(target.position);
            if (animator) animator.SetBool("isRunning", true);
        }
    }

    void FindNearestTarget()
    {
        // Sahnedeki o etikete sahip tüm objeleri bul
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float minDistance = Mathf.Infinity;
        GameObject nearest = null;

        foreach (GameObject enemy in enemies)
        {
            // Kendisiyle aynı etikettekilere saldırmasın ve ölü olanları pas geçsin
            GladiatorAI enemyAI = enemy.GetComponent<GladiatorAI>();
            if (enemyAI != null && !enemyAI.isDead) 
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = enemy;
                }
            }
        }
        target = nearest != null ? nearest.transform : null;
    }

    void Attack()
    {
        if (animator) animator.SetTrigger("Attack"); // "Attack" triggerını tetikle
        
        // Karşı tarafa hasar ver
        GladiatorAI enemyScript = target.GetComponent<GladiatorAI>();
        if(enemyScript != null)
        {
            if (Random.value < 0.2f)
            {
                enemyScript.TakeDamage(damage*2);
            }
            enemyScript.TakeDamage(damage);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        health -= amount;
        
        // Hasar efekti, ses, kan particle vs. buraya eklenebilir
        // Debug.Log(gameObject.name + " hasar aldı: " + amount);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        agent.isStopped = true;
        agent.enabled = false; // Navmesh'i kapat ki diğerleri üzerinden geçebilsin
        GetComponent<Collider>().enabled = false; // Tıklamayı kapat

        if (animator) animator.SetTrigger("Die"); // Ölme animasyonu

        // Savaş bitti mi kontrolü (Basit bir kontrol)
        CheckBattleEnd();
        
        // 5 saniye sonra cesedi yok et (veya yerde kalsın)
        Destroy(gameObject, 5f);
    }

    void CheckBattleEnd()
    {
        // Eğer sahnede hiç "Enemy" etiketli veya "Player" etiketli kimse kalmadıysa savaşı bitir
        // Bu kontrolü BattleManager'da yapmak daha performanslıdır ama şimdilik burada kalsın.
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
                if (agent.isActiveAndEnabled && agent.isOnNavMesh) 
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