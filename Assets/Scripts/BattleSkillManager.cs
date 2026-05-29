using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SkillSlotUI
{
    public Button skillBtn;
    public Image iconImg;
    public Image cooldownImg;
    
    [HideInInspector] public CommanderSkillData assignedSkill;
    [HideInInspector] public float currentCooldown = 0f;
    [HideInInspector] public bool isReady = true;
    [HideInInspector] public bool isUsedUp = false;
    [HideInInspector] public int slotIndex; // Hangi slot olduğunu hatırlamak için
}

public class BattleSkillManager : MonoBehaviour
{
    public static BattleSkillManager Instance;

    [Header("UI Yetenek Slotları")]
    public List<SkillSlotUI> skillSlots;

    [Header("Taktiksel Kamera (Kuşbakışı)")]
    public Camera battleCamera;
    public Vector3 topDownOffset = new Vector3(0, 12f, -4f); // Savaş alanına yukarıdan bakış
    public Vector3 topDownRotation = new Vector3(65f, 0, 0); // Yere doğru eğim
    public float cameraTransitionSpeed = 0.5f; // Kameranın kayma hızı (Gerçek zamanlı)

    // --- Arka Plan Değişkenleri ---
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    
    [HideInInspector] public bool isTargeting = false; // Zaman durdu mu?
    private SkillSlotUI activeTargetingSlot; // Şu an hedef seçimi bekleyen yetenek

    void Awake()
    {
        Instance = this;
        if (battleCamera == null) battleCamera = Camera.main;
    }

    void Update()
    {
        // 1. ZAMAN DURDUYSA VE HEDEF SEÇİYORSAK: Cooldown'ları doldurma, sadece tıklama bekle!
        if (isTargeting)
        {
            HandleTargetingInput();
            return; // Aşağıdaki cooldown kodlarını atla
        }

        // 2. NORMAL SAVAŞ AKIŞI: Cooldown'ları hesapla
        foreach (var slot in skillSlots)
        {
            if (slot.assignedSkill == null || slot.isUsedUp) continue;

            if (!slot.isReady)
            {
                slot.currentCooldown -= Time.deltaTime;
                slot.cooldownImg.fillAmount = slot.currentCooldown / slot.assignedSkill.cooldownTime;

                if (slot.currentCooldown <= 0)
                {
                    slot.isReady = true;
                    slot.skillBtn.interactable = true;
                }
            }
        }
    }

