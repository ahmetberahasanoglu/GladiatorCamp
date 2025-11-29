using UnityEngine;
using UnityEngine.AI;

public class Gladiator : MonoBehaviour
{
    public GladiatorData data;
    private NavMeshAgent agent;

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
