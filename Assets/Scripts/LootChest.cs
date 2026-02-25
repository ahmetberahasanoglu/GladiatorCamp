using UnityEngine;
using TMPro;
using System.Collections;

public class LootChest : MonoBehaviour
{
    [Header("Görsel ve Ses Efektleri")]
    public ParticleSystem coinBurstParticle; // Havaya saçılan altınlar
    public AudioSource coinSound;            // Şıngırdayan altın sesi
    
    [Header("Etkileşim UI")]
    public GameObject hoverTextObj;
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    private Vector3 originalScale;

    private int pendingGold = 0;
    private int pendingReputation = 0;

    void Start()
    {
        originalScale = transform.localScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);

        // 1. Hafızada bekleyen ganimet var mı bakıyoruz
        pendingGold = PlayerPrefs.GetInt("PendingGold", 0);
        pendingReputation = PlayerPrefs.GetInt("PendingReputation", 0);

        // 2. Eğer hiç ganimet yoksa, bu sandığı tamamen gizle (Kampa öylesine döndüysek sandık çıkmaz)
        if (pendingGold <= 0 && pendingReputation <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    void OnMouseEnter()
    {
        transform.localScale = hoverScale;
        if (hoverTextObj != null) 
        {
            hoverTextObj.SetActive(true);
            hoverTextObj.GetComponentInChildren<TextMeshPro>().text = "Ganimeti Aç\n(Tıkla)";
        }
    }

    void OnMouseExit()
    {
        transform.localScale = originalScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseDown()
    {
        // Zaten tıklanmışsa tekrar tıklanmasın diye collider'ı kapat
        GetComponent<Collider>().enabled = false;
        OnMouseExit(); // Büyümeyi ve yazıyı sıfırla

        // 1. Ganimetleri gerçek yöneticilere (cebimize) ekle
        if (pendingGold > 0)
        {
            MoneyManager.Instance.Add(pendingGold);
            // NotificationManager'dan bildirim gönderebiliriz
        }

        if (pendingReputation > 0)
        {
            ReputationManager.Instance.ChangeReputation(pendingReputation);
        }

        // 2. Coşkulu Efektleri Oynat!
        if (coinBurstParticle != null) coinBurstParticle.Play();
        if (coinSound != null) coinSound.Play();

        // 3. Ekrana toplam kazancı büyük bir yazıyla yazdır
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.Show($"Seferden Dönüldü! +{pendingGold} Akçe, +{pendingReputation} İtibar", NotificationType.Success);
        }

        // 4. Hafızayı temizle (Aynı ganimeti tekrar almamak için)
        PlayerPrefs.SetInt("PendingGold", 0);
        PlayerPrefs.SetInt("PendingReputation", 0);
        PlayerPrefs.Save();

        // 5. Sandığı yavaşça gizle
        StartCoroutine(HideAfterEffects());
    }

    IEnumerator HideAfterEffects()
    {
        // Tıklanır tıklanmaz sandığın 3D modelini (Mesh) gizle, sadece partiküller uçuşmaya devam etsin
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;

        // Partikülün ve sesin bitmesi için 2 saniye bekle
        yield return new WaitForSeconds(2f);
        
        // Obje işlevini tamamen bitirdi, kapat.
        gameObject.SetActive(false);
    }
}