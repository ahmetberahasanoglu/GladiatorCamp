using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GladiatorTraining : MonoBehaviour
{
    public bool isTraining = false;
    private NavMeshAgent agent;
    private Gladiator gladiator;  // referans; gladiator.data üzerinden statlara eriþ

    private TrainingSpot currentSpot;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();
    }

    public void StartTraining(TrainingSpot spot)
    {
        if (isTraining) return;
        if (gladiator == null || spot == null || spot.isBusy) return;

        isTraining = true;
        currentSpot = spot;
        spot.isBusy = true; 

        agent.SetDestination(spot.trainingPoint.position);
        StartCoroutine(TrainingRoutine());
    }

    IEnumerator TrainingRoutine()
    {
        Debug.Log("Training started for: " );
        //while (Vector3.Distance(transform.position, currentSpot.trainingPoint.position) > 0.5f)
          //  yield return null;
        //þey istiyorum. Eðitim alanýna gidince progress bar baþlasýn ama yukarýdaki kod çalýþýnca direkt döngüden çýktýgýmýzdan istenildigi gibi calýstmýyor


        float duration = GetTrainingDuration();
        float timer = 0f;

        // UI progress baþlat
        UITrainingProgress.Instance.StartProgress(duration);
        Debug.Log("Progress bar acilmali: ");
        while (timer < duration)
        {
            timer += Time.deltaTime;
            UITrainingProgress.Instance.UpdateProgress(timer / duration);
            yield return null;
        }

        // Stat artýþý: TrainingSpot.trainingType'a göre uygulama
        ApplyStatGain(currentSpot.trainingType, currentSpot.statGain);
        Debug.Log("stat artisi yasanmali: ");
        UITrainingProgress.Instance.Hide();

        isTraining = false;
        currentSpot.isBusy = false;
        currentSpot = null;
        gladiator.data.level += 1;
    }

    public float GetTrainingDuration()
    {
        return gladiator.data.baseTrainingTime + (gladiator.data.level * 1.5f);
    }

    private void ApplyStatGain(TrainingType type, int amount)
    {
        var d = gladiator.data;
        switch (type)
        {
            case TrainingType.Strength:
                d.strength += amount;
                break;
            case TrainingType.Speed:
                d.speed += amount;
                break;
            case TrainingType.Defense:
                d.defense += amount;
                break;
            case TrainingType.Morale:
                d.morale += amount;
                break;
            case TrainingType.Stamina:
                d.stamina += amount;
                break;
        }
    }
}
