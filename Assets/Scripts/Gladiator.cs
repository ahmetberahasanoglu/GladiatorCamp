using UnityEngine;
using UnityEngine.AI;
using System;
public class Gladiator : MonoBehaviour
{
    public JanissaryData data;
    private NavMeshAgent agent;
    public event Action OnStatsChanged;
    public bool isOnMission = false; 

    // Asker müsait mi? (Hem eğitimde değil, hem seferde değilse)
    public bool IsAvailable 
    {
        get 
        {
            var training = GetComponent<GladiatorTraining>();
            bool isTraining = training != null && training.IsTraining;
            
            return !isOnMission && !isTraining;
        }
    }   
    public void RefreshStats()
    {
        // Abone olan (Dinleyen) biri varsa (?.), haberi ilet (Invoke)
        OnStatsChanged?.Invoke();
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
       // GoTo(GameObject.Find("Target").transform.position);
    }


    public void GoTo(Vector3 target)
    {
        agent.SetDestination(target);
    }
}
