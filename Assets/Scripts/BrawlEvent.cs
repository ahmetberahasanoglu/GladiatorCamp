using UnityEngine;
using UnityEngine.EventSystems;

public class BrawlEvent : MonoBehaviour
{
    private Gladiator fighter1;
    private Gladiator fighter2;
    private float timeLeft;
    private bool isResolved = false;

    public float iconHeightOffset = 4.0f; 
    public float clashDistance = 1.0f; 

    public void Setup(Gladiator s1, Gladiator s2, float duration)
    {
        fighter1 = s1;
        fighter2 = s2;
        timeLeft = duration;

        if (fighter1 != null && fighter2 != null)
        {
            // Beyinleri tamamen kapat, böylece adamlar felç kalır ve kıpırdayamaz
            ToggleAI(fighter1, false);
            ToggleAI(fighter2, false);

            Vector3 midPoint = (fighter1.transform.position + fighter2.transform.position) / 2f;
            
            // Sadece BİR KERE pozisyonla ve birbirlerine baktır. 
            // Beyinleri kapalı olduğu için bunu Update'te sürekli yapmaya gerek yok!
            fighter1.transform.position = Vector3.MoveTowards(fighter1.transform.position, midPoint, Vector3.Distance(fighter1.transform.position, midPoint) - (clashDistance/2f));
            fighter2.transform.position = Vector3.MoveTowards(fighter2.transform.position, midPoint, Vector3.Distance(fighter2.transform.position, midPoint) - (clashDistance/2f));
            
            fighter1.transform.LookAt(fighter2.transform);
            fighter2.transform.LookAt(fighter1.transform);

            this.transform.position = midPoint + Vector3.up * iconHeightOffset; 

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.swordHit, 0.8f);
        }
    }

    void Update()
    {
        if (isResolved) return;

        if (CampBrawlManager.Instance != null && CampBrawlManager.Instance.isMapOpen) return;

        if (fighter1 == null || fighter2 == null || fighter1.data.currentActivity != SoldierActivity.Idling || fighter2.data.currentActivity != SoldierActivity.Idling)
        {
            CancelBrawlSafely();
            return;
        }

        // --- OPTİMİZASYON: Pozisyon zorlamaları silindi, Update artık çok hafif! ---

        timeLeft -= Time.deltaTime;

        // İkonu nabız gibi attır (Çok düşük maliyetli)
        float pulse = Mathf.PingPong(Time.time * 2f, 0.2f);
        transform.localScale = Vector3.one * (1f + pulse);

        if (timeLeft <= 0)
        {
            ResolveBrawl(false);
        }
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject() || isResolved) return;
        if (CampBrawlManager.Instance != null && CampBrawlManager.Instance.isMapOpen) return;
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        ResolveBrawl(true); 
    }

    private void ToggleAI(Gladiator gladiator, bool state)
    {
        if (gladiator == null) return;

        GladiatorAI ai = gladiator.GetComponent<GladiatorAI>();
        if (ai != null) ai.enabled = state;

        UnityEngine.AI.NavMeshAgent agent = gladiator.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = !state;
            if (!state) agent.ResetPath();
        }
    }

    private void ReleaseFighters()
    {
        ToggleAI(fighter1, true);
        ToggleAI(fighter2, true);
    }

    private void CancelBrawlSafely()
    {
        isResolved = true;
        ReleaseFighters();

        if (CampBrawlManager.Instance != null)
        {
            if (fighter1 != null) CampBrawlManager.Instance.activeBrawlers.Remove(fighter1);
            if (fighter2 != null) CampBrawlManager.Instance.activeBrawlers.Remove(fighter2);
        }
        Destroy(gameObject);
    }

    private void ResolveBrawl(bool playerIntervened)
    {
        isResolved = true;

        if (fighter1 == null || fighter2 == null || fighter1.data == null || fighter2.data == null) 
        {
            CancelBrawlSafely();
            return;
        }

        ReleaseFighters();

        if (CampBrawlManager.Instance != null)
        {
            CampBrawlManager.Instance.activeBrawlers.Remove(fighter1);
            CampBrawlManager.Instance.activeBrawlers.Remove(fighter2);
        }

        if (playerIntervened)
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("<color=yellow>DİSİPLİN!</color> Askerlerin arasına girdin ve kavgayı büyümeden ayırdın.", NotificationType.Info);
        }
        else
        {
            fighter1.currentHealth = Mathf.Max(1f, fighter1.currentHealth - 15f);
            fighter2.currentHealth = Mathf.Max(1f, fighter2.currentHealth - 15f);
            
            if (fighter1.healthBar != null) fighter1.healthBar.UpdateBar(fighter1.currentHealth, fighter1.maxHealth);
            if (fighter2.healthBar != null) fighter2.healthBar.UpdateBar(fighter2.currentHealth, fighter2.maxHealth);
            
            fighter1.RefreshStats();
            fighter2.RefreshStats();

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.gruntSound, 1f);
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5);
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("<color=red>KAN DÖKÜLDÜ!</color> Askerler birbirine girdi ve yaralandılar. Kamp morali düştü!", NotificationType.Error);
        }
        Destroy(gameObject);
    }
}