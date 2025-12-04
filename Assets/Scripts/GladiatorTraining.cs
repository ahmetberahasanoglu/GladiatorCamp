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

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
    }

    // UI butonundan çağrılır
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
      
        // Noktaya varmasını bekle
       // while (Vector3.Distance(transform.position, currentSpot.trainingPoint.position) > 0.6f)
        //    yield return null;

        // Animasyon ileride eklenecek

        // GÜN BAZLI eğitim başlıyor
        UITrainingProgress.Instance.StartProgress(remainingDays);

        DayManager.Instance.OnNewDay += OnNewDay;
        yield return null;
    }

    void OnNewDay()
    {
        if (!IsTraining) return;

        remainingDays--;

        UITrainingProgress.Instance.UpdateProgress(remainingDays);

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
        UITrainingProgress.Instance.Hide();
    }

    int GetRequiredDays(int level)
    {
        if (level <= 5) return 1;
        if (level <= 10) return 2;
        return 3;
    }

    private void ApplyStatGain(TrainingType type, int amount)
    {
        var d = gladiator.data;

        switch (type)
        {
            case TrainingType.Strength: d.strength += amount; break;
            case TrainingType.Speed: d.speed += amount; break;
            case TrainingType.Defense: d.defense += amount; break;
            case TrainingType.Morale: d.morale += amount; break;
            case TrainingType.Stamina: d.stamina += amount; break;
        }

        Debug.Log($"Stat Artışı → {type} +{amount}");
    }
}
