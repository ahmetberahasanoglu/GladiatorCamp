using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GladiatorAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Gladiator gladiator; 
    private GladiatorTraining training; 

    private ActivityPoint currentPoint; 
    private float activityTimer;

    [Header("Efektler")]
    public GameObject deathEffectPrefab;
   // public ParticleSystem gaziEffectPrefab; // İsteğe bağlı: Gazi olunca çıkacak parlama efekti

    [Header("Özellikler")]
    public string enemyTag = "Enemy"; 
    public float attackRange = 2.0f;  
    public float attackCooldown = 1.5f; 
    public int damage = 10;
    public int health = 100;

    [Header("Durum")]
    public Transform target;
    private Animator animator;
    private float lastAttackTime;
    public bool isDead = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
        training = GetComponent<GladiatorTraining>();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        agent.speed = 3.5f + (gladiator.data.speed * 0.05f);
        StartCoroutine(LifeCycleRoutine());
    }
    
    void Update()
    {
        if (isDead || BattleManager.Instance.state != BattleState.Fighting) return;

        if (target == null)
        {
            FindNearestTarget();
            if (target == null) return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            // --- SALDIRI ---
            if (agent.isActiveAndEnabled) agent.isStopped = true;
            if (animator) animator.SetBool("isRunning", false);

            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if(direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // --- KOVALAMA ---
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
            if (animator) animator.SetBool("isRunning", true);
        }
    }

    void Attack()
    {
        if (animator) animator.SetTrigger("Attack");

        if (target != null)
        {
            GladiatorAI enemyAI = target.GetComponent<GladiatorAI>();
            if (enemyAI != null && gladiator.data != null)
            {
                float damage = gladiator.data.strength * 1.5f;
                damage += (gladiator.data.level * 2);

                bool isCrit = Random.Range(0, 100) < gladiator.data.speed;
                if (isCrit) damage *= 1.5f; 

                enemyAI.TakeDamage(damage, isCrit);
            }
        }
    }

    public void TakeDamage(float incomingDamage, bool isCritical = false)
    {
        if (isDead || gladiator.data == null) return;
        //  kampın genel moralini alıyoruz. Moral %100 ise ekstra +10 Defans sağlar.
        float campMorale = CampMoraleManager.Instance != null ? CampMoraleManager.Instance.currentMorale : 50f;
        float defensePower = gladiator.data.defense + (campMorale / 10.0f);
        
        float reduction = 100.0f / (100.0f + defensePower);
        float finalDamage = incomingDamage * reduction;

        gladiator.currentHealth -= finalDamage;
        if (gladiator.healthBar != null)
        {
            gladiator.healthBar.UpdateBar(gladiator.currentHealth, gladiator.maxHealth);
        }

        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.ShowDamage(transform.position, finalDamage, isCritical ? 1 : 0);
        }

        if (gladiator.currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return; 

        isDead = true;
        
        // 1. Savaş dışı bırak
        if (agent.isActiveAndEnabled) 
        {
            agent.isStopped = true;
            agent.enabled = false; 
        }
        GetComponent<Collider>().enabled = false; 

        // 2. Animasyon
        if (animator) animator.SetTrigger("Die");

        // --- DEĞİŞEN KISIM: ARTIK KALICI ÖLÜM VAR ---
        gladiator.currentHealth = 0; // Can sıfırda kalır
        if (gladiator.healthBar != null) gladiator.healthBar.gameObject.SetActive(false); // Can barını gizle
        // -------------------------------------------

        if (deathEffectPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 2.0f;
            GameObject skullVFX = Instantiate(deathEffectPrefab, spawnPos, Quaternion.identity);
            Destroy(skullVFX, 3.0f);
        }
        
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.CheckBattleStatus();
        }

        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.Show($"{gladiator.data.gladiatorName} öldü", NotificationType.Error);
        }

        // --- YENİ EKLENEN KISIM: CESEDİ SİL VE LİSTEDEN ÇIKAR ---
        // Askerin birliğinden kalıcı olarak silinmesini sağlayan fonksiyonu çağır (GladiatorRoster gibi bir kodun varsa)
        // Sonra da objeyi yok et ki kampa geri dönmesin. 
        // 5 saniye bekle ki ölüm animasyonu izlensin.
        Destroy(gameObject, 5f);
    }

    void FindNearestTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float minDst = Mathf.Infinity;
        GameObject nearest = null;
        
        foreach (GameObject e in enemies)
        {
            var ai = e.GetComponent<GladiatorAI>();
            if (ai != null && !ai.isDead)
            {
                float dst = Vector3.Distance(transform.position, e.transform.position);
                if (dst < minDst) { minDst = dst; nearest = e; }
            }
        }
        target = nearest != null ? nearest.transform : null;
    }

    // --- YENİLENEN KISIM: SAVAŞTAN SAĞ DÖNENLER İÇİN GAZİ SİSTEMİ ---
    public void ReviveForCamp()
    {
        // Eğer asker öldüyse zaten Destroy edilecek, kod buraya girmesin.
        if (isDead) return;

        target = null; 
        GetComponent<Collider>().enabled = true;
        
        if (animator) 
        {
            animator.Rebind(); 
            animator.Update(0f);
        }

        if (agent != null)
        {
            agent.enabled = true; 
            agent.isStopped = true; 
        }

        // --- GAZİ SİSTEMİ KONTROLÜ ---
        // Eğer asker savaşa girdiyse, seviyesi 3'ten büyükse ve henüz gazi değilse ona bu onuru ver.
        // (JanissaryData içine public bool isGazi; eklemen gerekecek)
        if (gladiator.data.level >= 3 && !gladiator.data.isGazi)
        {
            MakeGazi();
        }
    }
    
    void MakeGazi()
    {
        gladiator.data.isGazi = true;
        
        // Gazi olan askerin morali kalıcı olarak artar
       // gladiator.data.morale += 30; 
        /*
        if (gaziEffectPrefab != null)
        {
            Instantiate(gaziEffectPrefab, transform.position, Quaternion.identity, transform);
        }*/

        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.Show($"{gladiator.data.gladiatorName} artık bir GAZİ! Morali yükseldi.", NotificationType.Success);
        }
    }

    IEnumerator LifeCycleRoutine()
    {
        while (true)
        {
            if (isDead) yield break; // Öldüyse döngüyü kır

            if (gladiator.isOnMission || (training != null && training.IsTraining))
            {
                if (currentPoint != null) LeavePoint();
                yield return new WaitForSeconds(2f);
                continue;
            }

            if (currentPoint != null)
            {
                if (agent.isActiveAndEnabled && agent.isOnNavMesh) 
                {
                    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    {
                        activityTimer -= Time.deltaTime;
                    }
                }   
                if (activityTimer <= 0)
                {
                    LeavePoint();
                }
            }
            else
            {
                FindNewActivity();
            }

            yield return null; 
        }
    }

    void FindNewActivity()
    {
        ActivityPoint.PointType desiredType = ActivityPoint.PointType.Idle;
        float roll = Random.value;
        
        if (roll < 0.3f) desiredType = ActivityPoint.PointType.Eating;
        else if (roll < 0.5f) desiredType = ActivityPoint.PointType.Praying;

        ActivityPoint p = CampLifeManager.Instance.GetFreePoint(desiredType);

        if (p != null) TakePoint(p);
        else activityTimer = Random.Range(2f, 5f);
    }

    void TakePoint(ActivityPoint p)
    {
        currentPoint = p;
        currentPoint.isOccupied = true; 
        agent.SetDestination(currentPoint.transform.position);
        activityTimer = Random.Range(10f, 20f); 
    }

    void LeavePoint()
    {
        if (currentPoint != null)
        {
            currentPoint.isOccupied = false; 
            currentPoint = null;
        }
    }
    
    void OnDestroy()
    {
        LeavePoint();
    }
}