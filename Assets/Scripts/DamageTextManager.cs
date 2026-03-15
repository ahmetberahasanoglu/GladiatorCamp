using UnityEngine;
using System.Collections.Generic;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance;

    [Header("Ayarlar")]
    public GameObject popupPrefab;
    public int poolSize = 30; // Havuzda kaç yazı hazır beklesin?

    private Queue<DamagePopup> poolQueue = new Queue<DamagePopup>();
    private Transform poolContainer; // Hiyerarşide dağınıklık olmasın diye bir kutu

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    void InitializePool()
    {
        // Objenin altında bir klasör gibi boş obje oluştur
        GameObject container = new GameObject("DamageTextPool");
        poolContainer = container.transform;
        poolContainer.SetParent(transform);

        // Havuzu doldur
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewPopup();
        }
    }

    DamagePopup CreateNewPopup()
    {
        GameObject obj = Instantiate(popupPrefab, poolContainer);
        DamagePopup popup = obj.GetComponent<DamagePopup>();
        obj.SetActive(false); // Başta kapalı olsun
        poolQueue.Enqueue(popup); // Kuyruğa ekle
        return popup;
    }

    public void ShowDamage(Vector3 position, float amount, int type)
    {
        // Havuzda eleman kalmadıysa yeni yarat
        if (poolQueue.Count == 0)
        {
            CreateNewPopup();
        }

        // Kuyruktan bir tane al
        DamagePopup popup = poolQueue.Dequeue();
        
        // Pozisyonunu ayarla (Kafanın biraz üstü)
        popup.transform.position = position + Vector3.up * 1.5f; 
        popup.gameObject.SetActive(true);
        popup.Setup(Mathf.RoundToInt(amount), type);

        // İşi bitince tekrar kuyruğa girmesi için (Basit yöntem: Enqueue'yu burada değil, obje kapanırken yapabiliriz ama şimdilik bu yeterli)
        // Daha sağlam bir pool yapısı için objenin OnDisable'ında kuyruğa geri eklemek gerekir ama bu casual oyun için Queue döngüsü yeterli olacaktır.
        poolQueue.Enqueue(popup);
    }
    // YENİ: Sadece özel yazılar ("ALLAH KORUDU", "BLOK", "ISKA") basmak için
    public void ShowCustomText(Vector3 position, string text, Color color)
    {
        if (poolQueue.Count == 0)
        {
            CreateNewPopup();
        }

        DamagePopup popup = poolQueue.Dequeue();
        
        popup.transform.position = position + Vector3.up * 1.5f; 
        popup.gameObject.SetActive(true);
        
        // Popup'a özel yazıyı ve rengi gönderiyoruz
        popup.SetupCustom(text, color);

        poolQueue.Enqueue(popup);
    }
}