using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BattleSkillManager : MonoBehaviour
{
    public static BattleSkillManager Instance;

    [Header("Yetenek 1: Şifalı Otlar")]
    public Button healBtn;
    public Image healCooldownImg;
    public float healCooldown = 10f; // 10 saniyede bir
    public int healAmount = 30;
    private bool isHealReady = true;

    [Header("Yetenek 2: Ok Yağmuru")]
    public Button arrowBtn;
    public Image arrowCooldownImg;
    public float arrowCooldown = 8f; // 8 saniyede bir
    public int arrowDamage = 25;
    private bool isArrowReady = true;

    [Header("Efektler")]
    public GameObject healEffectPrefab; // Yeşil parıltı (Opsiyonel)
    public GameObject arrowEffectPrefab; // Ok yağmuru efekti (Opsiyonel)

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Butonlara tıklandığında ne yapacaklarını söyle
        healBtn.onClick.AddListener(UseHealSkill);
        arrowBtn.onClick.AddListener(UseArrowSkill);
        
        // Cooldown görsellerini sıfırla
        healCooldownImg.fillAmount = 0;
        arrowCooldownImg.fillAmount = 0;
    }

    void Update()
    {
        // Cooldown görsellerini güncelle (Geri sayım animasyonu)
        if (!isHealReady)
        {
            healCooldownImg.fillAmount -= 1.0f / healCooldown * Time.deltaTime;
            if (healCooldownImg.fillAmount <= 0) isHealReady = true;
        }

        if (!isArrowReady)
        {
            arrowCooldownImg.fillAmount -= 1.0f / arrowCooldown * Time.deltaTime;
            if (arrowCooldownImg.fillAmount <= 0) isArrowReady = true;
        }

        // Butonların tıklanabilirliğini ayarla
        healBtn.interactable = isHealReady;
        arrowBtn.interactable = isArrowReady;
    }

    // --- YETENEK 1: ŞİFA ---
    void UseHealSkill()
    {
        if (!isHealReady) return;

        Debug.Log("Şifalı Otlar Basıldı!");

        // Sahnedeki TÜM askerleri bul
        var allUnits = FindObjectsOfType<Gladiator>();

        foreach (var unit in allUnits)
        {
            // Sadece "MySoldier" etiketli (Bizimkiler) olanlara etki et
            // (Tag kontrolünü GladiatorAI üzerinden veya direkt gameObject.tag ile yapabilirsin)
            if (unit.gameObject.CompareTag("MySoldier")) 
            {
                HealUnit(unit);
            }
        }

        // Cooldown'ı başlat
        isHealReady = false;
        healCooldownImg.fillAmount = 1; // Perdeyi kapat
    }

    void HealUnit(Gladiator unit)
    {
        if (unit.currentHealth <= 0) return; // Ölüye ilaç fayda etmez

        // Canı artır ama max canı geçmesin
        unit.currentHealth += healAmount;
        if (unit.currentHealth > unit.maxHealth) unit.currentHealth = unit.maxHealth;

        // Can barını güncelle
        if (unit.healthBar != null) unit.healthBar.UpdateBar(unit.currentHealth, unit.maxHealth);

        // Yeşil "+30" yazısı çıkart (Hasar yazısını yeşil renkle kullanabiliriz)
        if (DamageTextManager.Instance != null)
        {
            // Pozisyonu hafif yukarı al
            DamageTextManager.Instance.ShowDamage(unit.transform.position, healAmount, 2); 
            // NOT: ShowDamage scriptinde "Heal" için ayrı bir renk ayarı yaparsan süper olur.
            // Şimdilik "Kritik" (Sarı) olarak çıkacak, ama yeşil olması lazım.
        }
    }

    // --- YETENEK 2: OK YAĞMURU ---
    void UseArrowSkill()
    {
        if (!isArrowReady) return;

        Debug.Log("Oklar Yağıyor!");

        // Sahnedeki TÜM düşmanları bul
        var allUnits = FindObjectsOfType<Gladiator>();

        foreach (var unit in allUnits)
        {
            // Sadece "EnemySoldier" etiketli olanlara vur
            if (unit.gameObject.CompareTag("EnemySoldier"))
            {
                // Düşmanın AI scriptini bulup hasar fonksiyonunu çağır
                var ai = unit.GetComponent<GladiatorAI>();
                if (ai != null)
                {
                    // Hasar ver (Kritik değil)
                    ai.TakeDamage(arrowDamage, false);
                }
            }
        }

        isArrowReady = false;
        arrowCooldownImg.fillAmount = 1;
    }
}