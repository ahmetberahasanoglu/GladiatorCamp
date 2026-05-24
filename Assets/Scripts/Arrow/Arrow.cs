using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody rb;
    private bool isStuck = false;
    public AudioClip hitSound;
    private AudioSource audioSource;

    [Header("Efektler")]
    public GameObject floatingTextPrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    void Update()
    {
        if (!isStuck && rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity) * Quaternion.Euler(90, 0, 0);
        }
    }

    // Rüzgar kuvvetini FixedUpdate'te uygula (fizik döngüsüyle senkron)
    void FixedUpdate()
    {
        if (isStuck || rb == null || rb.isKinematic) return;

        if (WindManager.Instance != null)
        {
            Vector3 windForce = WindManager.Instance.GetWindForce();
            rb.AddForce(windForce, ForceMode.Force);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isStuck) return;
if (collision.transform.root.GetComponent<BowController>() != null || collision.gameObject.name.Contains("Bow"))
            return;

        // 2. Ok, havada başka bir oka çarparsa durma!
        if (collision.gameObject.GetComponent<Arrow>() != null)
            return;
        isStuck = true;
        
        // Fiziği durdur (Saplanma hissi)
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // YENİ: Saplandıktan sonra Trail'i kapat ki iz bırakmaya devam etmesin
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.emitting = false;
        
        // Ok, çarptığı objenin çocuğu olsun (Hedef tahtası hareket etse bile üstünde kalır)
        transform.SetParent(collision.transform);
if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        // Eğer hedefe çarptıysa puanı hesapla
        if (collision.gameObject.CompareTag("Target"))
        {
            CalculateScore(collision.contacts[0].point, collision.transform);
        }
    }

    void CalculateScore(Vector3 hitPoint, Transform targetTransform)
    {
        // Vurulan nokta ile hedefin TAM MERKEZİ arasındaki mesafeyi ölç
        float distance = Vector3.Distance(hitPoint, targetTransform.position);
        
        int score = 0;
        
        // Mesafeye göre puan ver (Bu değerleri kendi hedef tahtanın boyutuna göre ayarlayacağız)
        if (distance < 0.3f) score = 10;      // Tam on ikiden
        else if (distance < 0.55f) score = 5;  // Orta halka
        else if (distance < 1.0f) score = 2;  // Dış halka
        else score = 1;                       // Tahtaya değdi ama çok kenarda


        if (floatingTextPrefab != null)
        {
            // Vuruş noktasında yazıyı oluştur
            GameObject floatText = Instantiate(floatingTextPrefab, hitPoint, Quaternion.identity);
            
            // Scriptine ulaşıp puanı gönder
            FloatingTextAnim animScript = floatText.GetComponent<FloatingTextAnim>();
            if (animScript != null)
            {
                animScript.Setup(score);
            }
        }
       // Debug.Log($"Vurulan Puan: {score} (Merkeze Uzaklık: {distance:F2})");
        if (ArcheryGameManager.Instance != null)
    {
        ArcheryGameManager.Instance.AddScore(score);
    }
        // TODO: UI'ı güncelleyeceğiz
    }
}