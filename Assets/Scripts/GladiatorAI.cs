using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GladiatorAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Gladiator gladiator; 
    private GladiatorTraining training; 
    private GladiatorInventory inventory; // YENİ: Silahı okumak için eklendi
    private ActivityPoint currentPoint; 
    private float activityTimer;

    [Header("Efektler ve Menzilli Saldırı")]
    public GameObject deathEffectPrefab;
    public GameObject arrowPrefab; // YENİ: Fırlatılacak ok prefabi
    public Transform arrowSpawnPoint; // YENİ: Okun çıkacağı yer (Okçunun eli veya yayı)

    [Header("Özellikler")]
    public string enemyTag = "Enemy"; 
    
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

    // --- DİNAMİK STAT OKUYUCULAR ---
    private float CurrentAttackSpeed => 1f + (gladiator.data.speed * 0.02f);
    private float CurrentPoise => 1f + (gladiator.data.stamina * 0.05f);
    private float CurrentMoveSpeed => 3.5f + (gladiator.data.speed * 0.05f);

    // YENİ: SİHİRLİ MENZİL OKUYUCU
    // Artık sabit bir menzil yok. Elindeki silaha bakar, ona göre menzilini ve saldırı tipini anlar.
    private float CurrentAttackRange
    {
        get
        {
            if (inventory != null && inventory.weapon != null)
            {
                return inventory.weapon.weaponRange; // ItemData içine ekleyeceğimiz değişken
            }
            return 2.0f; // Silahsızsa varsayılan yakın dövüş
        }
    }

    private bool IsRangedWeapon
    {
        get
        {
            if (inventory != null && inventory.weapon != null)
            {
                return inventory.weapon.isRanged; // ItemData içine ekleyeceğimiz değişken
            }
            return false;
        }
    }

    [Header("Savaş Durumu")]
    public bool isInBattle = false; 

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
        training = GetComponent<GladiatorTraining>();
        inventory = GetComponent<GladiatorInventory>(); // Envanteri bağla
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogWarning(gameObject.name + " objesinde Animator bulunamadı!");
        
        StartCoroutine(LifeCycleRoutine());
    }
    
    void Update()
    {
        if (isDead || BattleManager.Instance.state != BattleState.Fighting || !isInBattle || isGettingHit) return;

        agent.speed = CurrentMoveSpeed;
        
        // YENİ: Durma mesafesini silahın menziline göre SÜREKLİ güncelle!
        agent.stoppingDistance = CurrentAttackRange * 0.8f; 

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

        if (distance <= CurrentAttackRange)
        {
            // --- MENZİLDEYİZ ---
            if (agent.isActiveAndEnabled) agent.isStopped = true;
            if (animator) animator.SetBool("isRunning", false);

            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if(direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);

            if (!isAttacking && Time.time > lastAttackTime + (0.1f / CurrentAttackSpeed))
            {
                if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
                _attackCoroutine = StartCoroutine(AttackRoutine());
            }
        }
        else
        {
            // --- KOVALAMA ---
            if (!isAttacking)
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
        
        float currentSpd = CurrentAttackSpeed;
        if (animator) 
        {
         
            animator.SetBool("IsRanged", IsRangedWeapon); 
            
            animator.SetFloat("AttackSpeedMultiplier", currentSpd);
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(baseAttackAnimLength / currentSpd);
        
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    // YENİ: Ortak Saldırı İğnesi (Animation Event burayı çağırmalı)
    public void ExecuteAttackEvent()
    {
        if (target == null || isDead) return;

        // Hasarı burada hesaplıyoruz (Hem ok hem kılıç için geçerli)
        float attackDamage = gladiator.data.strength * 2f + (gladiator.data.level * 2);
        bool isCrit = Random.Range(0, 100) < gladiator.data.speed;
        if (isCrit) attackDamage *= 1.5f; 

        if (IsRangedWeapon)
        {
            // --- MENZİLLİ SALDIRI (OK FIRLAT) ---
            if (arrowPrefab != null && arrowSpawnPoint != null)
            {
                // Oku hedefe doğru bakacak şekilde yarat
                Vector3 aimDirection = (target.position + Vector3.up * 1f) - arrowSpawnPoint.position;
                GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.LookRotation(aimDirection));
                
                // Oku fırlat
                Projectile projectile = arrow.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Setup(target, attackDamage, isCrit);
                }
            }
        }
        else
        {
            // --- YAKIN DÖVÜŞ (KILIÇ/MIZRAK) ---
            float currentDist = Vector3.Distance(transform.position, target.position);
            // Menzilin biraz fazlasına (x1.5) tolerans tanıyoruz ki adam 1 adım kaçtı diye vuruş boşa gitmesin
            if (currentDist <= CurrentAttackRange * 1.5f) 
            {
                GladiatorAI enemyAI = target.GetComponent<GladiatorAI>();
                if (enemyAI != null && !enemyAI.isDead && gladiator.data != null)
                {   
                    enemyAI.TakeDamage(attackDamage, isCrit);
                }
            }
        }
    }

    // ... (TakeDamage, Die, HitStunRoutine, ReviveForCamp, LifeCycleRoutine fonksiyonları BİREBİR AYNI KALACAK) ...

    public void TakeDamage(float incomingDamage, bool isCritical = false)
    {
        if (isDead || gladiator.data == null) return;
        
        float campMorale = CampMoraleManager.Instance != null ? CampMoraleManager.Instance.currentMorale : 50f;
        float defensePower = gladiator.data.defense + (campMorale / 10.0f);
        
        float reduction = 100.0f / (100.0f + defensePower);
        float finalDamage = incomingDamage * reduction;

        gladiator.currentHealth -= finalDamage;
        if (gladiator.currentHealth <= 0)
        {
            int currentNasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
            
            // Eğer oyuncunun Nasip'i çok yüksekse (Örn: 5 veya 6) VE %15 şans tutarsa...
            // (Eğer asker "MySoldier" değil de düşmansa, ona nasip işlemiememeli)
            if (currentNasip >= 5 && gladiator.CompareTag("MySoldier") && Random.Range(0, 100) < 15)
            {
                // MUCİZE GERÇEKLEŞTİ! Ölümü iptal et.
                gladiator.currentHealth = 1;

                // Ekrana altın rengi yazıyı bas
                if (DamageTextManager.Instance != null) 
                  DamageTextManager.Instance.ShowCustomText(transform.position, "ALLAH KORUDU!", Color.yellow);
                
                // İsteğe bağlı: Kutsal bir ses efekti çal
                // AudioManager.Instance.PlaySFX(AudioManager.Instance.miracleSound, 1.0f);

                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show($"<color=yellow>MUCİZE!</color> {gladiator.data.gladiatorName} Nasibi sayesinde ölümden döndü!", NotificationType.Success);
            }
        }
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
           if (ai != null && !ai.isDead && ai.isInBattle)
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

    }
    

    public void MakeGazi()
    {
        if (gladiator.data.isGazi) return; // Zaten gaziyse işlemi iptal et

        gladiator.data.isGazi = true;
        gladiator.data.level += 2;


        gladiator.RecalculateMaxHealth();

        //if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(10);
        if (NotificationManager.Instance != null) NotificationManager.Instance.Show($"{gladiator.data.gladiatorName} artık bir GAZİ! (+2 Seviye)", NotificationType.Success);
    }

    IEnumerator LifeCycleRoutine()
    {
        while (true)
        {
            if (isDead) yield break; 
            if (isInBattle || (BattleManager.Instance != null && BattleManager.Instance.state == BattleState.Fighting))
            {
                if (currentPoint != null) LeavePoint(); // Savaşı gördüğü an elindeki işi/çorbayı bıraksın
                yield return new WaitForSeconds(0.5f); // Savaş bitene kadar yarım saniyede bir kontrol et
                continue; // Aşağıdaki yürüme ve kamp kodlarını KESİNLİKLE çalıştırma!
            }

            if (gladiator.isOnMission || (training != null && training.IsTraining) || (gladiator.data != null && gladiator.data.currentActivity == SoldierActivity.Working&&gladiator.data.currentActivity==SoldierActivity.Praying))
            {
                if (currentPoint != null) LeavePoint(); 
                
                // askeri kampta görünmez yapabilirim
                // gladiator.gameObject.SetActive(false); 

                yield return new WaitForSeconds(2f);
                continue; // Döngüyü başa sar, hiçbir yere yürüme!
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