    public void LoadSelectedSkills(List<CommanderSkillData> selectedSkills)
    {
        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (i < selectedSkills.Count && selectedSkills[i] != null)
            {
                skillSlots[i].slotIndex = i;
                skillSlots[i].assignedSkill = selectedSkills[i];
                skillSlots[i].iconImg.sprite = selectedSkills[i].skillIcon;
                skillSlots[i].iconImg.enabled = true;
                skillSlots[i].cooldownImg.fillAmount = 0;
                skillSlots[i].isReady = true;
                skillSlots[i].isUsedUp = false;
                skillSlots[i].skillBtn.interactable = true;
                skillSlots[i].skillBtn.gameObject.SetActive(true);

                int slotIndex = i; 
                skillSlots[i].skillBtn.onClick.RemoveAllListeners();
                skillSlots[i].skillBtn.onClick.AddListener(() => ActivateSkill(slotIndex));
            }
            else
            {
                skillSlots[i].skillBtn.gameObject.SetActive(false);
            }
        }
    }

    void ActivateSkill(int slotIndex)
    {
        SkillSlotUI slot = skillSlots[slotIndex];
        CommanderSkillData skillData = slot.assignedSkill;

        if (!slot.isReady || slot.isUsedUp || isTargeting) return;

        // EĞER HEDEF GEREKTİREN BİR YETENEKSE ZAMANI DURDUR!
        if (skillData.skillType == CommanderSkillType.HealSingle || skillData.skillType == CommanderSkillType.DamageSingle)
        {
            StartTargetingMode(slot);
        }
        else
        {
            // Hedef gerektirmeyen anında yetenekler (Mevcut olanlar)
            switch (skillData.skillType)
            {
                case CommanderSkillType.HealAll: ExecuteHealAll(skillData); break;
                case CommanderSkillType.DamageAllEnemies: ExecuteArrowRain(skillData); break;
                case CommanderSkillType.BuffAll: ExecuteBuffAll(skillData); break;
            }
            ConsumeSkill(slot); // Kullandı say
        }
    }

    // --- TAKTİKSEL DURAKLAMA (SİHİRLİ KISIM) ---
    void StartTargetingMode(SkillSlotUI slot)
    {
        activeTargetingSlot = slot;
        isTargeting = true;
        
        // Zamanı Durdur
        Time.timeScale = 0f; 
        
        // Orijinal kamera açısını kaydet ve yukarı kaydır
        originalCamPos = battleCamera.transform.position;
        originalCamRot = battleCamera.transform.rotation;
        
        Vector3 targetPos = originalCamPos + topDownOffset;
        Quaternion targetRot = Quaternion.Euler(topDownRotation);
        StartCoroutine(MoveCamera(targetPos, targetRot, cameraTransitionSpeed));

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show("Taktiksel Mod: Hedef Seçmek İçin Tıkla (İptal için Sağ Tık)", NotificationType.Info);
    }

    void HandleTargetingInput()
    {
        // SOL TIK: HEDEFİ SEÇ VE BÜYÜYÜ UYGULA
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = battleCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Gladiator target = hit.collider.GetComponent<Gladiator>();
                if (target != null)
                {
                    CommanderSkillType type = activeTargetingSlot.assignedSkill.skillType;

                    // Kendi askerine şifa
                    if (type == CommanderSkillType.HealSingle && target.CompareTag("MySoldier"))
                    {
                        ExecuteSingleHeal(target, activeTargetingSlot.assignedSkill);
                        EndTargetingMode(true);
                    }
                    // Düşmana yıldırım/suikast
                    else if (type == CommanderSkillType.DamageSingle && target.CompareTag("EnemySoldier"))
                    {
                        ExecuteSingleDamage(target, activeTargetingSlot.assignedSkill);
                        EndTargetingMode(true);
                    }
                    else
                    {
                        if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Geçersiz hedef!", NotificationType.Error);
                    }
                }
            }
        }

        // SAĞ TIK: İPTAL ET
        if (Input.GetMouseButtonDown(1))
        {
            EndTargetingMode(false); // Büyüyü kullanmadan çık
        }
    }

    void EndTargetingMode(bool skillUsed)
    {
        isTargeting = false;
        Time.timeScale = 1f; // Zamanı geri başlat

        // Kamerayı eski yerine kaydır
        StartCoroutine(MoveCamera(originalCamPos, originalCamRot, cameraTransitionSpeed));

        if (skillUsed && activeTargetingSlot != null)
        {
            ConsumeSkill(activeTargetingSlot);
        }
        activeTargetingSlot = null;
    }

    // Kamerayı Time.timeScale=0 iken bile kaydırmak için WaitForSecondsRealtime kullanılır
    IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 startPos = battleCamera.transform.position;
        Quaternion startRot = battleCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Oyun dursa bile zaman geçer
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            battleCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            battleCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        battleCamera.transform.position = targetPos;
        battleCamera.transform.rotation = targetRot;
    }

    void ConsumeSkill(SkillSlotUI slot)
    {
        if (slot.assignedSkill.isSingleUse)
        {
            slot.isUsedUp = true;
            slot.skillBtn.interactable = false;
            slot.iconImg.color = new Color(0.3f, 0.3f, 0.3f); 
            
            // ── YENİ: KULLANILAN NÜSHAYI ÇANTADAN SİL ──
            if (InventoryStorage.Instance != null)
            {
                // Çantada bu yeteneği barındıran ilk ItemData'yı bul ve sil
                for (int i = 0; i < InventoryStorage.Instance.storedItems.Count; i++)
                {
                    var item = InventoryStorage.Instance.storedItems[i];
                    if (item.type == ItemType.Nusha && item.spellData == slot.assignedSkill)
                    {
                        InventoryStorage.Instance.storedItems.RemoveAt(i);
                        break; // Sadece 1 tane eksilt
                    }
                }
            }
            // ──────────────────────────────────────────
        }
        else
        {
            slot.isReady = false;
            slot.currentCooldown = slot.assignedSkill.cooldownTime;
            slot.cooldownImg.fillAmount = 1;
            slot.skillBtn.interactable = false;
        }
    }

    // --- YETENEK FONKSİYONLARI ---

    void ExecuteSingleHeal(Gladiator target, CommanderSkillData data)
    {
        if (target.currentHealth <= 0) return;
        target.currentHealth += data.powerAmount;
        if (target.currentHealth > target.maxHealth) target.currentHealth = target.maxHealth;
        if (target.healthBar != null) target.healthBar.UpdateBar(target.currentHealth, target.maxHealth);

        if (DamageTextManager.Instance != null) DamageTextManager.Instance.ShowDamage(target.transform.position, data.powerAmount, 2); 
        PlayVFX(data.effectPrefab, target.transform);
    }

    void ExecuteSingleDamage(Gladiator target, CommanderSkillData data)
    {
        var ai = target.GetComponent<GladiatorAI>();
        if (ai != null && !ai.isDead)
        {
            ai.TakeDamage(data.powerAmount, false);
            PlayVFX(data.effectPrefab, target.transform);
        }
    }

    void ExecuteBuffAll(CommanderSkillData data)
    {
        var allUnits = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit.gameObject.CompareTag("MySoldier") && unit.currentHealth > 0) 
            {
                // Geçici olarak gücünü artırır (Savaş bitince normale dönmesi için buff sistemi gerekir)
                unit.data.strength += data.powerAmount;
                PlayVFX(data.effectPrefab, unit.transform);
            }
        }
    }

    // Mevcut olan HealAll ve ArrowRain fonksiyonların buraya gelecek...
    void ExecuteHealAll(CommanderSkillData data) { /* ... Eski kod ... */ }
    void ExecuteArrowRain(CommanderSkillData data) { /* ... Eski kod ... */ }

    void PlayVFX(GameObject prefab, Transform targetTransform)
    {
        if (prefab != null)
        {
            GameObject vfx = Instantiate(prefab, targetTransform.position + Vector3.up * 0.1f, Quaternion.identity);
            vfx.transform.SetParent(targetTransform);
            Destroy(vfx, 2.0f);
        }
    }
}