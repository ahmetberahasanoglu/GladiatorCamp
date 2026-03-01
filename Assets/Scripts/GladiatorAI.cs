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

    // --- YENİ EKLENEN KISIM ---
    // Hasar alırken animasyonun kesilmemesi için ufak bir kontrol
    private bool isGettingHit = false; 

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
        training = GetComponent<GladiatorTraining>();
    }

    void Start()
    {
        // YENİ: Animator bileşenini sadece ana objede değil, çocuk objelerde de ara
        // (Çünkü modeli genelde Gladiator objesinin altına sürükleriz)
        animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogWarning(gameObject.name + " objesinde Animator bulunamadı!");

        agent.speed = 3.5f + (gladiator.data.speed * 0.05f);
        StartCoroutine(LifeCycleRoutine());
    }
    
    void Update()
    {
        // Eğer öldüysek, savaşmıyorsak veya hasar animasyonu oynuyorsa hareketi kes
        if (isDead || BattleManager.Instance.state != BattleState.Fighting || isGettingHit) return;

        if (target == null)
        {
            FindNearestTarget();
            if (target == null)
            {
                if (animator) animator.SetBool("isRunning", false);
                return;
            }
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

        // --- YENİ EKLENEN KISIM: HASAR ALMA ANİMASYONU ---
        if (gladiator.currentHealth > 0)
        {
            if (animator) animator.SetTrigger("getHit");
            StartCoroutine(HitStunRoutine());
        }
        else
        {
            Die();
        }
    }

    // Hasar alındığında kısa süreli (0.5s) sersemleme efekti yaratır
    IEnumerator HitStunRoutine()
    {
        isGettingHit = true;
        if (agent.isActiveAndEnabled) agent.isStopped = true;
        if (animator) animator.SetBool("isRunning", false);
        
        // Animasyonun bitmesini bekle (Yaklaşık yarım saniye)
        yield return new WaitForSeconds(0.5f);
        
        isGettingHit = false;
    }

    void Die()
    {
        if (isDead) return; 

        isDead = true;
        
        // --- YENİ EKLENEN SATIR: UI'a "Ben öldüm, kapasiteyi güncelle" diye bağır ---
        if (TopInfoBarUI.Instance != null) TopInfoBarUI.Instance.UpdateCapacity();
        if (GladiatorSelector.Instance != null) GladiatorSelector.Instance.DeselectIfDead(this.gameObject);
        
        if (agent.isActiveAndEnabled) 
        {
            agent.isStopped = true;
            agent.enabled = false; 
        }
        GetComponent<Collider>().enabled = false; 

        if (animator) animator.SetTrigger("Die");

        gladiator.currentHealth = 0; 
        if (gladiator.healthBar != null) gladiator.healthBar.gameObject.SetActive(false); 

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

    public void ReviveForCamp()
    {
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

        if (gladiator.data.level >= 3 && !gladiator.data.isGazi)
        {
            MakeGazi();
        }
    }
    
    void MakeGazi()
    {
        gladiator.data.isGazi = true;
        
        if (CampMoraleManager.Instance != null)
        {
            CampMoraleManager.Instance.ChangeMorale(10);
        }

        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.Show($"{gladiator.data.gladiatorName} artık bir GAZİ! Morali yükseldi.", NotificationType.Success);
        }
    }

    IEnumerator LifeCycleRoutine()
    {
        while (true)
        {
            if (isDead) yield break; 

            if (gladiator.isOnMission || (training != null && training.IsTraining))
            {
                if (currentPoint != null) LeavePoint();
                yield return new WaitForSeconds(2f);
                continue;
            }

            // --- YENİ EKLENEN KISIM: KAMP İÇİ HAREKET ANİMASYONLARI ---
            if (currentPoint != null)
            {
                if (agent.isActiveAndEnabled && agent.isOnNavMesh) 
                {
                    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    {
                        if (animator) animator.SetBool("isRunning", false);
                        activityTimer -= Time.deltaTime;
                    }
                    else
                    {
                        if (animator) animator.SetBool("isRunning", true);
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