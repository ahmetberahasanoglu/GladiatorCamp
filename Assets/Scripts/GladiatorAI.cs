using UnityEngine;
using UnityEngine.AI;

public class GladiatorAI : MonoBehaviour
{
    public GladiatorTraining training;
    public NavMeshAgent agent;

    public float wanderRadius = 5f;
    public float idleTime = 2f;

    private float timer;

    void Start()
    {
        timer = idleTime;
    }

    void Update()
    {
        if (training.isTraining) return; 

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            timer = idleTime + Random.Range(0, 3f);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, -1);
        return navHit.position;
    }
}
