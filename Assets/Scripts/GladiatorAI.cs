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
    public GameObject bloodEffectPrefab;

    [Header("Özellikler")]
    public string enemyTag = "EnemySoldier"; 
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

    // --- DURUM EFEKTLERİ ---
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

    // ── YENİ: DÜŞMAN MENZİL HATASINI ÇÖZEN LOKAL SİLAH SİSTEMİ ──
    [Header("Düşman / Lokal Silah Ayarları")]
    [Tooltip("Eğer bu bir düşmense veya ScriptableObject dışından silah atanacaksa bunu aktif edin.")]
    public bool useLocalWeaponStats = false;
    public float localAttackRange = 2.0f;
    public bool localIsRanged = false;
    public WeaponClass localWeaponClass = WeaponClass.Unarmed;

   private float CurrentAttackRange => useLocalWeaponStats ? localAttackRange 
        : ((inventory != null && inventory.weapon != null) ? inventory.weapon.weaponRange 
        : ((gladiator.data != null) ? gladiator.data.attackRange : 2.0f));

    private bool IsRangedWeapon => useLocalWeaponStats ? localIsRanged 
        : ((inventory != null && inventory.weapon != null) ? inventory.weapon.isRanged 
        : ((gladiator.data != null) ? gladiator.data.isRanged : false));

    private WeaponClass CurrentWeaponClass => useLocalWeaponStats ? localWeaponClass 
        : ((inventory != null && inventory.weapon != null) ? inventory.weapon.weaponClass 
        : ((gladiator.data != null) ? gladiator.data.weaponClass : WeaponClass.Unarmed));
    // ────────────────────────────────────────────────────────────

    [Header("Savaş Durumu")]
    public bool isInBattle = false; 

  [Header("Enerji (Ulti) Sistemi")]
    public float currentEnergy = 0f;
    public float maxEnergy = 100f;
    public UnityEngine.UI.Image energyFillImage; 
    public ParticleSystem ultimateVFX; 
    
    // ── YENİ: SİNEMATİK ULTİ PREFABLARI ──
    [Tooltip("Ulti tetiklendiği an askerin vücudundan fışkıracak aura patlaması")]
    public GameObject ultimateActivationVFXPrefab;
    [Tooltip("Ulti darbesi düşmana indiğinde çıkacak devasa şok dalgası/patlama efekti")]
    public GameObject ultimateImpactVFXPrefab;

    private bool isCastingUltimate = false; 
    private bool isUltimateStrike = false;

    [Header("Özel Hedef (RTS Sistemi)")]
    public Transform forcedTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
        training = GetComponent<GladiatorTraining>();
        inventory = GetComponent<GladiatorInventory>(); 
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        StartCoroutine(LifeCycleRoutine());
    }
    
    void Update()
    {
        if (isDead) return;
  if (animator)
        {
            animator.SetInteger("WeaponType", (int)CurrentWeaponClass);
            animator.SetBool("IsRanged", IsRangedWeapon);
        }
        HandleStatusEffects();

        if (!isInBattle || isGettingHit) return;
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Fighting) return;

        if (_delayTimer < battleStartDelay)
        {
            _delayTimer += Time.deltaTime;
            if (agent != null && agent.isActiveAndEnabled) agent.isStopped = true; 
            if (animator) animator.SetBool("isRunning", false);
            return; 
        }

        // ── YENİ: BİZİM ASKERLERİN ANİMASYON HATASINI ÇÖZEN GÜNCELLEME ──
        // Savaş boyunca Animator parametrelerini sürekli güncel tutuyoruz ki 
        // dururken veya koşarken elindeki silaha uygun blend-tree devreye girsin.
      
        // ──────────────────────────────────────────────────────────────

        agent.speed = CurrentMoveSpeed;
        agent.stoppingDistance = CurrentAttackRange * 0.8f; // Menzile göre otomatik durma mesafesi

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

            if (!isAttacking && !isCastingUltimate && Time.time > lastAttackTime + (0.1f / CurrentAttackSpeed))
            {
                if (currentEnergy >= maxEnergy)
                {
                    StartCoroutine(CastUltimateRoutine());
                }
                else
                {
                    if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
                    _attackCoroutine = StartCoroutine(AttackRoutine());
                }
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

    /// <summary>
    /// Düşmanlar veya harici birimler doğduğunda ScriptableObject'i bozmadan 
    /// lokal silah verilerini ve yapay zeka radar menzilini eşitler.
    /// </summary>
    public void ForceSetupWeaponStats(float range, bool isRanged, WeaponClass wClass)
    {
        useLocalWeaponStats = true;
        localAttackRange = range;
        localIsRanged = isRanged;
        localWeaponClass = wClass;

        if (agent != null)
        {
            agent.stoppingDistance = range * 0.8f;
        }
    }

    public void GainEnergy(float amount)
    {
        if (currentEnergy >= maxEnergy || isDead) return;
        currentEnergy += amount;
        if (energyFillImage != null) energyFillImage.fillAmount = currentEnergy / maxEnergy;

        if (currentEnergy >= maxEnergy)
        {
            currentEnergy = maxEnergy;
            energyFillImage.color = Color.yellow; 
        }
    }

    private void HandleStatusEffects()
    {
        if (poisonDuration > 0)
        {
            poisonTimer += Time.deltaTime;
            if (poisonTimer >= 1.0f)
            {
                TakeDamage(poisonDamagePerTick, false, true); 
                poisonDuration -= 1.0f;
                poisonTimer = 0f;
                if (poisonDuration <= 0 && activePoisonVFX != null) Destroy(activePoisonVFX);
            }
        }

        if (fireDuration > 0)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= 1.0f)
            {
                TakeDamage(fireDamagePerTick, false, true); 
                fireDuration -= 1.0f;
                fireTimer = 0f;
                if (fireDuration <= 0 && activeFireVFX != null) Destroy(activeFireVFX);
            }
        }
    }

    public void ApplyPoison(float dps, float duration) 
    { 
        poisonDamagePerTick = dps; 
        poisonDuration = duration; 
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
            animator.SetInteger("WeaponType", (int)CurrentWeaponClass);
            animator.SetTrigger("Attack");
        }
        yield return new WaitForSeconds(baseAttackAnimLength / currentSpd);
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    IEnumerator CastUltimateRoutine()
    {
        isCastingUltimate = true;
        isAttacking = true;
        isUltimateStrike = true; 

        currentEnergy = 0f;
        if (energyFillImage != null)
        {
            energyFillImage.fillAmount = 0f;
            energyFillImage.color = new Color(0f, 0.5f, 1f); 
        }

        // ── YENİ: ULTİ TETİKLENME AURA EFEKTİ ──
        if (ultimateActivationVFXPrefab != null)
        {
            GameObject activationVFX = Instantiate(ultimateActivationVFXPrefab, transform.position + Vector3.up * 0.2f, Quaternion.identity, transform);
            Destroy(activationVFX, 2.0f); // Çöp kalmasın
        }

        if (ultimateVFX != null) ultimateVFX.Play();

        float currentSpd = CurrentAttackSpeed;
        if (animator) 
        {
            animator.SetBool("IsRanged", IsRangedWeapon); 
            animator.SetFloat("AttackSpeedMultiplier", currentSpd);
            animator.SetInteger("WeaponType", (int)CurrentWeaponClass);
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(baseAttackAnimLength / currentSpd);
        
        lastAttackTime = Time.time;
        isCastingUltimate = false;
        isAttacking = false;
        isUltimateStrike = false; 
    }
    IEnumerator LocalHitStopEffect(GladiatorAI targetAI)
    {
        if (animator != null) animator.speed = 0f;
        if (targetAI != null && targetAI.animator != null) targetAI.animator.speed = 0f;
        yield return new WaitForSeconds(0.12f); 
        if (animator != null) animator.speed = 1f;
        if (targetAI != null && targetAI.animator != null) targetAI.animator.speed = 1f;
    }

    public void ExecuteAttackEvent()
    {
        if (target == null || isDead) return;

        float attackDamage = gladiator.data.strength * 2f + (gladiator.data.level * 2);
        bool isCrit = Random.Range(0, 100) < gladiator.data.speed;

        if (isUltimateStrike)
        {
            attackDamage *= 3.0f; 
            isCrit = true; 
            if (ultimateImpactVFXPrefab != null)
            {
                // Darbenin indiği yer düşmanın ayak ucu olsun
                Vector3 vfxPos = target != null ? target.position : transform.position + transform.forward * 2f;
                GameObject impactVFX = Instantiate(ultimateImpactVFXPrefab, vfxPos + Vector3.up * 0.1f, Quaternion.identity);
                Destroy(impactVFX, 2.5f);
            }       
        }
        else
        {
            if (isCrit) attackDamage *= 1.5f; 
        }

        if (IsRangedWeapon)
        {
            if (arrowPrefab != null && arrowSpawnPoint != null)
            {
                Vector3 aimDirection = (target.position + Vector3.up * 1f) - arrowSpawnPoint.position;
                GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.LookRotation(aimDirection));
                Projectile projectile = arrow.GetComponent<Projectile>();
                if (projectile != null)
                {
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
                    GainEnergy(15f);
                    ProcessOnHitEffects(enemyAI, attackDamage); 

                    if (isUltimateStrike)
                    {
                        StartCoroutine(LocalHitStopEffect(enemyAI));
                        //if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.15f, 0.4f);
                        Vector3 vfxPos = target != null ? target.position : transform.position + transform.forward * 2f;
                GameObject impactVFX = Instantiate(ultimateImpactVFXPrefab, vfxPos + Vector3.up * 0.1f, Quaternion.identity);
                Destroy(impactVFX, 2.5f);
                        Collider[] splashHits = Physics.OverlapSphere(target.position, 2.5f);
                        foreach (var hit in splashHits)
                        {
                            if (hit.CompareTag(enemyTag) && hit.gameObject != target.gameObject)
                            {
                                GladiatorAI nearbyEnemy = hit.GetComponent<GladiatorAI>();
                                if (nearbyEnemy != null && !nearbyEnemy.isDead)
                                {
                                    nearbyEnemy.TakeDamage(attackDamage * 0.3f, false, false); 
                                }
                            }
                        }
                    }
                }
                else 
                {
                    VillagerNPC villager = target.GetComponent<VillagerNPC>();
                    if (villager != null && !villager.IsDead)
                    {
                        villager.TakeDamage(attackDamage);
                        if (AudioManager.Instance != null) 
                            AudioManager.Instance.PlaySFX(AudioManager.Instance.hitSound, 0.6f);
                    }
                }
            }
        }
    }

    public void ProcessOnHitEffects(GladiatorAI enemyAI, float baseDamage)
    {
        if (inventory == null || inventory.activeSetPieceCount < 3) return; 

        SoldierTrait myTrait = gladiator.data.trait;

        if (inventory.activeSet == ItemSetType.Poison)
        {
            float poisonDps = baseDamage * 0.2f; 
            float pDuration = 3f;
            if (myTrait == SoldierTrait.Obur) 
            {
                poisonDps *= 2f; 
                pDuration = 5f;
            }
            enemyAI.ApplyPoison(poisonDps, pDuration);
        }
        else if (inventory.activeSet == ItemSetType.Fire)
        {
            float burnDps = baseDamage * 0.3f;
            enemyAI.ApplyBurn(burnDps, 2f);

            if (myTrait == SoldierTrait.Yetenekli) 
            {
                Collider[] hitEnemies = Physics.OverlapSphere(enemyAI.transform.position, 2.5f);
                foreach (var hit in hitEnemies)
                {
                    if (hit.CompareTag(enemyTag))
                    {
                        GladiatorAI nearbyEnemy = hit.GetComponent<GladiatorAI>();
                        if (nearbyEnemy != null && nearbyEnemy != enemyAI && !nearbyEnemy.isDead)
                        {
                            nearbyEnemy.TakeDamage(baseDamage * 0.4f, false, true); 
                        }
                    }
                }
            }
        }
        else if (inventory.activeSet == ItemSetType.Faith)
        {
            float healAmount = baseDamage * 0.2f; 
            float healRadius = 3.0f;

            if (myTrait == SoldierTrait.Dindar) 
            {
                healAmount *= 2f;
                healRadius = 6.0f; 
            }

            Collider[] allies = Physics.OverlapSphere(transform.position, healRadius);
            foreach (var hit in allies)
            {
                if (hit.CompareTag(this.gameObject.tag)) 
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
                    }
                }
            }
        }
    }

    public void TakeDamage(float incomingDamage, bool isCritical = false, bool isDoT = false)
    {
        if (isDead || gladiator.data == null) return;
        if (bloodEffectPrefab != null && !isDoT)
        {
            Vector3 bloodSpawnPos = transform.position + Vector3.up * 1.2f;
            GameObject bloodInstance = Instantiate(bloodEffectPrefab, bloodSpawnPos, Quaternion.identity);
            Destroy(bloodInstance, 1.5f); 
        }
        
        float campMorale = CampMoraleManager.Instance != null ? CampMoraleManager.Instance.currentMorale : 50f;
        float defensePower = gladiator.data.defense + (campMorale / 10.0f);
        
        float reduction = 100.0f / (100.0f + defensePower);
        float finalDamage = incomingDamage * reduction;

        gladiator.currentHealth -= finalDamage;
        GainEnergy(10f);
        if (!IsRangedWeapon && !isDoT) 
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
        if (BattleManager.Instance != null && BattleManager.Instance.currentFocusTarget == this.transform) BattleManager.Instance.ClearFocusTarget();
        if (VillageDefenseManager.Instance != null && VillageDefenseManager.Instance.currentFocusTarget == this.transform) VillageDefenseManager.Instance.ClearFocusTarget();
        
        if (AudioManager.Instance != null)
        {
            if (isBeast) AudioManager.Instance.PlaySFX(AudioManager.Instance.beastDeath, 1.0f);
            else AudioManager.Instance.PlaySFX(AudioManager.Instance.deathSound, 1.0f);
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
        BattleElementUI.Instance?.Refresh();
    }

    void FindNearestTarget()
    {
        if (forcedTarget != null)
        {
            GladiatorAI fAI = forcedTarget.GetComponent<GladiatorAI>();
            VillagerNPC fNPC = forcedTarget.GetComponent<VillagerNPC>();
            bool isForcedAlive = (fAI != null && !fAI.isDead) || (fNPC != null && !fNPC.IsDead);
            if (isForcedAlive) { target = forcedTarget; return; }
            else { forcedTarget = null; }
        }
        string targetTag = this.CompareTag("MySoldier") ? "EnemySoldier" : "MySoldier";
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag(targetTag);
        
        float minDst = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (var pt in potentialTargets)
        {
            if (pt == this.gameObject) continue;
            bool isAlive = false;

            GladiatorAI ai = pt.GetComponent<GladiatorAI>();
            if (ai != null && !ai.isDead && ai.isInBattle) isAlive = true;

            VillagerNPC villager = pt.GetComponent<VillagerNPC>();
            if (villager != null && !villager.IsDead) isAlive = true;

            if (isAlive)
            {
                float dst = Vector3.Distance(transform.position, pt.transform.position);
                if (dst < minDst) { minDst = dst; bestTarget = pt.transform; }
            }
        }

        if (target == null) { target = bestTarget; }
        else if (bestTarget != null && bestTarget != target)
        {
            float currentTargetDst = Vector3.Distance(transform.position, target.position);
            if (minDst < currentTargetDst * 0.7f) { target = bestTarget; }
        }
    }

    public void ReviveForCamp()
    {
        if (isDead) return;
        target = null; 
        GetComponent<Collider>().enabled = true;
        if (animator) { animator.Rebind(); animator.Update(0f); }
        if (agent != null) { agent.enabled = true; agent.isStopped = true; }
    }

    public void MakeGazi()
    {
        if (gladiator.data.isGazi) return;
        gladiator.data.isGazi = true;
        gladiator.data.level += 2;
        gladiator.RecalculateMaxHealth();
        if (GaziFeedback.Instance != null) GaziFeedback.Instance.ShowGaziCeremony(gladiator);
        else if (NotificationManager.Instance != null) NotificationManager.Instance.Show($"{gladiator.data.gladiatorName} artık bir GAZİ! (+2 Seviye)", NotificationType.Success);
    }

    IEnumerator LifeCycleRoutine()
    {
        while (true)
        {
            if (isDead) yield break; 
            if (isInBattle || (BattleManager.Instance != null && BattleManager.Instance.state == BattleState.Fighting))
            {
                if (currentPoint != null) LeavePoint(); 
                yield return new WaitForSeconds(0.5f); 
                continue; 
            }

            if (gladiator.isOnMission || (training != null && training.IsTraining) || (gladiator.data != null && gladiator.data.currentActivity == SoldierActivity.Working && gladiator.data.currentActivity == SoldierActivity.Praying))
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
                    else { if (animator) animator.SetBool("isRunning", true); }
                }   
                if (activityTimer <= 0) LeavePoint();
            }
            else { FindNewActivity(); }
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

    void TakePoint(ActivityPoint p) { currentPoint = p; currentPoint.isOccupied = true; agent.SetDestination(currentPoint.transform.position); activityTimer = Random.Range(10f, 20f); }
    void LeavePoint() { if (currentPoint != null) { currentPoint.isOccupied = false; currentPoint = null; } }
    void OnDestroy() { LeavePoint(); }
}