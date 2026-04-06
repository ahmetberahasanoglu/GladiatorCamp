using UnityEngine;
using UnityEngine.EventSystems;

public class CampCameraController : MonoBehaviour
{
    [Header("Hedef (Kampın Ortasındaki Boş Obje)")]
    public Transform cameraPivot;

    [Header("Yakınlaştırma (Zoom) Ayarları")]
    public float zoomSpeed = 5f;
    public float minDistance = 5f;   
    public float maxDistance = 40f;  
    public float zoomSmoothness = 10f;

    [Header("Döndürme (Rotation) Ayarları")]
    public float rotationSpeed = 300f;
    public float rotationSmoothness = 10f;

    private float pitchAngle; 
    private float currentDistance;
    private float targetDistance;
    
    private float currentYaw;
    private float targetYaw;

    void Start()
    {
        if (cameraPivot == null) return;

        // 1. Senin kamerayı koyduğun yer ile pivot arasındaki farkı (vektörü) bul
        Vector3 offset = transform.position - cameraPivot.position;
        
        // 2. Başlangıç mesafesini senin koyduğun yere göre kesin olarak hesapla
        targetDistance = offset.magnitude;
        currentDistance = targetDistance;

        // 3. Kameranın zıplamaması için, açıları senin pozisyonuna göre TERSİNE MÜHENDİSLİKLE bul
        Quaternion exactLookRotation = Quaternion.LookRotation(-offset);
        
        pitchAngle = exactLookRotation.eulerAngles.x;
        targetYaw = exactLookRotation.eulerAngles.y;
        currentYaw = targetYaw;

        // 4. Kameranın başlangıçta milimetrik olarak pivota bakmasını garantile
        transform.rotation = exactLookRotation;
    }

    void Update()
    {
        if (cameraPivot == null) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        HandleZoom();
        HandleRotation();
    }

    void LateUpdate()
    {
        if (cameraPivot == null) return;

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSmoothness);
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * rotationSmoothness);

        Quaternion rotation = Quaternion.Euler(pitchAngle, currentYaw, 1.169f);
        Vector3 position = cameraPivot.position - (rotation * Vector3.forward * currentDistance);

        transform.position = position;
        transform.rotation = rotation;
    }

    private void HandleZoom()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        if (scrollData != 0)
        {
            targetDistance -= scrollData * zoomSpeed * 10f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
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
}