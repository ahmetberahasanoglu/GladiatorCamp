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
    public float baseAttackCooldown = 1.5f; // Temel bekleme süresi
    public int damage = 10;
    public int health = 100;

    [Header("Durum")]
    public Transform target;
    private Animator animator;
    private float lastAttackTime;
    public bool isDead = false;

    // --- YENİ: SAVAŞ KONTROLCÜLERİ ---
    private bool isGettingHit = false; 
    private bool isAttacking = false; // Saldırı animasyonu oynarken kilitler
    private float currentAttackSpeed = 1f; // Hız statından gelecek çarpan

    private float lastHitStunTime = 0f;
    public float stunImmunityDuration = 1.2f; // Karakter sendeledikten sonra 1.2 saniye boyunca tekrar sendelemez
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
        training = GetComponent<GladiatorTraining>();
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogWarning(gameObject.name + " objesinde Animator bulunamadı!");
        
        stunImmunityDuration = 0.5f + (gladiator.data.stamina * 0.04f);
        // --- YENİ: HIZ MATEMATİĞİ VE ÇARPIŞMA KONTROLÜ ---
        // Hız 10 ise %20 (1.2x) daha hızlı vurur, Hız 50 ise %100 (2.0x) daha hızlı vurur!
        currentAttackSpeed = 1f + (gladiator.data.speed * 0.02f);
      
        // Yürüme hızı
        agent.speed = 3.5f + (gladiator.data.speed * 0.05f);
        
        // KAOS ENGELLEYİCİ: Askerler hedefin tam içine girmek yerine menzilin ucunda durur
        agent.stoppingDistance = attackRange * 0.8f; 
        
        StartCoroutine(LifeCycleRoutine());
    }
    
    void Update()
    {
        // Ölüyse, savaşmıyorsa, hasar alıyorsa veya SALDIRI YAPARKEN kilitliyse hareketi kes
        if (isDead || BattleManager.Instance.state != BattleState.Fighting || isGettingHit || isAttacking) return;

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
            // --- MENZİLDEYİZ ---
            if (agent.isActiveAndEnabled) agent.isStopped = true;
            if (animator) animator.SetBool("isRunning", false);

            // Hedefe Dön
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if(direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);

            // Cooldown süresini askerin Hızına (currentAttackSpeed) göre kısaltıyoruz!
            if (Time.time > lastAttackTime + (baseAttackCooldown / currentAttackSpeed))
            {
                StartCoroutine(AttackRoutine());
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

    
// Eski AttackRoutine'i silip bunu yapıştır:
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (animator) 
        {
            animator.SetFloat("AttackSpeedMultiplier", currentAttackSpeed);
            animator.SetTrigger("Attack");
        }

        // Artık burada saniye beklemiyoruz! Animasyonun bitmesini bekliyoruz.
        // Saldırı animasyonunun uzunluğu yaklaşık 1 saniye ise, hıza bölerek bekliyoruz.
        yield return new WaitForSeconds(1.0f / currentAttackSpeed);
        
        isAttacking = false;
    }

    // YENİ FONKSİYON: Animasyondaki iğne (Event) tetiklendiğinde burası çalışır
    public void ExecuteMeleeHit()
    {
        if (target != null && !isDead)
        {
            float currentDist = Vector3.Distance(transform.position, target.position);
            // Kılıç savrulurken adam kaçmadıysa (Menzildeyse) hasar ver
            if (currentDist <= attackRange * 1.5f) 
            {
                GladiatorAI enemyAI = target.GetComponent<GladiatorAI>();
                if (enemyAI != null && !enemyAI.isDead && gladiator.data != null)
                {
                    float attackDamage = gladiator.data.strength * 1.5f;
                    attackDamage += (gladiator.data.level * 2);

                    bool isCrit = Random.Range(0, 100) < gladiator.data.speed;
                    if (isCrit) attackDamage *= 1.5f; 

                    enemyAI.TakeDamage(attackDamage, isCrit);
                }
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

       if (gladiator.currentHealth > 0)
        {
            // Sadece son sendelemenin üzerinden yeterli zaman geçtiyse VEYA yediği darbe Kritikse animasyon iptal edilsin
            if (Time.time > lastHitStunTime + stunImmunityDuration || isCritical)
            {
                if (animator) animator.SetTrigger("getHit");
                StartCoroutine(HitStunRoutine());
                
                lastHitStunTime = Time.time; 
            }
          
        }
        else
        {
            Die();
        }
    }

    IEnumerator HitStunRoutine()
    {
        isGettingHit = true;
        if (agent.isActiveAndEnabled) agent.isStopped = true;
        if (animator) animator.SetBool("isRunning", false);
        
        yield return new WaitForSeconds(0.5f);
        
        isGettingHit = false;
    }

    void Die()
    {
        if (isDead) return; 

        isDead = true;
        isAttacking = false; // Ölürken saldırı döngüsünü kesin kır
        
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
        
        if (BattleManager.Instance != null) BattleManager.Instance.CheckBattleStatus();
        if (NotificationManager.Instance != null) NotificationManager.Instance.Show($"{gladiator.data.gladiatorName} öldü", NotificationType.Error);

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