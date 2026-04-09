using UnityEngine;
using UnityEngine.EventSystems;

public class CampCameraController : MonoBehaviour
{
    [Header("Hareket (Pan) Ayarları")]
    public float panSpeed = 25f;
    public float panSmoothness = 10f;

    [Header("Harita Sınırları (Limits)")]
    public bool useLimits = true;
    public float minX = -30f;
    public float maxX = 50f;
    public float minZ = -40f;
    public float maxZ = 40f;

    [Header("Yakınlaştırma (Zoom) Ayarları")]
    public float zoomSpeed = 20f;
    public float minZoomY = 5f;   
    public float maxZoomY = 35f;  
    public float zoomSmoothness = 10f;

    [Header("Döndürme (Rotation) Ayarları")]
    public float rotationSpeed = 300f;
    public float rotationSmoothness = 10f;

    private Vector3 targetPosition;
    private float targetYaw;
    private float pitchAngle;


    public bool isLocked = false;

    void Start()
    {
        targetPosition = transform.position;
        targetYaw = transform.eulerAngles.y;
        pitchAngle = transform.eulerAngles.x;
    }

    void Update()
    {
 
        if (isLocked || EventSystem.current.IsPointerOverGameObject()) return;

        HandleMovement();
        HandleZoomToMouse();
        HandleRotation();
    }

    void LateUpdate()
    {
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * panSmoothness);
        float smoothedYaw = Mathf.LerpAngle(transform.eulerAngles.y, targetYaw, Time.deltaTime * rotationSmoothness);

        transform.position = smoothedPosition;
        transform.rotation = Quaternion.Euler(pitchAngle, smoothedYaw, 0f);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        if (moveX != 0 || moveZ != 0)
        {
            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            float speedMultiplier = targetPosition.y / 10f; 
            Vector3 moveDir = (forward * moveZ + right * moveX).normalized;
            
            targetPosition += moveDir * panSpeed * speedMultiplier * Time.deltaTime;
        }

        if (Input.GetMouseButton(2)) 
        {
            float mouseX = -Input.GetAxis("Mouse X");
            float mouseY = -Input.GetAxis("Mouse Y");
            
            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            targetPosition += (right * mouseX + forward * mouseY) * panSpeed * Time.deltaTime;
        }

        ApplyLimits(); 
    }

    private void HandleZoomToMouse()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        if (scrollData != 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); 
            
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 directionToHit = hitPoint - targetPosition;

                targetPosition += directionToHit * scrollData * zoomSpeed * Time.deltaTime;
                targetPosition.y = Mathf.Clamp(targetPosition.y, minZoomY, maxZoomY);
                
                ApplyLimits();
            }
        }
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1)) 
        {
            float mouseX = Input.GetAxis("Mouse X");
            targetYaw += mouseX * rotationSpeed * Time.deltaTime;
        }
    }


    private void ApplyLimits()
    {
        if (!useLimits) return;
        
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
    }

  
    public void TeleportTo(Transform newTarget, bool lockCamera)
    {
        targetPosition = newTarget.position;
        targetYaw = newTarget.eulerAngles.y;
        pitchAngle = newTarget.eulerAngles.x;

        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(pitchAngle, targetYaw, 0f);

        isLocked = lockCamera;
    }
}