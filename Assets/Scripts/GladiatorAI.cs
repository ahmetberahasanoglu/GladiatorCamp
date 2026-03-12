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
    
    // YENİ: Animasyonunun tam uzunluğunu buraya yaz (1.733f)
    [Tooltip("Attack animasyonunun saniye cinsinden uzunluğu")]
    public float baseAttackAnimLength = 1.733f; 

    [Header("Durum")]
    public Transform target;
    private Animator animator;
    private float lastAttackTime;
    public bool isDead = false;

    // --- SAVAŞ KONTROLCÜLERİ ---
    private bool isGettingHit = false; 
    private bool isAttacking = false; 
    private float lastHitStunTime = 0f;

    private Coroutine _attackCoroutine;
    private Coroutine _hitStunCoroutine;

    // --- SİHİRLİ KISIM: DİNAMİK STAT OKUYUCULAR ---
    // Artık bu değerler Start'ta değil, çağrıldıkları HER AN gladyatörün anlık datasına bakarak hesaplanır!
    private float CurrentAttackSpeed => 1f + (gladiator.data.speed * 0.02f);
    private float CurrentPoise => 1f + (gladiator.data.stamina * 0.05f);
    private float CurrentMoveSpeed => 3.5f + (gladiator.data.speed * 0.05f);

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
        
        agent.stoppingDistance = attackRange * 0.8f; 
        
        StartCoroutine(LifeCycleRoutine());
    }
    
    void Update()
    {
        if (isDead || BattleManager.Instance.state != BattleState.Fighting || isGettingHit) return;

        // Yürüme hızını sürekli güncelle (Eşya vs. takılırsa anında hızlansın)
        agent.speed = CurrentMoveSpeed;
if (target != null)
        {
            GladiatorAI targetAI = target.GetComponent<GladiatorAI>();
            if (targetAI != null && targetAI.isDead)
            {
                target = null; 
            }
        }
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

            // YENİ SALDIRI MANTIĞI: Eğer şu an saldırmıyorsa ve bir önceki saldırı bittiyse VUR!
            // (0.1f -> İki saldırı arasına robot gibi durmasın diye koyduğumuz çok minik bir nefes alma payı)
            if (!isAttacking && Time.time > lastAttackTime + (0.1f / CurrentAttackSpeed))
            {
                if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
                _attackCoroutine = StartCoroutine(AttackRoutine());
            }
        }
        else
        {
            // --- KOVALAMA ---
            if (!isAttacking) // Sadece saldırmıyorsa yürüyebilir!
            {
                if (agent.isActiveAndEnabled)
                {
                    agent.isStopped = false;
                    agent.SetDestination(target.position);
                }
                if (animator) animator.SetBool("isRunning", true);
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        
        // Hızı o anki statlardan çek ve Animator'a yolla
        float currentSpd = CurrentAttackSpeed;
        if (animator) 
        {
            animator.SetFloat("AttackSpeedMultiplier", currentSpd);
            animator.SetTrigger("Attack");
        }

        // COOLDOWN ÇÖPE GİTTİ! Artık kılıcın savrulma süresi kadar bekliyoruz.
        // Eğer animasyon 1.733 saniyeyse ve askerin hızı 2x ise, burası sadece 0.86 saniye bekler!
        yield return new WaitForSeconds(baseAttackAnimLength / currentSpd);
        
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    // Animasyondaki iğne (Event) tetiklendiğinde burası çalışır
    public void ExecuteMeleeHit()
    {
        if (target != null && !isDead)
        {
            float currentDist = Vector3.Distance(transform.position, target.position);
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
        if (gladiator.healthBar != null) gladiator.healthBar.UpdateBar(gladiator.currentHealth, gladiator.maxHealth);

        if (DamageTextManager.Instance != null) DamageTextManager.Instance.ShowDamage(transform.position, finalDamage, isCritical ? 1 : 0);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.hitSound, isCritical ? 1.0f : 0.6f);
        if (gladiator.currentHealth > 0)
        {
            // 1. Kural: Son sendelemenin üzerinden yeterli zaman (Stamina'ya bağlı Poise) geçmiş olmalı.
            bool isPoiseCooldownReady = Time.time > lastHitStunTime + CurrentPoise;
            
            // 2. Kural: Ya asker şu an saldırmıyor olacak, YA DA yediği darbe Kritik (Çok Ağır) olacak!
            bool isPoiseBroken = !isAttacking || isCritical;

            if (isPoiseCooldownReady && isPoiseBroken && !isGettingHit)
            {
                // Eğer adam kılıç savururken KRİTİK yediyse, o saldırı acıdan dolayı iptal olur!
                if (isAttacking)
                {
                    if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
                    isAttacking = false;
                    if (animator) animator.ResetTrigger("Attack"); // Kılıç savurmayı çöpe at
                }
                AudioManager.Instance.PlaySFX(AudioManager.Instance.gruntSound, 0.8f);
                if (animator) 
                {
                    animator.ResetTrigger("getHit"); 
                    // animator.Play("Take Damage") yerine SetTrigger kullanmak her zaman daha güvenlidir
                    // (Çünkü Animator'daki kutunun adı bazen TakeDamage, bazen getHit olabiliyor, isim hatasına düşmeyelim)
                    animator.SetTrigger("getHit"); 
                }

                if (_hitStunCoroutine != null) StopCoroutine(_hitStunCoroutine);
                _hitStunCoroutine = StartCoroutine(HitStunRoutine());
                
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
        
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;
        if (animator) animator.SetBool("isRunning", false);
        
        // TakeDamage animasyonunun süresi (0.967s) göz önüne alınarak bekleme
        yield return new WaitForSeconds(0.6f); 
        
        isGettingHit = false;

        if (animator) animator.ResetTrigger("getHit");

        if (target != null && agent.isActiveAndEnabled && agent.isOnNavMesh && !isDead) 
        {
            agent.isStopped = false;
        }
    }

    public void Die()
    {
        if (isDead) return; 
        AudioManager.Instance.PlaySFX(AudioManager.Instance.deathSound, 1.0f);
        isDead = true;
        isAttacking = false; 
        
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

        Destroy(gameObject, 2f);
    }

    // ... FindNearestTarget, ReviveForCamp, MakeGazi, LifeCycleRoutine, vb. diğer fonksiyonlar aynı kalacak ...
    
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

        if (gladiator.data.level >= 3 && !gladiator.data.isGazi) MakeGazi();
    }
    
    void MakeGazi()
    {
        gladiator.data.isGazi = true;
        if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(10);
        if (NotificationManager.Instance != null) NotificationManager.Instance.Show($"{gladiator.data.gladiatorName} artık bir GAZİ! Morali yükseldi.", NotificationType.Success);
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
                if (activityTimer <= 0) LeavePoint();
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