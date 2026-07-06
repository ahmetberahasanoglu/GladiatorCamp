using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class SkillSlotUI
{
    public Button skillBtn;
    public Image  iconImg;
    public Image  cooldownImg;

    [HideInInspector] public CommanderSkillData assignedSkill;
    [HideInInspector] public float currentCooldown = 0f;
    [HideInInspector] public bool  isReady         = true;
    [HideInInspector] public bool  isUsedUp        = false;
    [HideInInspector] public int   slotIndex;
}

public class BattleSkillManager : MonoBehaviour
{
    public static BattleSkillManager Instance;

    [Header("UI Yetenek Slotları")]
    public List<SkillSlotUI> skillSlots;

    [Header("Taktiksel Kamera (Kuşbakışı)")]
    public Camera battleCamera;
    public Vector3 topDownOffset = new Vector3(0, 14f, -5f);       
    public Vector3 topDownRotation = new Vector3(70f, 0, 0);     
    public float cameraTransitionSpeed = 0.4f;                  

    [Header("Hedefleme Overlay UI (Minimalist)")]
    [Tooltip("Tüm ekranı kaplayan panel. Zaman durduğunda dünyayı hafifçe karartır (Sinematik etki).")]
    public Image targetingOverlay;        
    [Tooltip("Ekranın üstünde belirecek zarif hedef ipucu metni")]
    public TextMeshProUGUI targetingHintText;
    
    [Header("Hafif Karartma Rengi (Vignette Etkisi)")]
    public Color tacticalPauseTint = new Color(0.02f, 0.02f, 0.02f, 0.18f); // Renksiz, sadece çok hafif bir gölge

    [Header("Hedef Halkaları")]
    [Tooltip("Geçerli hedeflerin altına yerleşecek halka prefabı")]
    public GameObject targetRingPrefab;        
    public Color validTargetColor   = new Color(0.2f, 1f, 0.3f, 0.6f);   // Yeşil — Şeffaflığı artırıldı
    public Color invalidTargetColor = new Color(1f, 0.2f, 0.1f, 0.15f);  // Kırmızı — Çok silik

    // ── İç Durum Kontrolleri ─────────────────────────────────────────────
    [HideInInspector] public bool isTargeting = false;
    private SkillSlotUI      _activeSlot;
    private List<GameObject> _spawnedRings = new List<GameObject>();
    private Vector3          _originalCamPos;
    private Quaternion       _originalCamRot;
    private Coroutine        _cameraCoroutine;

