using UnityEngine;
using UnityEngine.AI;

public class Gladiator : MonoBehaviour
{
    public JanissaryData data;
    private NavMeshAgent agent;
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
