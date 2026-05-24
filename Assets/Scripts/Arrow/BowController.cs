using UnityEngine;

public class BowController : MonoBehaviour
{
    [Header("Nişan Ayarları")]
    public float mouseSensitivity = 2f;
    private float pitch = 0f;
    private float yaw = 0f;

    [Header("Atış Ayarları")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float maxShootForce = 40f;
    
  [Header("Germe ve Titreme")]
    public float timeToMaxDraw = 1.5f;
    private float currentDrawTime = 0f;
    private bool isDrawing = false;
    public float swayAmount = 2f;
    public float windSwayEffect = 0.5f; 

    [Header("Görsel ve İşitsel Hissiyat (Game Feel)")]
    public RectTransform crosshair;
    public float minCrosshairScale = 0.4f;
    private Vector3 originalCrosshairScale;

    public Camera mainCamera;
    public float normalFOV = 60f; // Normal kamera açısı
    public float zoomFOV = 40f;   // Gerdiğimizde ne kadar yakınlaşacak?

    public AudioSource audioSource;
    public AudioClip drawSound;   // Gıcırdayan yay (Ahşap/İp) sesi
    public AudioClip shootSound;  // Okun fırlama (Swoosh) sesi

    public float pullDistance = 0.8f; // Oku geriye doğru ne kadar çekeceğiz?
    private GameObject loadedArrow;   // O an elimizde (yayda) tuttuğumuz ok

    

   void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshair != null) originalCrosshairScale = crosshair.localScale;
        if (mainCamera == null) mainCamera = Camera.main; 

       
        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            GameObject fakeArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
            fakeArrow.transform.SetParent(arrowSpawnPoint);
            Destroy(fakeArrow); 
        }
    }

   void Update()
    {
      
        if (ArcheryGameManager.Instance != null && ArcheryGameManager.Instance.isGameOver)
        {
            return; 
        }

        Aim();
        HandleShooting();
    }

    void Aim()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -30f, 30f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }

    void HandleShooting()
    {
    
        if (Input.GetMouseButtonDown(0))
        {
            if (ArcheryGameManager.Instance != null && !ArcheryGameManager.Instance.CanShoot()) return;

            isDrawing = true;
            currentDrawTime = 0f;

            // Oku önceden yarat ve yayın ucuna tak
            if (arrowPrefab != null && arrowSpawnPoint != null)
            {
                // YENİ: Okun yaratılıp bağlanma kısmını güncelledik
                loadedArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
                
                // İkinci parametre olarak 'false' vermek, okun uzaydaki bozuk ölçekleri miras almasını engeller
                loadedArrow.transform.SetParent(arrowSpawnPoint, false); 
                
                // Garantilemek için okun yerel (local) değerlerini tamamen sıfırlıyoruz
                loadedArrow.transform.localPosition = Vector3.zero;
                loadedArrow.transform.localRotation = Quaternion.identity;
                
                // Eğer okun prefab'ında özel bir scale kullanmadıysan (genelde 1,1,1'dir) bunu da eşitle:
                loadedArrow.transform.localScale = new Vector3(4, 2f, 4f);
                
                // Fiziğini kapat ki elimizden düşmesin!
                Rigidbody rb = loadedArrow.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }

            // Gerilme sesini başlat
            if (audioSource != null && drawSound != null)
            {
                audioSource.clip = drawSound;
                audioSource.Play();
            }
        }

        // 2. BASILI TUTARKEN (GERME İŞLEMİ)
        if (Input.GetMouseButton(0) && isDrawing)
        {
            currentDrawTime += Time.deltaTime;
            float drawRatio = Mathf.Clamp01(currentDrawTime / timeToMaxDraw);
            
            // İMLECİ KÜÇÜLT
            if (crosshair != null)
            {
                float currentScaleMultiplier = Mathf.Lerp(1f, minCrosshairScale, drawRatio);
                crosshair.localScale = originalCrosshairScale * currentScaleMultiplier;
            }

            // KAMERAYI YAKINLAŞTIR (ZOOM YAP)
            if (mainCamera != null)
            {
                mainCamera.fieldOfView = Mathf.Lerp(normalFOV, zoomFOV, drawRatio);
            }

            // OKU GERİYE ÇEK (Görsel Yayın Gerilmesi)
            if (loadedArrow != null)
            {
                // Okun Z ekseninde (kendi gerisine doğru) hareket ettiriyoruz
                loadedArrow.transform.localPosition = new Vector3(0, 0, -drawRatio * pullDistance);
            }

            if (currentDrawTime > timeToMaxDraw)
            {
                // Yorulmadan kaynaklı rastgele titreme
                float swayX = Mathf.PerlinNoise(Time.time * 10f, 0f) - 0.5f;
                float swayY = Mathf.PerlinNoise(0f, Time.time * 10f) - 0.5f;
                
                // Rüzgardan kaynaklı savrulma (WindManager'dan çekiyoruz)
                float windSwayX = 0f;
                float windSwayY = 0f;

                if (WindManager.Instance != null)
                {
                    Vector3 currentWind = WindManager.Instance.GetWindForce();
                    // Rüzgarın X (Sol/Sağ) ve Z (Ön/Arka) yönlerini kameranın Yaw/Pitch değerlerine yediriyoruz
                    windSwayX = currentWind.x * windSwayEffect; 
                    windSwayY = currentWind.z * windSwayEffect; 
                }

                // Hem yorulmayı hem rüzgarı ekle
                pitch += (swayY * swayAmount + windSwayY) * Time.deltaTime;
                yaw += (swayX * swayAmount + windSwayX) * Time.deltaTime;
            }
        }

        // 3. BIRAKTIĞIMIZDA (FIRLATMA)
        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            Shoot();
        }
    }

    void Shoot()
    {
        // Görselleri ve Kamerayı Sıfırla
        if (crosshair != null) crosshair.localScale = originalCrosshairScale;
        if (mainCamera != null) mainCamera.fieldOfView = normalFOV;

        // Ses efektini değiştir
        if (audioSource != null)
        {
            audioSource.Stop(); // Gerilme sesini kes
            if (shootSound != null) audioSource.PlayOneShot(shootSound); // Fırlama sesini çal
        }

        // Gerçek Fırlatma İşlemi
        float drawPower = Mathf.Clamp01(currentDrawTime / timeToMaxDraw);
        if (drawPower < 0.2f) drawPower = 0.2f;

        if (loadedArrow != null)
        {
            loadedArrow.transform.SetParent(null); // Oku yaydan kopar (Özgür bırak)
            
            Rigidbody rb = loadedArrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Fiziği tekrar aç (Yerçekimi etki etsin)
                rb.AddForce(transform.forward * (drawPower * maxShootForce), ForceMode.Impulse);
            }
            
            loadedArrow = null; // Elimizi boşalt
        }

        currentDrawTime = 0f;

        if (ArcheryGameManager.Instance != null)
        {
            ArcheryGameManager.Instance.RegisterShot();
        }
    }
}