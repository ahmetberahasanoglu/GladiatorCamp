using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GladiatorAI : MonoBehaviour
{
    public NavMeshAgent agent;
    private Gladiator gladiator; 
    private GladiatorTraining training; 
    private GladiatorInventory inventory; 
    private ActivityPoint currentPoint; 
    private float activityTimer;
    

    [Header("Sinerji Efektleri (VFX)")]
    public GameObject poisonVFXPrefab;
    public GameObject fireVFXPrefab;
    public GameObject healVFXPrefab;


    private GameObject activePoisonVFX;
    private GameObject activeFireVFX;

    [Header("Efektler ve Menzilli Saldırı")]
    public GameObject deathEffectPrefab;
    public GameObject arrowPrefab; 
    public Transform arrowSpawnPoint; 

    [Header("Özellikler")]
    public string enemyTag = "Enemy"; 
    public float baseAttackAnimLength = 1.733f; 

    [Header("Durum")]
    public Transform target;
    public Animator animator;
    private float lastAttackTime;
    public bool isDead = false;

    // --- SAVAŞ KONTROLCÜLERİ ---
    private bool isGettingHit = false; 
    private bool isAttacking = false; 
    private float lastHitStunTime = 0f;

    private Coroutine _attackCoroutine;
    private Coroutine _hitStunCoroutine;

    // --- DURUM EFEKTLERİ (YENİ: Zehir ve Ateş) ---
    private float poisonDuration = 0f;
    private float poisonDamagePerTick = 0f;
    private float poisonTimer = 0f;

    private float fireDuration = 0f;
    private float fireDamagePerTick = 0f;
    private float fireTimer = 0f;

    // --- DİNAMİK STAT OKUYUCULAR ---
    private float CurrentAttackSpeed => 1f + (gladiator.data.speed * 0.02f);
    private float CurrentPoise => 1f + (gladiator.data.stamina * 0.05f);
    private float CurrentMoveSpeed => 3.5f + (gladiator.data.speed * 0.05f);
    
    [Header("Yapay Zeka (Radar)")]
    public float retargetInterval = 1.0f; 
    private float retargetTimer = 0f;

    [Header("Savaş Öncesi (Intro)")]
    public float battleStartDelay = 0f; 
    private float _delayTimer = 0f;
[Header("Birim Özellikleri")]
    public bool isBeast = false; 
    private float CurrentAttackRange
    {
        get { return (inventory != null && inventory.weapon != null) ? inventory.weapon.weaponRange : 2.0f; }
    }

    private bool IsRangedWeapon
    {
        get { return (inventory != null && inventory.weapon != null) ? inventory.weapon.isRanged : false; }
    }

    [Header("Savaş Durumu")]
    public bool isInBattle = false; 

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
        training = GetComponent<GladiatorTraining>();
        inventory = GetComponent<GladiatorInventory>(); 
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        StartCoroutine(LifeCycleRoutine());
    }
    
    void Update()
    {
        if (isDead) return;

      
        HandleStatusEffects();

        if (BattleManager.Instance.state != BattleState.Fighting || !isInBattle || isGettingHit) return;


        if (_delayTimer < battleStartDelay)
        {
            _delayTimer += Time.deltaTime;
            
            // Bekleme süresince ajanı durdur ve koşma animasyonunu kapat
            if (agent != null && agent.isActiveAndEnabled) agent.isStopped = true; 
            if (animator) animator.SetBool("isRunning", false);
            
            return; 
        }


        agent.speed = CurrentMoveSpeed;
        agent.stoppingDistance = CurrentAttackRange * 0.8f; 

        retargetTimer -= Time.deltaTime; 

        if (target != null)
        {
            GladiatorAI targetAI = target.GetComponent<GladiatorAI>();
            if (targetAI != null && targetAI.isDead) target = null; 
        }
        
        if (target == null || retargetTimer <= 0f)
        {
            FindNearestTarget();
            retargetTimer = retargetInterval; 
        }

        if (target == null)
        {
            if (animator) animator.SetBool("isRunning", false);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= CurrentAttackRange)
        {
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

    // --- YENİ: ZEHİR VE ATEŞ YANMA İŞLEMLERİ ---
  private void HandleStatusEffects()
    {
        // Zehir İşleyişi
        if (poisonDuration > 0)
        {
            poisonTimer += Time.deltaTime;
            if (poisonTimer >= 1.0f)
            {
                TakeDamage(poisonDamagePerTick, false, true); 
                poisonDuration -= 1.0f;
                poisonTimer = 0f;
                
                // YENİ: Süre bittiyse efekti sil
                if (poisonDuration <= 0 && activePoisonVFX != null) Destroy(activePoisonVFX);
            }
        }

        // Ateş (Yanma) İşleyişi
        if (fireDuration > 0)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= 1.0f)
            {
                TakeDamage(fireDamagePerTick, false, true); 
                fireDuration -= 1.0f;
                fireTimer = 0f;

                // YENİ: Süre bittiyse efekti sil
                if (fireDuration <= 0 && activeFireVFX != null) Destroy(activeFireVFX);
            }
        }
    }

   public void ApplyPoison(float dps, float duration) 
    { 
        poisonDamagePerTick = dps; 
        poisonDuration = duration; 
        
        // Eğer üstümüzde zaten zehir efekti yoksa, yenisini yarat ve içimize (child) al
        if (activePoisonVFX == null && poisonVFXPrefab != null)
        {
            activePoisonVFX = Instantiate(poisonVFXPrefab, transform.position + Vector3.up, Quaternion.identity, transform);
        }
    }

    public void ApplyBurn(float dps, float duration) 
    { 
        fireDamagePerTick = dps; 
        fireDuration = duration; 
        
        if (activeFireVFX == null && fireVFXPrefab != null)
        {
            activeFireVFX = Instantiate(fireVFXPrefab, transform.position + Vector3.up, Quaternion.identity, transform);
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

    public void ExecuteAttackEvent()
    {
        if (target == null || isDead) return;

        float attackDamage = gladiator.data.strength * 2f + (gladiator.data.level * 2);
        bool isCrit = Random.Range(0, 100) < gladiator.data.speed;
        if (isCrit) attackDamage *= 1.5f; 

        if (IsRangedWeapon)
        {
            if (arrowPrefab != null && arrowSpawnPoint != null)
            {
                Vector3 aimDirection = (target.position + Vector3.up * 1f) - arrowSpawnPoint.position;
                GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.LookRotation(aimDirection));
                Projectile projectile = arrow.GetComponent<Projectile>();
                if (projectile != null)
                {
                    // Ok hedefe çarptığında bizim yerimize ProcessOnHitEffects'i çağıracak
                    projectile.Setup(target, attackDamage, isCrit, this); 
                }
            }
        }
        else
        {
            float currentDist = Vector3.Distance(transform.position, target.position);
            if (currentDist <= CurrentAttackRange * 1.5f) 
            {
                GladiatorAI enemyAI = target.GetComponent<GladiatorAI>();
                if (enemyAI != null && !enemyAI.isDead && gladiator.data != null)
                {   
                    enemyAI.TakeDamage(attackDamage, isCrit);
                    ProcessOnHitEffects(enemyAI, attackDamage); // YENİ: Vuruş gerçekleştiğinde Sinerjiyi çalıştır
                }
            }
        }
    }

    // --- YENİ: SİNERJİ VE EFSANEVİ SET MOTORU ---
    public void ProcessOnHitEffects(GladiatorAI enemyAI, float baseDamage)
    {
        if (inventory == null || inventory.activeSetPieceCount < 3) return; // Set tamamlanmadıysa çık

        SoldierTrait myTrait = gladiator.data.trait;

        // 1. ZEHİR SETİ
        if (inventory.activeSet == ItemSetType.Poison)
        {
            float poisonDps = baseDamage * 0.2f; // Hasarın %20'si kadar zehir
            float pDuration = 3f;

            if (myTrait == SoldierTrait.Obur) // SİNERJİ: Asitli Mide
            {
                poisonDps *= 2f; 
                pDuration = 5f;
            }
            enemyAI.ApplyPoison(poisonDps, pDuration);
        }
        // 2. ATEŞ SETİ
        else if (inventory.activeSet == ItemSetType.Fire)
        {
            float burnDps = baseDamage * 0.3f;
            enemyAI.ApplyBurn(burnDps, 2f);

            if (myTrait == SoldierTrait.Yetenekli) // SİNERJİ: Cehennem Ustası (Alan Hasarı)
            {
                Collider[] hitEnemies = Physics.OverlapSphere(enemyAI.transform.position, 2.5f);
                foreach (var hit in hitEnemies)
                {
                    if (hit.CompareTag(enemyTag))
                    {
                        GladiatorAI nearbyEnemy = hit.GetComponent<GladiatorAI>();
                        if (nearbyEnemy != null && nearbyEnemy != enemyAI && !nearbyEnemy.isDead)
                        {
                            nearbyEnemy.TakeDamage(baseDamage * 0.4f, false, true); // Etrafa %40 sıçrama hasarı
                        }
                    }
                }
            }
        }
        // 3. İNANÇ SETİ
        else if (inventory.activeSet == ItemSetType.Faith)
        {
            float healAmount = baseDamage * 0.2f; // Hasarın %20'si kadar şifa
            float healRadius = 3.0f;

            if (myTrait == SoldierTrait.Dindar) // SİNERJİ: Kutsal İrade
            {
                healAmount *= 2f;
                healRadius = 6.0f; // Etki alanı devasa olur
            }

            // Etraftaki DOSTLARI bul ve iyileştir
            Collider[] allies = Physics.OverlapSphere(transform.position, healRadius);
            foreach (var hit in allies)
            {
                if (hit.CompareTag(this.gameObject.tag)) // Kendi takımımdansa
                {
                    Gladiator ally = hit.GetComponent<Gladiator>();
                    if (ally != null && ally.currentHealth > 0)
                    {
                        ally.currentHealth = Mathf.Min(ally.currentHealth + healAmount, ally.maxHealth);
                        if (ally.healthBar != null) ally.healthBar.UpdateBar(ally.currentHealth, ally.maxHealth);
                        if (healVFXPrefab != null)
                        {
                            GameObject healVFX = Instantiate(healVFXPrefab, ally.transform.position, Quaternion.identity);
                            Destroy(healVFX, 1.5f);
                        }    
                        // Opsiyonel: Şifa texti gösterilebilir (+20) yeşil renkte
                    }
                }
            }
        }
    }

    // YENİ: isDoT parametresi eklendi!
    public void TakeDamage(float incomingDamage, bool isCritical = false, bool isDoT = false)
    {
        if (isDead || gladiator.data == null) return;
        
        float campMorale = CampMoraleManager.Instance != null ? CampMoraleManager.Instance.currentMorale : 50f;
        float defensePower = gladiator.data.defense + (campMorale / 10.0f);
        
        float reduction = 100.0f / (100.0f + defensePower);
        float finalDamage = incomingDamage * reduction;

        gladiator.currentHealth -= finalDamage;
        if (!IsRangedWeapon && !isDoT) // Zehir yerken radarı bozma
        {
            retargetTimer = 0f; 
        }

        if (gladiator.currentHealth <= 0)
        {
            int currentNasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
            if (currentNasip >= 5 && gladiator.CompareTag("MySoldier") && Random.Range(0, 100) < 15 && !isDoT)
            {
                gladiator.currentHealth = 1;
                if (DamageTextManager.Instance != null) 
                  DamageTextManager.Instance.ShowCustomText(transform.position, "ALLAH KORUDU!", Color.yellow);
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show($"<color=yellow>MUCİZE!</color> {gladiator.data.gladiatorName} Nasibi sayesinde ölümden döndü!", NotificationType.Success);
            }
        }

        if (gladiator.healthBar != null) gladiator.healthBar.UpdateBar(gladiator.currentHealth, gladiator.maxHealth);
        if (DamageTextManager.Instance != null) DamageTextManager.Instance.ShowDamage(transform.position, finalDamage, isCritical ? 1 : 0);
        
        if (!isDoT) AudioManager.Instance.PlaySFX(AudioManager.Instance.hitSound, isCritical ? 1.0f : 0.6f);

        if (gladiator.currentHealth > 0)
        {
            // ZAMANLA HASAR (DoT) YİYORSA ASLA SENDELEME (HITSTUN) YAŞATMA!
            if (isDoT) return; 

            bool isPoiseCooldownReady = Time.time > lastHitStunTime + CurrentPoise;
            bool isPoiseBroken = !isAttacking || isCritical;

            if (isPoiseCooldownReady && isPoiseBroken && !isGettingHit)
            {
                if (isAttacking)
                {
                    if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
                    isAttacking = false;
                    if (animator) animator.ResetTrigger("Attack"); 
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
        
        isDead = true;
        if (BattleManager.Instance != null && BattleManager.Instance.currentFocusTarget == this.transform)
        {
            BattleManager.Instance.ClearFocusTarget();
        }
         if (AudioManager.Instance != null)
        {
            if (isBeast)
            {
                // YENİ: AudioManager'a ayı/hayvan ölme sesi eklemelisin
                AudioManager.Instance.PlaySFX(AudioManager.Instance.beastDeath, 1.0f);
            }
            else
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.deathSound, 1.0f);
            }
        }
        isInBattle = false;
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            isAttacking = false;
        }
        
        if (TopInfoBarUI.Instance != null) TopInfoBarUI.Instance.UpdateCapacity();
        if (GladiatorSelector.Instance != null) GladiatorSelector.Instance.DeselectIfDead(this.gameObject);
        
        if (agent.isActiveAndEnabled) 
        {
            agent.isStopped = true;
            agent.enabled = false; 
        }
        GetComponent<Collider>().enabled = false; 

        if (animator) {
            animator.SetBool("isRunning", false);
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Die");
        }
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
        Gladiator[] allEnemies = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        float minDst = Mathf.Infinity;
        Transform bestTarget = null;
         if (gameObject.CompareTag("MySoldier") && BattleManager.Instance != null && BattleManager.Instance.currentFocusTarget != null)
        {
            GladiatorAI focusAI = BattleManager.Instance.currentFocusTarget.GetComponent<GladiatorAI>();
            if (focusAI != null && !focusAI.isDead)
            {
                target = BattleManager.Instance.currentFocusTarget;
                return; 
            }
        }
        
        foreach (Gladiator e in allEnemies)
        {
            // Eğer bizimle aynı takımdaysa es geç (Tag kontrolü yerine dinamik kontrol daha iyidir ama kendi mantığına göre uyarlayabilirsin)
            if (e.gameObject.CompareTag(this.gameObject.tag)) continue; 

            GladiatorAI ai = e.GetComponent<GladiatorAI>();
            if (ai != null && !ai.isDead && ai.isInBattle)
            {
                float dst = Vector3.Distance(transform.position, e.transform.position);
                if (dst < minDst) 
                { 
                    minDst = dst; 
                    bestTarget = e.transform; 
                }
            }
        }
       
GladiatorAI[] allUnits = FindObjectsByType<GladiatorAI>(FindObjectsSortMode.None);
        float bestValue = Mathf.Infinity;
        // Eğer şu an bir hedefimiz YOKSA veya YENİ HEDEF eskisine göre bariz şekilde daha yakındaysa değiştir!
        if (target == null)
        {
            target = bestTarget;
        }
        else if (bestTarget != null && bestTarget != target)
        {
            float currentTargetDst = Vector3.Distance(transform.position, target.position);
            // Kararsızlığı (Ping-pong) önlemek için: Yeni düşman %30 daha yakınsa dön!
            if (minDst < currentTargetDst * 0.7f) 
            {
                target = bestTarget;
            }
        }
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
        if (gladiator.data.isGazi) return;

        gladiator.data.isGazi = true;
        gladiator.data.level += 2;
        gladiator.RecalculateMaxHealth();

        // Özel Gazi töreni ekranı
        if (GaziFeedback.Instance != null)
            GaziFeedback.Instance.ShowGaziCeremony(gladiator);
        else if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show(
                $"{gladiator.data.gladiatorName} artık bir GAZİ! (+2 Seviye)",
                NotificationType.Success);
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