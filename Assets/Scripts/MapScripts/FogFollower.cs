using UnityEngine;

public class FogFollower : MonoBehaviour
{
    public RectTransform playerIcon;
    public float followSpeed = 5f;
    private RectTransform myRect;

    void Start()
    {
        myRect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (playerIcon != null)
        {
            // Atlı sarsılarak (wiggle) yürüse bile, karanlık maske yumuşakça ve sarsılmadan atlıyı takip eder
            myRect.position = Vector3.Lerp(myRect.position, playerIcon.position, Time.deltaTime * followSpeed);
        }
    }
}