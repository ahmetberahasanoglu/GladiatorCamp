using UnityEngine;
using UnityEngine.EventSystems;

public class CampCameraController : MonoBehaviour
{
    [Header("Hareket (Pan) Ayarları")]
    public float panSpeed = 25f;
    public float panSmoothness = 10f;

    [Header("Yakınlaştırma (Zoom) Ayarları")]
    public float zoomSpeed = 20f;
    public float minZoomY = 5f;   // Yere ne kadar yaklaşsın
    public float maxZoomY = 35f;  // Havaya ne kadar çıksın
    public float zoomSmoothness = 10f;

    [Header("Döndürme (Rotation) Ayarları")]
    public float rotationSpeed = 300f;
    public float rotationSmoothness = 10f;

    private Vector3 targetPosition;
    private float targetYaw;
    private float pitchAngle;

    void Start()
    {
        // Editörde kamerayı nereye bıraktıysan orayı hedef kabul et (Zıplamayı önler)
        targetPosition = transform.position;
        targetYaw = transform.eulerAngles.y;
        pitchAngle = transform.eulerAngles.x;
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        HandleMovement();
        HandleZoomToMouse();
        HandleRotation();
    }

    void LateUpdate()
    {
        // Yumuşak geçişleri (Smoothness) uygula
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * panSmoothness);
        float smoothedYaw = Mathf.LerpAngle(transform.eulerAngles.y, targetYaw, Time.deltaTime * rotationSmoothness);

        transform.position = smoothedPosition;
        transform.rotation = Quaternion.Euler(pitchAngle, smoothedYaw, 0f);
    }

    private void HandleMovement()
    {
        // 1. WASD veya Yön Tuşları ile Haritada Gezinme
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        if (moveX != 0 || moveZ != 0)
        {
            // Kameranın baktığı açıya göre ileri/sağ vektörlerini bul (Y eksenini yoksay)
            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            // Uzaktayken hızlı, yakındayken yavaş kayması için Y yüksekliğini çarpan olarak kullanıyoruz
            float speedMultiplier = targetPosition.y / 10f; 
            Vector3 moveDir = (forward * moveZ + right * moveX).normalized;
            
            targetPosition += moveDir * panSpeed * speedMultiplier * Time.deltaTime;
        }

        // 2. Farenin Orta Tuşuna (Tekerlek) basılı tutarak haritayı kaydırma
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
    }

    private void HandleZoomToMouse()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        if (scrollData != 0)
        {
            // Ekranda imlecin olduğu noktadan yere doğru sanal bir ışın (Ray) yolluyoruz
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Y=0 düzlemi (Kamp zemini)
            
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                
                // İmlecin olduğu yere doğru olan yönü bul
                Vector3 directionToHit = hitPoint - targetPosition;

                // Zoom yap
                targetPosition += directionToHit * scrollData * zoomSpeed * Time.deltaTime;

                // Kameranın yerin dibine girmesini veya uzaya çıkmasını engelle
                targetPosition.y = Mathf.Clamp(targetPosition.y, minZoomY, maxZoomY);
            }
        }
    }

    private void HandleRotation()
    {
        // Sağ tık basılıyken döndür
        if (Input.GetMouseButton(1)) 
        {
            float mouseX = Input.GetAxis("Mouse X");
            targetYaw += mouseX * rotationSpeed * Time.deltaTime;
        }
    }
}