using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GladiatorHealing : MonoBehaviour
{
    // Asker şu an şifahanede mi? (Eğitim kontrolündeki gibi)
    public bool IsHealing => remainingDays > 0;

    private NavMeshAgent agent;
    private Gladiator gladiator;
    private HealingSpot currentSpot;
    private int remainingDays = 0;

    [Header("Görsel Efekt")]
    public ParticleSystem healParticle; // İyileşince çıkacak yeşil artılar vb.

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
    }

    public void StartHealing(HealingSpot spot)
    {
        if (IsHealing) return;
        if (spot == null || spot.isBusy) return;

        currentSpot = spot;
        spot.isBusy = true;
        
        // Şifahanenin kaç gün süreceğini alıyoruz
        remainingDays = spot.timeCost;

        // Şifahaneye yürümeye başla
        agent.SetDestination(spot.healingPoint.position);
        StartCoroutine(WalkAndBeginHealing());
    }

    IEnumerator WalkAndBeginHealing()
    {
        // 1. Yolun hesaplanmasını bekle
        while (agent.pathPending) yield return null;
        
        // 2. Çadıra varana kadar bekle
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // --- ÇADIRA VARDI VE BEKLEMEYE BAŞLADI ---

        // (İsteğe bağlı: Eğitimdeki gibi ekranda bir bar çıkmasını istersen alttaki satırı açabilirsin)
        // UITrainingProgress.Instance.StartProgress(remainingDays);

        // Manuel "Sonraki Gün" butonuna basılmasını beklemek için olaya (event) abone oluyoruz!
        DayManager.Instance.OnNewDay += OnNewDay;
    }

    // Sen UI'dan "Next Day" butonuna bastığında otomatik burası tetiklenecek
    void OnNewDay()
    {
        if (!IsHealing) return;

        remainingDays--;

        // UITrainingProgress.Instance.UpdateProgress(remainingDays);

        // Süre doldu mu?
        if (remainingDays <= 0)
        {
            FinishHealing();
        }
    }

    void FinishHealing()
    {
        // NOT: Senin "HealAllSoldiers(20)" fonksiyonun zaten NextDay butonuna basınca 
        // global olarak çalıştı ve bu askere de 20 can verdi. 
        // BİZ BURADA SADECE ŞİFAHANENİN VERDİĞİ "EKSTRA" CANI EKLİYORUZ.

        /* KENDİ KODUNA GÖRE UYARLA:
        gladiator.data içindeki can değişkeninin adı neyse (currentHp, health vb.) aşağıyı düzelt.
        Örnek:
        */
        gladiator.currentHealth += currentSpot.extraHealAmount;
        if (gladiator.currentHealth > gladiator.currentHealth) 
        {
            gladiator.currentHealth = gladiator.currentHealth;
        }
        

        // Efekti patlat
        if (healParticle != null) healParticle.Play();

        // Ekrana bildirim ver
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show($"{gladiator.data.gladiatorName} şifahaneden taburcu oldu! (+{currentSpot.extraHealAmount} Bonus HP)", NotificationType.Success);

        // UI panellerini yenile
        gladiator.RefreshStats();
        
        // Çadırı sıradaki hasta için boşalt
        currentSpot.isBusy = false;
        currentSpot = null;

        // Aboneliği iptal et ki sonsuza kadar can basmasın
        DayManager.Instance.OnNewDay -= OnNewDay;


        // UITrainingProgress.Instance.Hide();
    }
}