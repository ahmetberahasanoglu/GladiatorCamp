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
    public float healCooldown = 10f;
    public int healAmount = 30;
    private bool isHealReady = true;

    [Header("Yetenek 2: Ok Yağmuru")]
    public Button arrowBtn;
    public Image arrowCooldownImg;
    public float arrowCooldown = 8f;
    public int arrowDamage = 25;
    private bool isArrowReady = true;

    [Header("Efektler")]
    public GameObject healEffectPrefab; // Inspector'dan atamayı unutma!
    public GameObject arrowEffectPrefab; // Inspector'dan atamayı unutma!

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (healBtn != null) healBtn.onClick.AddListener(UseHealSkill);
        if (arrowBtn != null) arrowBtn.onClick.AddListener(UseArrowSkill);
        
        if (healCooldownImg != null) healCooldownImg.fillAmount = 0;
        if (arrowCooldownImg != null) arrowCooldownImg.fillAmount = 0;
    }

    void Update()
    {
        if (!isHealReady && healCooldownImg != null)
        {
            healCooldownImg.fillAmount -= 1.0f / healCooldown * Time.deltaTime;
            if (healCooldownImg.fillAmount <= 0) isHealReady = true;
        }

        if (!isArrowReady && arrowCooldownImg != null)
        {
            arrowCooldownImg.fillAmount -= 1.0f / arrowCooldown * Time.deltaTime;
            if (arrowCooldownImg.fillAmount <= 0) isArrowReady = true;
        }

        if (healBtn != null) healBtn.interactable = isHealReady;
        if (arrowBtn != null) arrowBtn.interactable = isArrowReady;
    }

    // --- YETENEK 1: ŞİFA ---
    void UseHealSkill()
    {
        if (!isHealReady) return;

        Debug.Log("Şifalı Otlar Basıldı!");

        // Prefab kontrolü
        if (healEffectPrefab == null) Debug.LogWarning("Heal Effect Prefab atanmamış!");

        var allUnits = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);

        foreach (var unit in allUnits)
        {
            if (unit.gameObject.CompareTag("MySoldier")) 
            {
                HealUnit(unit);
            }
        }

        isHealReady = false;
        if (healCooldownImg != null) healCooldownImg.fillAmount = 1;
    }

    void HealUnit(Gladiator unit)
    {
        if (unit.currentHealth <= 0) return;

        unit.currentHealth += healAmount;
        if (unit.currentHealth > unit.maxHealth) unit.currentHealth = unit.maxHealth;

        if (unit.healthBar != null) unit.healthBar.UpdateBar(unit.currentHealth, unit.maxHealth);

        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.ShowDamage(unit.transform.position, healAmount, 2); 
        }

        // --- EKLENEN KISIM: EFEKT YARATMA (INSTANTIATE) ---
        if (healEffectPrefab != null)
        {
            // Askerin pozisyonunda efekti yarat
            GameObject vfx = Instantiate(healEffectPrefab, unit.transform.position, Quaternion.identity);
            
            // Efekti askerin içine koy (asker hareket ederse efekt de etsin) - Opsiyonel
            vfx.transform.SetParent(unit.transform);

            // 2 saniye sonra efekti sil ki oyun kasmasın
            Destroy(vfx, 2.0f);
        }
        // --------------------------------------------------
    }

    // --- YETENEK 2: OK YAĞMURU ---
    void UseArrowSkill()
    {
        if (!isArrowReady) return;

        Debug.Log("Oklar Yağıyor!");

        // Prefab kontrolü
        if (arrowEffectPrefab == null) Debug.LogWarning("Arrow Effect Prefab atanmamış!");

        var allUnits = FindObjectsOfType<Gladiator>();

        foreach (var unit in allUnits)
        {
            if (unit.gameObject.CompareTag("EnemySoldier"))
            {
                var ai = unit.GetComponent<GladiatorAI>();
                if (ai != null)
                {
                    ai.TakeDamage(arrowDamage, false);

                    // --- EKLENEN KISIM: OK EFEKTİ YARATMA ---
                    if (arrowEffectPrefab != null)
                    {
                        // Düşmanın kafasının 5 birim üstünde oluşsun
                        Vector3 spawnPos = unit.transform.position + Vector3.up * 5.0f;
                        
                        // Efekti yere bakacak şekilde döndür (90 derece)
                        Quaternion rotation = Quaternion.Euler(90, 0, 0);

                        GameObject vfx = Instantiate(arrowEffectPrefab, spawnPos, rotation);
                        
                        // 2 saniye sonra sil
                        Destroy(vfx, 2.0f);
                    }
                    // ----------------------------------------
                }
            }
        }

        isArrowReady = false;
        if (arrowCooldownImg != null) arrowCooldownImg.fillAmount = 1;
    }
}