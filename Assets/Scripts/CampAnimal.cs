using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NavMeshAgent))] // Bu script atıldığında otomatik NavMeshAgent ekler
public class CampAnimal : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Dolaşma Ayarları")]
    public float wanderRadius = 15f; // Kendi etrafında ne kadar uzağa gidebilir
    public float minWaitTime = 2f;   // Gittiği yerde en az bekleme süresi
    public float maxWaitTime = 7f;   // Gittiği yerde en çok bekleme süresi

    [Header("Etkileşim Ayarları")]
    public GameObject loveEffectPrefab; // Tıklayınca çıkacak Kalp efekti
    public int moraleBoost = 1;         // Vereceği moral
    public float interactionCooldown = 60f; // Sınırsız sevilmesin diye (Örn: 60 saniyede bir)

    private float nextInteractionTime = 0f;
     public AudioSource barkSound;

    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        
        // Hayvanlar askerlerden biraz daha yavaş ve rahat yürüsün
        agent.speed = 2f; 

        StartCoroutine(WanderRoutine());
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            // 1. Gittiği yerde bekle (Dinlenme / Etrafı koklama)
            if (animator) animator.SetBool("isRunning", false); // Senin animasyon parametrene göre değiştir
            
            float wait = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(wait);

            // 2. Yeni hedef bul
            Vector3 newPos = GetRandomNavmeshLocation(wanderRadius);
            agent.SetDestination(newPos);
            
            if (animator) animator.SetBool("isRunning", true);

            // 3. Hedefe varana kadar bekle
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null; // Yürümeye devam et
            }
        }
    }

    // NavMesh üzerinde rastgele ve GEÇERLİ bir nokta bulur
    Vector3 GetRandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        
        // Rastgele seçilen nokta yürünebilir bir yer mi diye kontrol eder
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return transform.position; // Bulamazsa olduğu yerde kalsın
    }

    // Fareyle hayvana tıklandığında
    void OnMouseDown()
    {   
       
        if (EventSystem.current.IsPointerOverGameObject()) return;
        barkSound.Play();
        // Hayvanı sevme süresi gelmiş mi?
        if (Time.time >= nextInteractionTime)
        {
          
            nextInteractionTime = Time.time + interactionCooldown;

            // 1. Moral Ver
            if (CampMoraleManager.Instance != null)
            {
                CampMoraleManager.Instance.ChangeMorale(moraleBoost);
            }

            // 2. Bildirim
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.Show("Kamp maskotunu sevdin! Askerlerin içi ısındı.", NotificationType.Success);
            }

            // 3. Kalp Efekti
            if (loveEffectPrefab != null)
            {
                // Köpeğin biraz üstünde çıksın
                GameObject fx = Instantiate(loveEffectPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }
        else
        {
            // Bekleme süresindeyse moral vermesin, sadece sevildiğini bilsin
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.Show("Hayvan daha fazla sevilmek istemiyor.", NotificationType.Info);
            }
        }
    }
}