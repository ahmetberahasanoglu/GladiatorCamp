using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GladiatorTraining : MonoBehaviour
{
    public bool IsTraining => remainingDays > 0;

    private NavMeshAgent agent;
    private Gladiator gladiator;

    private TrainingSpot currentSpot;
    private int remainingDays = 0;
    private TrainingType currentTrainingType;

    [Header("Görsel Geri Bildirim")]
    public GameObject selectionRing;

    [Header("Efektler")]
public ParticleSystem levelUpParticle;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();

        if (selectionRing != null) selectionRing.SetActive(false);
    }
    void OnMouseDown()
    {
        // Asker eğitimdeyse seçilemesin
        if (IsTraining)
        {
            if (NotificationManager.Instance != null) 
                NotificationManager.Instance.Show("Bu asker şu an eğitimde, rahatsız etme!", NotificationType.Warning);
            return;
        }

        // Kendi kendini yöneticiye "Beni seçti" diye bildirir
        if (TrainingUIManager.Instance != null)
        {
            TrainingUIManager.Instance.SetCurrentGladiator(this);
        }
    }
    public void SetSelectedVisual(bool isSelected)
    {
        if (selectionRing != null)
        {
            selectionRing.SetActive(isSelected);
        }
    }

    // 3D Obje (TrainingSpot) tıklandığında çağrılır
    public void StartTraining(TrainingSpot spot)
    {
        if (IsTraining) return;
        if (spot == null || spot.isBusy) return;

        currentSpot = spot;
        spot.isBusy = true;

        currentTrainingType = spot.trainingType;
        remainingDays = GetRequiredDays(gladiator.data.level);

        // Eğitim yerine yürüsün
        agent.SetDestination(spot.trainingPoint.position);

        // Noktaya varışın takibi
        StartCoroutine(WalkAndBeginTraining());
    }

    IEnumerator WalkAndBeginTraining()
    {
        // 1. NavMesh yolunu hesaplayana kadar bekle
        while (agent.pathPending) yield return null;
        
        // 2. Noktaya varana kadar bekle
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // Hedefe vardı! Animasyonu başlatabilirsin
        // GetComponent<Animator>().SetBool("IsTraining", true);

        // GÜN BAZLI eğitim başlıyor (Tıkladığın an gün GEÇMEZ)
       // UITrainingProgress.Instance.StartProgress(remainingDays);

        DayManager.Instance.OnNewDay += OnNewDay;
    }

    void OnNewDay()
    {
        if (!IsTraining) return;

        remainingDays--;

       // UITrainingProgress.Instance.UpdateProgress(remainingDays);

        if (remainingDays <= 0)
        {
            FinishTraining();
        }
    }

    void FinishTraining()
    {
        ApplyStatGain(currentTrainingType, currentSpot.statGain);

        gladiator.data.level += 1;

        currentSpot.isBusy = false;
        currentSpot = null;

        DayManager.Instance.OnNewDay -= OnNewDay;
        gladiator.RefreshStats();
        //UITrainingProgress.Instance.Hide();
        // Eğitim bitince coşkulu bir partikül patlat!
if (levelUpParticle != null)
{
    levelUpParticle.Play();
}
        // Eğitim bitti animasyonunu kapatabilirsin
        // GetComponent<Animator>().SetBool("IsTraining", false);
    }

    int GetRequiredDays(int level)
    {
        if (level <= 5) return 1;
        if (level <= 10) return 2;
        return 3;
    }

    private void ApplyStatGain(TrainingType type, int amount)
    {
        // GÜNCEL KISIM: Statı direkt data'ya yazarsak kılıç çıkardığında silinir.
        // O yüzden Inventory'deki Base (Ham) statları güncelleyen o yeni fonksiyonu çağırıyoruz!
        GladiatorInventory inv = GetComponent<GladiatorInventory>();
        if (inv != null)
        {
            inv.PermanentlyIncreaseStat(type, amount);
        }
        else
        {
            // Eğer bir sebeple Inventory yoksa (ki olmalı), yine de güvenli liman olarak data'ya yazsın
            switch (type)
            {
                case TrainingType.Strength: gladiator.data.strength += amount; break;
                case TrainingType.Speed: gladiator.data.speed += amount; break;
                case TrainingType.Defense: gladiator.data.defense += amount; break;
                case TrainingType.Stamina: gladiator.data.stamina += amount; break;
            }
        }

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show($"Eğitim Tamamlandı! {type} +{amount}", NotificationType.Success);
    }
}