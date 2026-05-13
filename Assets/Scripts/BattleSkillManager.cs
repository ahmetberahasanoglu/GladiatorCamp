using UnityEngine;
using UnityEngine.UI;
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
    [HideInInspector] public bool isUsedUp = false; // Tek kullanımlıksa takip etmek için
}

public class BattleSkillManager : MonoBehaviour
{
    public static BattleSkillManager Instance;

    [Header("UI Yetenek Slotları (Ekranda gözüken 3 buton)")]
    public List<SkillSlotUI> skillSlots;
    

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Her slotun bekleme süresini (Cooldown) ayrı ayrı hesapla
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

    // Taktik ekranından çıkıp savaşa girildiğinde bu fonksiyon çağrılır!
    public void LoadSelectedSkills(List<CommanderSkillData> selectedSkills)
    {
        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (i < selectedSkills.Count && selectedSkills[i] != null)
            {
                // Slotu doldur ve görünür yap
                skillSlots[i].assignedSkill = selectedSkills[i];
                skillSlots[i].iconImg.sprite = selectedSkills[i].skillIcon;
                skillSlots[i].iconImg.enabled = true;
                skillSlots[i].cooldownImg.fillAmount = 0;
                skillSlots[i].isReady = true;
                skillSlots[i].isUsedUp = false;
                skillSlots[i].skillBtn.interactable = true;
                skillSlots[i].skillBtn.gameObject.SetActive(true);

                // Butona tıklandığında hangi yeteneğin çalışacağını ayarla
                int slotIndex = i; // Listener için lokal kopya
                skillSlots[i].skillBtn.onClick.RemoveAllListeners();
                skillSlots[i].skillBtn.onClick.AddListener(() => ActivateSkill(slotIndex));
            }
            else
            {
                // Eğer oyuncu sadece 1 yetenek seçtiyse, diğer slotları gizle
                skillSlots[i].skillBtn.gameObject.SetActive(false);
            }
        }
    }

    // Butona basıldığında yeteneğin "Türüne" göre işlem yap
    void ActivateSkill(int slotIndex)
    {
        SkillSlotUI slot = skillSlots[slotIndex];
        CommanderSkillData skillData = slot.assignedSkill;

        if (!slot.isReady || slot.isUsedUp) return;

        Debug.Log(skillData.skillName + " Kullanıldı!");

        // YETENEK TÜRLERİ (Senin yazdığın mantıkların aynısı)
        switch (skillData.skillType)
        {
            case CommanderSkillType.HealAll:
                ExecuteHealAll(skillData);
                break;
            case CommanderSkillType.DamageAllEnemies:
                ExecuteArrowRain(skillData);
                break;
        }

        // COOLDOWN VE TEK KULLANIMLIK KONTROLÜ
        if (skillData.isSingleUse)
        {
            slot.isUsedUp = true;
            slot.skillBtn.interactable = false;
            slot.iconImg.color = new Color(0.3f, 0.3f, 0.3f); // Karart
        }
        else
        {
            slot.isReady = false;
            slot.currentCooldown = skillData.cooldownTime;
            slot.cooldownImg.fillAmount = 1;
            slot.skillBtn.interactable = false;
        }
    }

    // --- SENİN YAZDIĞIN FONKSİYONLARIN DİNAMİK HALİ ---

    void ExecuteHealAll(CommanderSkillData data)
    {
        var allUnits = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit.gameObject.CompareTag("MySoldier") && unit.currentHealth > 0) 
            {
                unit.currentHealth += data.powerAmount;
                if (unit.currentHealth > unit.maxHealth) unit.currentHealth = unit.maxHealth;
                if (unit.healthBar != null) unit.healthBar.UpdateBar(unit.currentHealth, unit.maxHealth);

                if (DamageTextManager.Instance != null)
                    DamageTextManager.Instance.ShowDamage(unit.transform.position, data.powerAmount, 2); 

                if (data.effectPrefab != null)
                {
                    GameObject vfx = Instantiate(data.effectPrefab, unit.transform.position, Quaternion.identity);
                    vfx.transform.SetParent(unit.transform);
                    Destroy(vfx, 2.0f);
                }
            }
        }
    }

    void ExecuteArrowRain(CommanderSkillData data)
    {
        var allUnits = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit.gameObject.CompareTag("EnemySoldier"))
            {
                var ai = unit.GetComponent<GladiatorAI>();
                if (ai != null && !ai.isDead)
                {
                    ai.TakeDamage(data.powerAmount, false);

                    if (data.effectPrefab != null)
                    {
                        Vector3 spawnPos = unit.transform.position + Vector3.up * 0.1f;
                        GameObject vfx = Instantiate(data.effectPrefab, spawnPos, Quaternion.Euler(90, 0, 0));
                        Destroy(vfx, 2.0f);
                    }
                }
            }
        }
    }
}