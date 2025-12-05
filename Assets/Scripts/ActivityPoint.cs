using UnityEngine;

public class ActivityPoint : MonoBehaviour
{
   
    public enum PointType { Idle, Eating, Praying, Guarding }
    public PointType activityType;

    public bool isOccupied = false; 

    void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        // Unity Editor kütüphanesini kullanmadığımız için text yazdırmıyorum ama renk verebiliriz.
        if(activityType == PointType.Eating) Gizmos.color = Color.yellow;
    }
}