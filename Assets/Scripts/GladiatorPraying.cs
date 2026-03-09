using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GladiatorPraying : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private Gladiator gladiator;
    
    public PrayingSpot currentSpot;
    public bool isPraying = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        gladiator = GetComponent<Gladiator>();
    }

    public void StartPraying(PrayingSpot spot)
    {
        // Eğer zaten bir seccadedeyse orayı boşalt
        if (currentSpot != null) currentSpot.isOccupied = false;

        currentSpot = spot;
        currentSpot.isOccupied = true;
        currentSpot.currentSoldier = gladiator;

        // Askerin durumunu güncelle (Çalışmaya veya savaşa gitmesini engellemek için)
        if (gladiator != null && gladiator.data != null)
            gladiator.data.currentActivity = SoldierActivity.Praying; 

        isPraying = true;
        StartCoroutine(WalkAndPrayRoutine());
    }

    IEnumerator WalkAndPrayRoutine()
    {
        // 1. Camiye Doğru Koş
        if (anim != null) anim.SetBool("isRunning", true);
        agent.isStopped = false;
        agent.SetDestination(currentSpot.transform.position);

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // 2. Hedefe Vardı
        agent.isStopped = true;
        if (anim != null) anim.SetBool("isRunning", false);

        // 3. Seccadenin Yönüne Dön (Kıble Mantığı)
        transform.rotation = currentSpot.transform.rotation;

        // 4. Animasyona Gir (Animator'ında Sit veya Pray adında bir bool parametresi olmalı)
        if (anim != null) anim.SetBool("isSitting", true);

        // 5. DayManager'a Abone Ol (Her gün geçişinde Nasip üretecek)
        DayManager.Instance.OnNewDay -= GenerateNasip; // Çifte aboneliği engelle
        DayManager.Instance.OnNewDay += GenerateNasip;
    }

    // Her "Next Day" butonuna basıldığında otomatik çalışır
    // Her "Next Day" butonuna basıldığında otomatik çalışır
    private void GenerateNasip()
    {
        if (isPraying && currentSpot != null)
        {
            // 1. Ödülü Ver (Nasip kazandır)
            if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(2);
            
            // Eğer istersen asker ibadet ettiği için ufak bir can da kazanabilir
            // gladiator.currentHealth += 10;

            // --- YENİ EKLENEN KISIM: İbadet bitti, askeri serbest bırak ---
            StopPraying(); 
        }
    }

    // Askeri savaşa veya başka bir yere gönderdiğinde bu fonksiyonu çağırmalısın
   public void StopPraying()
    {
        if (!isPraying) return;

        isPraying = false;
        
        if (currentSpot != null)
        {
            currentSpot.isOccupied = false;
            currentSpot.currentSoldier = null;
            currentSpot = null;
        }

        if (anim != null) anim.SetBool("isSitting", false);

        if (DayManager.Instance != null) DayManager.Instance.OnNewDay -= GenerateNasip;

        if (gladiator != null && gladiator.data != null)
            gladiator.data.currentActivity = SoldierActivity.Idling;
            gladiator.RefreshStats();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false; 
            agent.ResetPath(); // Eski hedefleri tamamen temizle
        }
    }

    void OnDestroy()
    {
        // Asker ölürse veya silinirse DayManager'dan aboneliğini mutlaka düş
        if (DayManager.Instance != null) DayManager.Instance.OnNewDay -= GenerateNasip;
    }
}