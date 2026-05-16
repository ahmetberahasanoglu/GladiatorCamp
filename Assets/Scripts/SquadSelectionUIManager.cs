using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SquadSelectionUIManager : MonoBehaviour
{
    public GameObject panel;
    public Transform rosterContentArea; // Kamptaki tüm askerlerin listeleneceği yer
    public GameObject rosterCardPrefab; // Askerlerin ufak UI kartı

    [Header("Seçilen Asker Yuvaları (Önden Arkaya)")]
    public List<TextMeshProUGUI> slotTexts; // Seçilen askerin adını yazacağımız 3 text
    public Button startBattleButton;

    private List<Gladiator> availableRoster = new List<Gladiator>();
    private List<Gladiator> selectedSquad = new List<Gladiator>();
    private bool _isBossBattle;
    [Header("Komutan Yetenekleri Seçimi")]
    public List<CommanderSkillData> unlockedSkills; 
    public Transform skillInventoryArea; 
    public GameObject skillCardPrefab; 
    
    [Header("Seçilen Yetenek Slotları (Max 3)")]
    public List<Image> selectedSkillSlots; 
    private List<CommanderSkillData> selectedSkills = new List<CommanderSkillData>();

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

    void PopulateRoster()
    {
        // 1. Önce listeyi temizle
        foreach (Transform child in rosterContentArea) Destroy(child.gameObject);
        availableRoster.Clear();

        // 2. Kamptaki tüm MÜSAİT askerleri bul
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        foreach (var soldier in allSoldiers)
        {
            if (soldier.CompareTag("MySoldier") && soldier.data.currentHealth > 0)
            {
                availableRoster.Add(soldier);
            }
        }

        // 3. UI Kartlarını oluştur
        foreach (var soldier in availableRoster)
        {
            GameObject card = Instantiate(rosterCardPrefab, rosterContentArea);
            
            // Kartın içindeki yazıları ayarla (Prefab'ında TextMeshProUGUI'ler olmalı)
            TextMeshProUGUI[] texts = card.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) texts[0].text = soldier.data.gladiatorName;
            if (texts.Length > 1) texts[1].text = $"{Mathf.RoundToInt(soldier.currentHealth)}";
            if (texts.Length > 2) texts[2].text = $"{soldier.data.strength}";

            // Karta tıklanma olayını ata
            Button btn = card.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnSoldierCardClicked(soldier, card));
            }
        }
    }
    void PopulateSkills()
    {
        // Önce temizle
        foreach (Transform child in skillInventoryArea) Destroy(child.gameObject);
        selectedSkills.Clear();
        UpdateSkillSlotVisuals();

        // Envanterdeki yetenekleri listele
        foreach (var skill in unlockedSkills)
        {
            GameObject card = Instantiate(skillCardPrefab, skillInventoryArea);
            
            // Kartın ikonunu ve adını ayarla
            Image icon = card.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI nameText = card.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            
            if (icon != null) icon.sprite = skill.skillIcon;
            if (nameText != null) nameText.text = skill.skillName;

            Button btn = card.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnSkillCardClicked(skill, card));
            }
        }
    }
void OnSkillCardClicked(CommanderSkillData skill, GameObject cardObject)
    {
        // Zaten seçiliyse çıkart
        if (selectedSkills.Contains(skill))
        {
            selectedSkills.Remove(skill);
            cardObject.GetComponent<Image>().color = Color.white; // Seçim iptal rengi
        }
        else
        {
            // Seçili değilse ve slotta yer varsa (Max 3) ekle
            if (selectedSkills.Count < 3) // İleride bu 3 sayısını oyuncunun Kamp yükseltmesine göre değişken yapabilirsin
            {
                selectedSkills.Add(skill);
                cardObject.GetComponent<Image>().color = Color.yellow; // Seçildiğini belli et
            }
            else
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show("En fazla 3 yetenek kuşanabilirsin!", NotificationType.Warning);
            }
        }

        UpdateSkillSlotVisuals();
    }

    void UpdateSkillSlotVisuals()
    {
        // Seçilen yeteneklerin ikonlarını boş yuvalara yerleştir
        for (int i = 0; i < selectedSkillSlots.Count; i++)
        {
            if (i < selectedSkills.Count)
            {
                selectedSkillSlots[i].sprite = selectedSkills[i].skillIcon;
                selectedSkillSlots[i].enabled = true; // İkonu göster
            }
            else
            {
                selectedSkillSlots[i].sprite = null;
                selectedSkillSlots[i].enabled = false; // Boşsa gizle veya silik bir arka plan göster
            }
        }
    }
    void OnSoldierCardClicked(Gladiator soldier, GameObject cardObject)
    {
        // Zaten seçiliyse listeden çıkar
        if (selectedSquad.Contains(soldier))
        {
            selectedSquad.Remove(soldier);
            cardObject.GetComponent<Image>().color = Color.white; // Rengi eski haline çevir
        }
        else
        {
            // Seçili değilse ve slotta yer varsa (Max 3) ekle
            if (selectedSquad.Count < 3)
            {
                selectedSquad.Add(soldier);
                cardObject.GetComponent<Image>().color = Color.green; // Seçildiğini belli et
            }
            else
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show("En fazla 3 asker seçebilirsin!", NotificationType.Warning);
            }
        }

        UpdateSlotVisuals();
    }

    void UpdateSlotVisuals()
    {
        // 3 Slotun yazılarını güncelle
        for (int i = 0; i < slotTexts.Count; i++)
        {
            if (i < selectedSquad.Count)
            {
                // İlk seçilen (Index 0) en öne gider
                string rank = (i == 0) ? "Ön Saf (Tank)" : (i == 1) ? "Orta Saf" : "Arka Saf";
                slotTexts[i].text = $"{rank}: {selectedSquad[i].data.gladiatorName}";
            }
            else
            {
                slotTexts[i].text = "BOŞ SLOT";
            }
        }

        // En az 1 asker seçilmeden savaş başlatılamasın
        startBattleButton.interactable = (selectedSquad.Count > 0);
    }

    void ConfirmSquadAndStartBattle()
    {
        panel.SetActive(false);
       AudioManager.Instance.PlayWarHorn();
       if (BattleSkillManager.Instance != null)
        {
            BattleSkillManager.Instance.LoadSelectedSkills(selectedSkills);
        }
        BattleManager.Instance.ExecuteBattleWithSquad(selectedSquad, _isBossBattle);
    }
}