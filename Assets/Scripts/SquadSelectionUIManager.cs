using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SquadSelectionUIManager : MonoBehaviour
{
    public GameObject panel;
    public Transform rosterContentArea; // Kamptaki tüm askerlerin listeleneceği yer
    
    [Tooltip("DİKKAT: Bu prefabın en üstünde 'SquadSlotUI' scripti takılı olmalıdır!")]
    public GameObject rosterCardPrefab; // Askerlerin animasyonlu UI kartı (SquadSlotUI taşır)

    [Header("Seçilen Asker Yuvaları (Önden Arkaya)")]
    public List<TextMeshProUGUI> slotTexts; // Seçilen askerin adını yazacağımız 3 text
    public Button startBattleButton;

    private List<Gladiator> availableRoster = new List<Gladiator>();
    private List<Gladiator> selectedSquad = new List<Gladiator>();
    private bool _isBossBattle;

    [Header("Komutan Yetenekleri Seçimi")]
    public Transform skillInventoryArea; 
    public GameObject skillCardPrefab; 
    
    [Header("Seçilen Yetenek Slotları (Max 3)")]
    public List<Image> selectedSkillSlots; 
    private List<CommanderSkillData> selectedSkills = new List<CommanderSkillData>();
    private Dictionary<CommanderSkillData, int> availableSpells = new Dictionary<CommanderSkillData, int>();
    
    // Yeteneklerin yumuşak büyüme animasyonları için takipli coroutine sözlüğü
    private Dictionary<GameObject, Coroutine> activeSkillAnimations = new Dictionary<GameObject, Coroutine>();

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        startBattleButton.onClick.AddListener(ConfirmSquadAndStartBattle);
    }

    public void OpenPanel(bool isBoss, int enemyCount, int difficulty)
    {
        _isBossBattle = isBoss;
        selectedSquad.Clear();
        UpdateSlotVisuals();

        panel.SetActive(true);
        PopulateRoster();
        PopulateSkills();
    }

    // ── ASKER LİSTESİ (SQUAD SLOT UI ENTEGRASYONU) ───────────────────────────
    void PopulateRoster()
    {
        // 1. Önce hiyerarşiyi temizle
        foreach (Transform child in rosterContentArea) Destroy(child.gameObject);
        availableRoster.Clear();

        // 2. Kamptaki tüm MÜSAİT ve canlı askerleri bul
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        foreach (var soldier in allSoldiers)
        {
            if (soldier.CompareTag("MySoldier") && soldier.data.currentHealth > 0)
            {
                availableRoster.Add(soldier);
            }
        }

        // 3. UI Kartlarını oluştur ve SquadSlotUI'a bağla
        foreach (var soldier in availableRoster)
        {
            GameObject card = Instantiate(rosterCardPrefab, rosterContentArea);
             TextMeshProUGUI[] texts = card.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 1) texts[1].text = $"{Mathf.RoundToInt(soldier.currentHealth)}";
            SquadSlotUI slotUI = card.GetComponent<SquadSlotUI>();

            if (slotUI != null)
            {
                // Sefer sistemindeki akıllı Setup'ı çağırıyoruz!
                slotUI.Setup(soldier, (glad, isSelected) => 
                {
                    // Tıklama olayı tetiklendiğinde çalışacak akıllı mantık:
                    if (isSelected)
                    {
                        // 3 kişilik ordu sınır kontrolü
                        if (selectedSquad.Count < 3)
                        {
                            selectedSquad.Add(glad);
                        }
                        else
                        {
                            // Sınır aşıldıysa kartın kendi içindeki seçimi zorla GERİ İPTAL ET (Animasyonu geri sarar)
                            slotUI.ForceDeselect(); 

                            if (NotificationManager.Instance != null)
                                NotificationManager.Instance.Show("En fazla 3 asker seçebilirsin!", NotificationType.Warning);
                        }
                    }
                    else
                    {
                        // Seçim iptal edildiyse listeden çıkar
                        selectedSquad.Remove(glad);
                    }

                    // Üst bar yazılarını/slot durumlarını tazele
                    UpdateSlotVisuals();
                });
            }
            else
            {
                Debug.LogWarning("[SquadSelection] rosterCardPrefab üzerinde 'SquadSlotUI' bileşeni bulunamadı!");
            }
        }
    }

    // ── KOMUTAN YETENEKLERİ LİSTESİ ─────────────────────────────────────────
    void PopulateSkills()
    {
        foreach (Transform child in skillInventoryArea) Destroy(child.gameObject);
        selectedSkills.Clear();
        availableSpells.Clear();
        activeSkillAnimations.Clear();
        UpdateSkillSlotVisuals();

        if (CommanderStorage.Instance != null)
        {
            foreach (var nusha in CommanderStorage.Instance.ownedNushas)
            {
                if (nusha?.spellData == null) continue;
                if (availableSpells.ContainsKey(nusha.spellData))
                    availableSpells[nusha.spellData]++;
                else
                    availableSpells[nusha.spellData] = 1;
            }
        }

        foreach (var kvp in availableSpells)
        {
            CommanderSkillData skill = kvp.Key;
            int count = kvp.Value;

            GameObject card = Instantiate(skillCardPrefab, skillInventoryArea);
            
            Image icon = card.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI nameText = card.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            Transform countObj = card.transform.Find("CountText");

            if (countObj != null) countObj.GetComponent<TextMeshProUGUI>().text = $"x{count}";
            if (icon != null) icon.sprite = skill.skillIcon;
            if (nameText != null) nameText.text = skill.skillName;

            Button btn = card.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnSkillCardClicked(skill, card));
            }
        }
    }

    // ── YETENEK KARTLARINA ÖZEL JUICE (ANİMASYONLU SEÇİM) ────────────────────
    void OnSkillCardClicked(CommanderSkillData skill, GameObject cardObject)
    {
        Image bgImage = cardObject.GetComponent<Image>();

        if (selectedSkills.Contains(skill))
        {
            selectedSkills.Remove(skill);
            if (bgImage != null) bgImage.color = Color.white; // Normale dön
            
            // Küçülme animasyonunu tetikle
            AnimateSkillCardScale(cardObject, Vector3.one);
        }
        else
        {
            if (selectedSkills.Count < 3)
            {
                selectedSkills.Add(skill);
                // Seçildiğinde efsanevi altın sarısı parlasın
                if (bgImage != null) bgImage.color = new Color(1f, 0.85f, 0.2f, 1f); 
                
                // Tıpkı asker kartları gibi hafifçe öne çıksın (Büyüsün)
                AnimateSkillCardScale(cardObject, Vector3.one * 1.06f);
            }
            else
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show("En fazla 3 yetenek kuşanabilirsin!", NotificationType.Warning);
            }
        }

        UpdateSkillSlotVisuals();
    }

    void AnimateSkillCardScale(GameObject targetCard, Vector3 targetScale)
    {
        if (activeSkillAnimations.ContainsKey(targetCard) && activeSkillAnimations[targetCard] != null)
        {
            StopCoroutine(activeSkillAnimations[targetCard]);
            activeSkillAnimations.Remove(targetCard);
        }

        Coroutine anim = StartCoroutine(SmoothScaleSkillCard(targetCard, targetScale));
        activeSkillAnimations.Add(targetCard, anim);
    }

    IEnumerator SmoothScaleSkillCard(GameObject targetCard, Vector3 targetScale)
    {
        if (targetCard == null) yield break;
        Vector3 startScale = targetCard.transform.localScale;
        float elapsed = 0f;
        float duration = 0.12f; // SquadSlotUI ile aynı hızda pürüzsüz geçiş

        while (elapsed < duration)
        {
            if (targetCard == null) yield break;
            elapsed += Time.unscaledDeltaTime;
            targetCard.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            yield return null;
        }
        if (targetCard != null) targetCard.transform.localScale = targetScale;
    }

    // ── GÖRSEL YAZI VE SLOT TAZELEMELERİ ─────────────────────────────────────
    void UpdateSkillSlotVisuals()
    {
        for (int i = 0; i < selectedSkillSlots.Count; i++)
        {
            if (i < selectedSkills.Count)
            {
                selectedSkillSlots[i].sprite = selectedSkills[i].skillIcon;
                selectedSkillSlots[i].enabled = true; 
            }
            else
            {
                selectedSkillSlots[i].sprite = null;
                selectedSkillSlots[i].enabled = false; 
            }
        }
    }

    void UpdateSlotVisuals()
    {
        for (int i = 0; i < slotTexts.Count; i++)
        {
            if (i < selectedSquad.Count)
            {
                string rank = (i == 0) ? "Ön Saf (Tank)" : (i == 1) ? "Orta Saf" : "Arka Saf";
                slotTexts[i].text = $"{rank}: {selectedSquad[i].data.gladiatorName}";
            }
            else
            {
                slotTexts[i].text = "BOŞ SLOT";
            }
        }

        startBattleButton.interactable = (selectedSquad.Count > 0);
    }

    void ConfirmSquadAndStartBattle()
    {
        panel.SetActive(false);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayWarHorn();
        
        if (BattleSkillManager.Instance != null)
        {
            BattleSkillManager.Instance.LoadSelectedSkills(selectedSkills);
        }
        BattleManager.Instance.ExecuteBattleWithSquad(selectedSquad, _isBossBattle);
    }
}