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
        animator = GetComponent<Animator>();
        agent.speed = 3.5f + (gladiator.data.speed * 0.05f);//değişebilirim.
        // Başlangıçta hemen bir iş bulmasın, biraz beklesin
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

        // Mesafe Kontrolü (NavMeshAgent kapalı olsa bile çalışır)
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            // --- SALDIRI ---
            if (agent.isActiveAndEnabled) agent.isStopped = true;
            if (animator) animator.SetBool("isRunning", false);

            // Vuruş yönüne dön
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

        // Karşıdaki askerin AI scriptini bul
        if (target != null)
        {
            GladiatorAI enemyAI = target.GetComponent<GladiatorAI>();
            if (enemyAI != null && gladiator.data != null)
            {
                // --- HASAR FORMÜLÜ (Senin JanissaryData değerlerine göre) ---
                
                // 1. Temel Hasar: Strength * 1.5
                float damage = gladiator.data.strength * 1.5f;

                // 2. Seviye Bonusu
                damage += (gladiator.data.level * 2);

                // 3. Kritik Vuruş (Speed etkili)
                // Speed değeri kadar yüzde şans (Örn: 20 Speed = %20 Kritik)
                bool isCrit = Random.Range(0, 100) < gladiator.data.speed;
                if (isCrit)
                {
                    damage *= 1.5f; // %50 Daha fazla vur
                    // Debug.Log("Kritik Vuruş!");
                }

                // Hasarı gönder
                enemyAI.TakeDamage(damage,isCrit);
            }
        }
    }

    public void TakeDamage(float incomingDamage,bool isCritical = false)
    {
        if (isDead || gladiator.data == null) return;

        // --- SAVUNMA FORMÜLÜ ---
        
        // Defans Puanı: Defense + (Morale / 5)
        // Morale 100 ise +20 Defans sağlar.
        float defensePower = gladiator.data.defense + (gladiator.data.morale / 5.0f);

        // Hasar Azaltma Oranı (LoL Mantığı)
        // 50 Defans = %33 Azaltma, 100 Defans = %50 Azaltma
        float reduction = 100.0f / (100.0f + defensePower);
        
        float finalDamage = incomingDamage * reduction;

        // Canı azalt (Gladiator scriptindeki currentHealth'ten)
        gladiator.currentHealth -= finalDamage;
        if (gladiator.healthBar != null)
        {
            gladiator.healthBar.UpdateBar(gladiator.currentHealth, gladiator.maxHealth);
        }

        if (DamageTextManager.Instance != null)
        {
            // Hasarı alan askerin pozisyonunda yazıyı çıkar
            DamageTextManager.Instance.ShowDamage(transform.position, finalDamage, isCritical);
        }

        if (gladiator.currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        if (agent.isActiveAndEnabled) agent.isStopped = true;
        
        GetComponent<Collider>().enabled = false; // Tıklanmayı kapat
        if (agent.isActiveAndEnabled) agent.enabled = false; // Yolu aç
        
        if (animator) animator.SetTrigger("Die");

        Destroy(gameObject, 4f); // 4 saniye sonra cesedi sil
    }

    void FindNearestTarget()
    {
        // ... (Bu fonksiyon öncekiyle aynı kalabilir) ...
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float minDst = Mathf.Infinity;
        GameObject nearest = null;
        
        foreach (GameObject e in enemies)
        {
            // Kendisi değilse ve ölmediyse
            var ai = e.GetComponent<GladiatorAI>();
            if (ai != null && !ai.isDead)
            {
                float dst = Vector3.Distance(transform.position, e.transform.position);
                if (dst < minDst) { minDst = dst; nearest = e; }
            }
        }
        target = nearest != null ? nearest.transform : null;
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