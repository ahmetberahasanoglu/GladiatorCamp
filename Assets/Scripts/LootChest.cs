using UnityEngine;
using TMPro;
using System.Collections;

public class LootChest : MonoBehaviour
{
    // Diğer scriptlerden kolayca ulaşmak için Singleton yapıyoruz
    public static LootChest Instance; 

    [Header("Görsel ve Ses Efektleri")]
    public GameObject chestVisuals;          // YENİ: Sandığın 3D modelini buraya sürükleyeceğiz
    public ParticleSystem coinBurstParticle; 
    public AudioSource coinSound;            
    
    [Header("Etkileşim UI")]
    public GameObject hoverTextObj;
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    private Vector3 originalScale;

    private int pendingGold = 0;
    private int pendingReputation = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        originalScale = transform.localScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);

        // İlk açılışta kontrol et
        CheckForLoot(); 
    }

    // Kampa dönüldüğünde çağrılacak ana fonksiyon
    public void CheckForLoot()
    {
        pendingGold = PlayerPrefs.GetInt("PendingGold", 0);
        pendingReputation = PlayerPrefs.GetInt("PendingReputation", 0);

        bool hasLoot = (pendingGold > 0 || pendingReputation > 0);

        // Objenin kendisini DEĞİL, görselini ve tıklanmasını aç/kapat
        if (chestVisuals != null) chestVisuals.SetActive(hasLoot);
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = hasLoot;
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
        // Çift tıklamayı önle
        GetComponent<Collider>().enabled = false;
        OnMouseExit(); 

        // 1. Ödülleri ver
        if (pendingGold > 0) MoneyManager.Instance.Add(pendingGold);
        if (pendingReputation > 0) ReputationManager.Instance.ChangeReputation(pendingReputation);

        // 2. Coşkulu Efektler!
        if (coinBurstParticle != null) coinBurstParticle.Play();
        if (coinSound != null) coinSound.Play();

        // 3. Bildirim
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.Show($"Seferden Dönüldü! +{pendingGold} Akçe, +{pendingReputation} İtibar", NotificationType.Success);
        }

        // 4. Hafızayı sıfırla
        PlayerPrefs.SetInt("PendingGold", 0);
        PlayerPrefs.SetInt("PendingReputation", 0);
        PlayerPrefs.Save();

        // 5. Kaybolma rutini
        StartCoroutine(HideAfterEffects());
    }

    IEnumerator HideAfterEffects()
    {
        // Tıklanır tıklanmaz sandığın modelini gizle, partiküller patlamaya devam etsin
        if (chestVisuals != null) chestVisuals.SetActive(false);

        yield return new WaitForSeconds(2f);
        
        // ESKİDEN BURADA gameObject.SetActive(false) VARDI, ARTIK SİLDİK! 
        // Obje uyanık kalmaya devam ediyor.
    }
}