    // ─────────────────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
        if (battleCamera == null) battleCamera = Camera.main;
    }

    void Start()
    {
        if (targetingOverlay  != null) targetingOverlay.gameObject.SetActive(false);
        if (targetingHintText != null) targetingHintText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isTargeting)
        {
            HandleTargetingInput();
            return;
        }

        foreach (var slot in skillSlots)
        {
            if (slot.assignedSkill == null || slot.isUsedUp || slot.isReady) continue;

            slot.currentCooldown -= Time.deltaTime;
            if (slot.cooldownImg != null)
                slot.cooldownImg.fillAmount = slot.currentCooldown / slot.assignedSkill.cooldownTime;

            if (slot.currentCooldown <= 0f)
            {
                slot.isReady            = true;
                slot.skillBtn.interactable = true;
                if (slot.cooldownImg != null) slot.cooldownImg.fillAmount = 0f;
            }
        }
    }

    public void LoadSelectedSkills(List<CommanderSkillData> selected)
    {
        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (i < selected.Count && selected[i] != null)
            {
                var slot = skillSlots[i];
                slot.slotIndex     = i;
                slot.assignedSkill = selected[i];
                slot.isReady       = true;
                slot.isUsedUp      = false;
                slot.currentCooldown = 0f;

                if (slot.iconImg != null) { slot.iconImg.sprite = selected[i].skillIcon; slot.iconImg.enabled = true; slot.iconImg.color = Color.white; }
                if (slot.cooldownImg != null) slot.cooldownImg.fillAmount = 0f;

                slot.skillBtn.interactable = true;
                slot.skillBtn.gameObject.SetActive(true);

                int captured = i;
                slot.skillBtn.onClick.RemoveAllListeners();
                slot.skillBtn.onClick.AddListener(() => ActivateSkill(captured));
            }
            else
            {
                skillSlots[i].skillBtn.gameObject.SetActive(false);
            }
        }
    }

    void ActivateSkill(int idx)
    {
        var slot = skillSlots[idx];
        if (!slot.isReady || slot.isUsedUp || isTargeting) return;

        bool needsTarget = slot.assignedSkill.skillType == CommanderSkillType.HealSingle
                        || slot.assignedSkill.skillType == CommanderSkillType.DamageSingle;

        if (needsTarget)
            StartTargetingMode(slot);
        else
        {
            ExecuteInstantSkill(slot);
            ConsumeSkill(slot);
        }
    }

    void ExecuteInstantSkill(SkillSlotUI slot)
    {
        switch (slot.assignedSkill.skillType)
        {
            case CommanderSkillType.HealAll:           ExecuteHealAll(slot.assignedSkill);   break;
            case CommanderSkillType.DamageAllEnemies:  ExecuteArrowRain(slot.assignedSkill); break;
            case CommanderSkillType.BuffAll:           ExecuteBuffAll(slot.assignedSkill);   break;
            case CommanderSkillType.StunEnemies:       ExecuteStunAll(slot.assignedSkill);   break;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  TAKTİKSEL MÜHÜR MODU
    // ════════════════════════════════════════════════════════════════════
    void StartTargetingMode(SkillSlotUI slot)
    {
        _activeSlot  = slot;
        isTargeting  = true;
        Time.timeScale = 0f; 

        bool wantsFriendly = slot.assignedSkill.skillType == CommanderSkillType.HealSingle;
        string targetTag   = wantsFriendly ? "MySoldier" : "EnemySoldier";

        // ── 1. Kamera Süzülüşü ───────────────────────────────────────────
        _originalCamPos = battleCamera.transform.position;
        _originalCamRot = battleCamera.transform.rotation;
        
        Vector3 targetCamPos = _originalCamPos + topDownOffset;
        Quaternion targetCamRot = Quaternion.Euler(topDownRotation);
        
        if (_cameraCoroutine != null) StopCoroutine(_cameraCoroutine);
        _cameraCoroutine = StartCoroutine(MoveCamera(targetCamPos, targetCamRot, cameraTransitionSpeed));

        // ── 2. Sinematik Karartma (Renk Patlaması Kaldırıldı) ───────────────
        if (targetingOverlay != null)
        {
            targetingOverlay.gameObject.SetActive(true);
            targetingOverlay.color = tacticalPauseTint; // Sadece dünyayı hafifçe gölgeler, oyunun paletini bozmaz
        }

        // ── 3. Zarif Yazı Renklendirmesi (Gözü Yormayan AAA Tarzı) ─────────
        if (targetingHintText != null)
        {
            targetingHintText.gameObject.SetActive(true);
            targetingHintText.text = wantsFriendly
                ? "Iyilestirmek istedigin <color=#42f560>askerini</color> sec  |  [Sag Tik] Iptal"
                : "Yildirim dusurmek istedigin <color=#f54242>dusmani</color> sec  |  [Sag Tik] Iptal";
        }

        // ── 4. Ayak Altı Halkaları ───────────────────────────────────────
        SpawnTargetRings(targetTag);
    }

    void SpawnTargetRings(string validTag)
    {
        ClearRings();
        if (targetRingPrefab == null) return;

        var allGlads = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        foreach (var g in allGlads)
        {
            var ai = g.GetComponent<GladiatorAI>();
            if (ai == null || ai.isDead || !ai.isInBattle) continue;

            bool isValid = g.CompareTag(validTag);

            GameObject ring = Instantiate(targetRingPrefab, g.transform.position, Quaternion.identity);
            ring.transform.SetParent(g.transform);
            ring.transform.localPosition = new Vector3(0f, 0.05f, 0f); 

            var rend = ring.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = new Material(rend.material); 
                rend.material.color = isValid ? validTargetColor : invalidTargetColor;
            }

            if (!isValid)
            {
                ring.transform.localScale *= 0.65f; // Geçersizleri küçült ki karmaşa azalsın
            }

            _spawnedRings.Add(ring);
        }
    }

    void ClearRings()
    {
        foreach (var r in _spawnedRings)
            if (r != null) Destroy(r);
        _spawnedRings.Clear();
    }

    void HandleTargetingInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = battleCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                var target = hit.collider.GetComponentInParent<Gladiator>();
                if (target != null && target.currentHealth > 0)
                {
                    var type = _activeSlot.assignedSkill.skillType;

                    if (type == CommanderSkillType.HealSingle && target.CompareTag("MySoldier"))
                    {
                        ExecuteSingleHeal(target, _activeSlot.assignedSkill);
                        EndTargetingMode(true);
                    }
                    else if (type == CommanderSkillType.DamageSingle && target.CompareTag("EnemySoldier"))
                    {
                        var targetAI = target.GetComponent<GladiatorAI>();
                        if (targetAI != null && !targetAI.isDead)
                        {
                            ExecuteSingleDamage(target, _activeSlot.assignedSkill);
                            EndTargetingMode(true);
                        }
                    }
                    else
                    {
                        StartCoroutine(FlashHintText());
                    }
                }
            }
        }

        // Sağ tık veya ESC basılırsa büyü harcanmadan iptal edilir
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            EndTargetingMode(false);
        }
    }

    void EndTargetingMode(bool skillUsed)
    {
        isTargeting = false;
        Time.timeScale = 1f; 

        if (targetingOverlay  != null) targetingOverlay.gameObject.SetActive(false);
        if (targetingHintText != null) targetingHintText.gameObject.SetActive(false);

        ClearRings();

        if (_cameraCoroutine != null) StopCoroutine(_cameraCoroutine);
        _cameraCoroutine = StartCoroutine(MoveCamera(_originalCamPos, _originalCamRot, cameraTransitionSpeed));

        if (skillUsed && _activeSlot != null)
            ConsumeSkill(_activeSlot);

        _activeSlot = null;
    }

    IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 startPos = battleCamera.transform.position;
        Quaternion startRot = battleCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration); 
            battleCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            battleCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        battleCamera.transform.position = targetPos;
        battleCamera.transform.rotation = targetRot;
    }

    IEnumerator FlashHintText()
    {
        if (targetingHintText == null) yield break;
        Color origColor = targetingHintText.color;
        targetingHintText.color = Color.white;
        yield return new WaitForSecondsRealtime(0.12f);
        targetingHintText.color = origColor;
    }

    void ConsumeSkill(SkillSlotUI slot)
    {
        if (slot.assignedSkill.isSingleUse)
        {
            slot.isUsedUp               = true;
            slot.skillBtn.interactable = false;
            if (slot.iconImg != null) slot.iconImg.color = new Color(0.25f, 0.22f, 0.25f); 

            if (CommanderStorage.Instance != null)
            {
                for (int i = 0; i < CommanderStorage.Instance.ownedNushas.Count; i++)
                {
                    var item = CommanderStorage.Instance.ownedNushas[i];
                    if (item.type == ItemType.Nusha && item.spellData == slot.assignedSkill)
                    {
                        CommanderStorage.Instance.ownedNushas.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        else
        {
            slot.isReady               = false;
            slot.currentCooldown       = slot.assignedSkill.cooldownTime;
            slot.skillBtn.interactable = false;
            if (slot.cooldownImg != null) slot.cooldownImg.fillAmount = 1f;
        }
    }

    // ── YETENEK FONKSİYONLARI ───────────────────────────────────────────────
    void ExecuteSingleHeal(Gladiator target, CommanderSkillData data)
    {
        if (target.currentHealth <= 0) return;
        target.currentHealth = Mathf.Min(target.currentHealth + data.powerAmount, target.maxHealth);
        if (target.healthBar != null) target.healthBar.UpdateBar(target.currentHealth, target.maxHealth);
        
        DamageTextManager.Instance?.ShowDamage(target.transform.position, data.powerAmount, 2);
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

    void ExecuteHealAll(CommanderSkillData data)
    {
        foreach (var g in FindObjectsByType<Gladiator>(FindObjectsSortMode.None))
        {
            if (!g.CompareTag("MySoldier") || g.currentHealth <= 0) continue;
            g.currentHealth = Mathf.Min(g.currentHealth + data.powerAmount, g.maxHealth);
            if (g.healthBar != null) g.healthBar.UpdateBar(g.currentHealth, g.maxHealth);
            PlayVFX(data.effectPrefab, g.transform);
        }
    }

    void ExecuteArrowRain(CommanderSkillData data)
    {
        foreach (var g in FindObjectsByType<Gladiator>(FindObjectsSortMode.None))
        {
            if (!g.CompareTag("EnemySoldier")) continue;
            var ai = g.GetComponent<GladiatorAI>();
            if (ai != null && !ai.isDead)
            {
                ai.TakeDamage(data.powerAmount, false);
                PlayVFX(data.effectPrefab, g.transform);
            }
        }
    }

    void ExecuteBuffAll(CommanderSkillData data)
    {
        foreach (var g in FindObjectsByType<Gladiator>(FindObjectsSortMode.None))
        {
            if (!g.CompareTag("MySoldier") || g.currentHealth <= 0) continue;
            g.data.strength += data.powerAmount;
            PlayVFX(data.effectPrefab, g.transform);
        }
    }

    void ExecuteStunAll(CommanderSkillData data)
    {
        foreach (var ai in FindObjectsByType<GladiatorAI>(FindObjectsSortMode.None))
        {
            if (!ai.CompareTag("EnemySoldier") || ai.isDead) continue;
            StartCoroutine(StunRoutine(ai, data.powerAmount)); 
            PlayVFX(data.effectPrefab, ai.transform);
        }
    }

    IEnumerator StunRoutine(GladiatorAI ai, float duration)
    {
        if (ai.agent != null) ai.agent.isStopped = true;
        yield return new WaitForSeconds(duration);
        if (ai != null && !ai.isDead && ai.agent != null) ai.agent.isStopped = false;
    }

    void PlayVFX(GameObject prefab, Transform t)
    {
        if (prefab == null) return;
        var vfx = Instantiate(prefab, t.position + Vector3.up * 0.1f, Quaternion.identity);
        vfx.transform.SetParent(t);
        Destroy(vfx, 2f);
    }
}