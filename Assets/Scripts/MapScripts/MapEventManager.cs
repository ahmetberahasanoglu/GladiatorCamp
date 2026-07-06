using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MapEventManager : MonoBehaviour
{
    public static MapEventManager Instance;
[Header("Nüsha / Ferman Havuzları")]
    public List<ItemData> yayginNushalar; // Şifa, Ufak Güç vs.
    public List<ItemData> nadirFermanlar;
    [Header("UI Elemanları")]
    public GameObject eventPanel;       
    public GameObject endDayPanel;       
    public GameObject topPanel; 
    public Image eventImage;            
    public TextMeshProUGUI titleText;   
    public TextMeshProUGUI descText;    
    
    public Transform buttonContainer;   
    public GameObject buttonPrefab;     

    [Header("Olay Görselleri")]
    public Sprite villageSprite;  
    public Sprite nasibSprite;  
    public Sprite battleSprite;
    public Sprite treasureSprite;
    public Sprite bossSprite;
    public Sprite archerySprite;
    public Sprite horseSprite;
    public Sprite towerSprite;
    public Sprite tuccarSprite;
    public Sprite shieldSprite;
    public Sprite dungeonSprite;
    public Sprite wildSprite;
    public Sprite merchant; 
    public Sprite kervanSprite; 
    public Sprite caravanSprite; 
    public Sprite asikSprite; 
    public Sprite cenkSprite; 
    public Sprite dervishSprite; 
  
    public Sprite diceSprite; 
    public Sprite wrestlingSprite; 
    public Sprite nightraidSprite; 
  

   
    [Header("Mini Oyun Rakipleri")]
    public string atYarisiRakipIsim = "Mustafa Bey'in Atı";
    public int atYarisiRakipHiz = 12; 

    public string guresRakipIsim = "Gaddar Rüstem";
    public int guresRakipGuc = 15;   

    void Awake()
    {
        Instance = this;
        if (eventPanel != null) eventPanel.SetActive(false); 
    }

    public void TriggerEvent(NodeType type)
    {
        eventPanel.SetActive(true);
        AudioManager.Instance.PlayPaper();
        
        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = buttonContainer.GetChild(i);
            child.SetParent(null);     
            Destroy(child.gameObject); 
        }

        switch (type)
        {
            case NodeType.Village:      SetupVillageEvent(); break;
            case NodeType.Battle:       SetupBattleEvent(); break;
            case NodeType.Boss:         SetupBossEvent(); break;
            case NodeType.StartPoint:   SetupStartEvent(); break;
            case NodeType.Archery:      SetupArcheryEvent(); break;
            case NodeType.Atyarisi:     SetupAtYarisi(); break;
            case NodeType.KuleSavas:    SetupKuleSavas(); break;
            case NodeType.Treasure:     SetupTreasure(); break;
            case NodeType.Tuccar:       SetupTuccar(); break;
            case NodeType.Kalkan:       SetupKalkan(); break;
            case NodeType.RestArea:     SetupRestEvent(); break;
            case NodeType.Zindan:       SetupZindan(); break;
            case NodeType.Vahsi:        SetupVahsi(); break;
            case NodeType.Dice:         SetupMysteriousDiceEvent(); break;
            case NodeType.YagliGures:   SetupYagliGures(); break;
            case NodeType.FirstBattle:   SetupFirstBattleEvent(); break;
            case NodeType.NasibEncounter:   SetupNasibEvent(); break;
            case NodeType.DervishEncounter:   SetupDervishEvent(); break;
            case NodeType.CaravanEncounter:   SetupCaravanEvent(); break;
            case NodeType.KervanEncounter:   SetupKervansarayEvent(); break;
            case NodeType.CenkOyunu:          SetupCenkEvent(); break;
            case NodeType.AsikAtismasi:          SetupAsikEvent(); break;
            case NodeType.NightRaid:          SetupNightRaidEvent(); break;
        }
    }
void AddReward(int gold = 0, int rep = 0)
    {
        if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
            ExpeditionManager.Instance.AddLoot(gold, rep);
        else
        {
            if (gold > 0) MoneyManager.Instance?.Add(gold);
            else if (gold < 0) MoneyManager.Instance?.Spend(Mathf.Abs(gold));
            if (rep != 0) ReputationManager.Instance?.ChangeReputation(rep);
        }
    }

    // Harcamalar her zaman direkt kasadan
    bool Spend(int amount) => MoneyManager.Instance != null && MoneyManager.Instance.Spend(amount);
    int  Gold              => MoneyManager.Instance != null ? MoneyManager.Instance.gold : 0;
 
    private void AdjustButtonContainerWidth()
    {
        int count = buttonContainer.childCount;
        float targetWidth = 454f; 
        
    
        if (count > 2)
        {
            targetWidth += (count - 2) * 244f; 
        }


        RectTransform rt = buttonContainer.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(targetWidth, rt.sizeDelta.y);
    }
   void SetupVahsi()
    {
        titleText.text = "Aç Boz Ayı";
        descText.text = "Ormanın derinliklerinden devasa bir boz ayı kükreyerek üzerine koşuyor! Askerlerin silahlarına sarıldı. Bu vahşi yaratığı alt edebilirseniz postu ve eti epey para edecektir.";
        if(wildSprite != null) eventImage.sprite = wildSprite;

        int readySoldiers = GetAvailableSoldierCount();
    
        if (readySoldiers == 0)
        {
            descText.text += "\n\nSAVAŞA HAZIR ASKER YOK!\nHerkes görevde, çalışıyor veya yaralı.";
        }
        else
        {
            descText.text += $"\n\nSavaşa Hazır Asker: {readySoldiers}";
        }

        // SAVAŞ BUTONU
        GameObject atkBtnObj = Instantiate(buttonPrefab, buttonContainer);
        atkBtnObj.GetComponentInChildren<TextMeshProUGUI>().text = "Saldır (Kılıçları Çekin)";
        Button atkBtn = atkBtnObj.GetComponent<Button>();
        
        if (readySoldiers == 0)
        {
            atkBtn.interactable = false; 
        }
        else
        {
            atkBtn.onClick.AddListener(() => {
                ClosePanel();
                // 1 Adet Ayı, Zorluk: 2, Ortam: Orman
                BattleManager.Instance.StartBearBattle(1, 2, BattleEnvironment.Forest); 
            });
        }

        // KAÇIŞ — Ayıdan kaçmak zor, yüksek nasip gerekir
        AddEscapeButton(
            escapeText:    "Dağılın, Kaçın!",
            escapeType:    EscapeType.Optional,
            nasipRequired: 5,
            onEscape:      () => { ResolveEscapeContest(BattleEnvironment.Forest, 1, 2); }
        );
    }
    void SetupNasibEvent()
    {
        titleText.text = "Yoldaki Derviş";
        descText.text = "Üstü başı yırtık, yaşlı bir derviş yol kenarında bir ağacın dibinde oturuyor. Sana doğru bakıp titreyen bir sesle mırıldandı:\n\n\"Yolcu... Allah rızası için bir lokma ekmek, üç beş akçe sadaka verir misin?\"";

        if(villageSprite != null) eventImage.sprite = villageSprite; 

        CreateButton("Sadaka Ver (-20 Altın)", () => {
            if (MoneyManager.Instance.gold >= 20)
            {
                MoneyManager.Instance.Spend(20);
                
                // Nasip ve İtibar artar
                if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(1);
                if (ReputationManager.Instance != null) ReputationManager.Instance.ChangeReputation(5);
                
                NotificationManager.Instance.Show("Derviş sana hayır duası etti. Gönlüne bir ferahlık geldi.", NotificationType.Success);
            }
            else
            {
                NotificationManager.Instance.Show("Sadaka verecek kadar bile altının yok...", NotificationType.Error);
            }
            ClosePanel();
        });

        CreateButton("Görmezden Gel", () => {
            

            if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(1);
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5);
            
            NotificationManager.Instance.Show("Derviş arkandan sessizce baktı. İçini büyük bir sıkıntı bastı.", NotificationType.Warning);
            ClosePanel();
        });
    }
 public void SetupMysteriousDiceEvent()
    {
        titleText.text = "Gizemli Yabancı";
        descText.text = "Karanlık bir pelerin giymiş bir adam yolunu kesti. Avucundan kemikten yapılma bir zar çıkardı.\n\n\"Şu zarı at bakalım Uç Beyi... Eğer 3'ten büyük atarsan sana tam 1000 Akçe vereceğim. Ama kaybedersen, en değerli şeylerinden birini... ordundan bir canı alırım.\"";
        
        if(diceSprite != null) eventImage.sprite = diceSprite;

        CreateButton("Zarı At (Ölümcül Risk)", () => 
        {
            AudioManager.Instance.PlayDice();
            
            DiceManager.Instance.RollDice(6, (zarSonucu) => 
            {
                foreach(Transform child in buttonContainer) Destroy(child.gameObject);

                int mevcutNasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
                int totalScore = zarSonucu + mevcutNasip;
                int hedef = 4; 
                string mathText = $"\n\n<size=85%><b>Zar ({zarSonucu}) + Nasip ({mevcutNasip}) = {totalScore}</b> / Hedef ({hedef})</size>\n";

                if (totalScore >= hedef) 
                {
                    descText.text = $"KAZANDIN!\n\nAdam şaşkınlıkla sana baktı. Elindeki devasa keseyi ayaklarının dibine fırlattı ve tek kelime etmeden karanlıkta kayboldu." + mathText;
                    
                    MoneyManager.Instance.Add(1000); 
                    if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(15);
                    AudioManager.Instance.PlayCheer(); 
                    
                    CreateButton("Devam Et", () => { ClosePanel(); });
                }
                else
                {
                    descText.text = $"KAYBETTİN!\n\nAdam korkunç bir kahkaha attı. \"Kaybettin Uç Beyi... Söz verdiğin gibi, şimdi bana bir can ver!\"\n\n(Kimi feda edeceksin?)" + mathText;
                    ShowSacrificeSelection();
                }
            });
        });

        CreateButton("Adamı Kov", () => 
        {
            NotificationManager.Instance.Show("Şeytanla pazarlık yapmam deyip yoluna devam ettin.", NotificationType.Info);
            ClosePanel();
        });
    }

    private void ShowSacrificeSelection()
    {
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        bool hasEligibleSoldier = false;

        foreach (var soldier in allSoldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead) && soldier.data != null && soldier.data.currentHealth > 0)
            {
                if (soldier.data.currentActivity == SoldierActivity.OnMission) continue;

                hasEligibleSoldier = true;
                
                string btnText = $"{soldier.data.gladiatorName} Feda Et";
                
                Gladiator doomedSoldier = soldier; 
                CreateButton(btnText, () => {
                    ExecuteSacrifice(doomedSoldier);
                });
            }
        }

        if (!hasEligibleSoldier)
        {
            descText.text += "\n\n\"Alacak bir can bulamıyorum... Ordun zaten tükenmiş!\" diyerek alay etti ve hazinenden 500 Akçe çalıp kayboldu.";
            
            if (MoneyManager.Instance.gold >= 500) MoneyManager.Instance.Spend(500);
            else if (MoneyManager.Instance.gold > 0) MoneyManager.Instance.Spend(MoneyManager.Instance.gold);

            CreateButton("Devam Et", () => { ClosePanel(); });
        }
    }


    private void ExecuteSacrifice(Gladiator doomedSoldier)
    {
        foreach(Transform child in buttonContainer) Destroy(child.gameObject);

        string doomedName = doomedSoldier.data.gladiatorName;

        doomedSoldier.data.currentHealth = 0;
        GladiatorAI ai = doomedSoldier.GetComponent<GladiatorAI>();
        
        if (ai != null) ai.Die();
        else Destroy(doomedSoldier.gameObject);

        descText.text = $"Adam kara pelerininin içinden elini uzattı. {doomedName} acı dolu bir çığlık atarak oracıkta cansız yere yığıldı. Gözleri kararırken kampa ağır bir sessizlik çöktü...";
        
        if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-25);

        CreateButton("Devam Et", () => { ClosePanel(); });
    }
    void SetupZindan()
    {
        titleText.text = "Karanlık Mağara";
        descText.text = "İçeriden garip sesler geliyor. Büyük bir tehlike ama aynı zamanda büyük bir ganimet yatıyor olabilir.";
        if(dungeonSprite != null) eventImage.sprite = dungeonSprite;
        int readySoldiers = GetAvailableSoldierCount();
    
        if (readySoldiers == 0)
        {
            descText.text += "\n\nSAVAŞA HAZIR ASKER YOK!\nHerkes görevde, çalışıyor veya yaralı.";
        }
        else
        {
            descText.text += $"\n\nSavaşa Hazır Asker: {readySoldiers}";
        }
        GameObject enterBtnObj = Instantiate(buttonPrefab, buttonContainer);
        enterBtnObj.GetComponentInChildren<TextMeshProUGUI>().text = "İçeri Gir (Savaş)";
        Button enterBtn = enterBtnObj.GetComponent<Button>();
        
        if (readySoldiers == 0)
        {
            enterBtn.interactable = false; 
        }
        else
        {
            enterBtn.onClick.AddListener(() => {
               // AudioManager.Instance."PlayWarHorn"();
                ClosePanel();
                
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.Show("Zindana girdin! Kılıçlar çekilsin!", NotificationType.Warning);
                }
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.StartBattle(3, 2, BattleEnvironment.Cave, GetCurrentTier()); 
                }
            });
        }

        CreateButton("Tehlikeye Atılma (Yola Devam Et)", () => {
            ClosePanel();
            
            // Opsiyonel: Zindandan kaçmak Nasip veya Moral düşürebilir, 
            // şimdilik sadece güvenli bir şekilde yola devam ediyorlar.
        });
    }
    void SetupKalkan()
    {
        titleText.text = "Terk Edilmiş Karakol";
        descText.text = "Eski ve terk edilmiş bir gözetleme kulesi buldun. Askerlerin geride bıraktığı sağlam kalkanlar ve silahlar var.";
        
        CreateButton("Ganimetleri Topla (+30 Altın)", () => {
            
        
            int currentPending = PlayerPrefs.GetInt("PendingGold", 0);
            PlayerPrefs.SetInt("PendingGold", currentPending + 30);
            PlayerPrefs.Save();
            NotificationManager.Instance.Show("Eşyaları alıp sattın.", NotificationType.Success);
            ClosePanel();
        });

        CreateButton("Burası Tekinsiz, Ayrıl", () => {
            
            ClosePanel();
        });
    }

   void SetupTuccar()
    {
        titleText.text = "Gezgin Tüccar";
        if (merchant != null) eventImage.sprite = merchant;
        descText.text = "Uzak diyarlardan gelmiş kurnaz tüccar Salih sana mallarını sunuyor. Bugün arabasında şunlar var:\n\n";

        // Tüccarın satacağı 2 farklı eşyayı rastgele seçelim
        List<int> shopPool = new List<int> { 0, 1, 2, 3 }; // 0: Nüsha, 1: Odun, 2: Erzak, 3: İtibar
        int firstItem = shopPool[UnityEngine.Random.Range(0, shopPool.Count)];
        shopPool.Remove(firstItem);
        int secondItem = shopPool[UnityEngine.Random.Range(0, shopPool.Count)];

        // İç içe fonksiyon ile tüccar butonlarını dinamik oluşturuyoruz
        void AddStoreItem(int itemType)
        {
            switch (itemType)
            {
                case 0:
                    int spellCost = 100;
                    CreateButton($"Gizemli Nüsha Satın Al ({spellCost} Akçe)", () =>
                    {
                        if (MoneyManager.Instance.gold >= spellCost) {
                            MoneyManager.Instance.Spend(spellCost);
                            
                            // TODO: Mevcut Nüsha/Büyü verme kodunu buraya ekle
                            
                            NotificationManager.Instance.Show("Nüsha Çantaya Eklendi!", NotificationType.Success);
                            ClosePanel(); // Alışveriş sonrası paneli kapat
                        } else NotificationManager.Instance.Show("Yeterli Akçen yok!", NotificationType.Error);
                    });
                    break;

                case 1:
                    int woodAmount = UnityEngine.Random.Range(20, 51);
                    int woodCost = woodAmount * 2; // Odun başı 2 akçe
                    CreateButton($"{woodAmount} Odun Satın Al ({woodCost} Akçe)", () =>
                    {
                        if (MoneyManager.Instance.gold >= woodCost) {
                            MoneyManager.Instance.Spend(woodCost);
                            
                            // TODO: Kendi odun ekleme kodunu yaz (örn: ResourceManager.Instance.AddWood(woodAmount))
                            
                            NotificationManager.Instance.Show($"{woodAmount} Odun Kampa Gönderildi!", NotificationType.Success);
                            ClosePanel();
                        } else NotificationManager.Instance.Show("Yeterli Akçen yok!", NotificationType.Error);
                    });
                    break;

                case 2:
                    int foodAmount = UnityEngine.Random.Range(15, 40);
                    int foodCost = foodAmount * 3; // Erzak başı 3 akçe
                    CreateButton($"{foodAmount} Erzak Satın Al ({foodCost} Akçe)", () =>
                    {
                        if (MoneyManager.Instance.gold >= foodCost) {
                            MoneyManager.Instance.Spend(foodCost);
                            
                            // TODO: Kendi erzak ekleme kodunu yaz (örn: ResourceManager.Instance.AddFood(foodAmount))
                            
                            NotificationManager.Instance.Show($"{foodAmount} Erzak Depoya Eklendi!", NotificationType.Success);
                            ClosePanel();
                        } else NotificationManager.Instance.Show("Yeterli Akçen yok!", NotificationType.Error);
                    });
                    break;

                case 3:
                    int repCost = 150;
                    CreateButton($"Saray İtibarını Artır (+10) ({repCost} Akçe)", () =>
                    {
                        if (MoneyManager.Instance.gold >= repCost) {
                            MoneyManager.Instance.Spend(repCost);
                            
                            // TODO: İtibar artırma kodunu yaz (örn: ReputationManager.Instance.AddReputation(10))
                            
                            NotificationManager.Instance.Show("Saraydaki İtibarın Arttı!", NotificationType.Success);
                            ClosePanel();
                        } else NotificationManager.Instance.Show("Yeterli Akçen yok!", NotificationType.Error);
                    });
                    break;
            }
        }

        // İki rastgele butonu oluştur
        AddStoreItem(firstItem);
        AddStoreItem(secondItem);

        // Alışveriş yapmak istemezse her zaman ayrılma seçeneği olmalı
        CreateButton("Teşekkür Et ve Ayrıl", () => { ClosePanel(); });
    }

   void SetupTreasure()
    {
        titleText.text = "Gizli Hazine";
        descText.text = "Ormanın derinliklerinde eski, ahşap bir sandık duruyor. Üzerindeki işlemeler çok değerli olduğunu gösteriyor... ama bir şeyler garip.";
        if(treasureSprite != null) eventImage.sprite = treasureSprite;
       CreateButton("Sandığı Aç", () => {
        ClearAllButtons(); 
        int roll = Random.Range(0, 100);
        
        if (roll < 75) // %75 İhtimalle Normal Hazine
        {
            int goldAmount = Random.Range(80, 150); 
            if (MoneyManager.Instance != null) MoneyManager.Instance.Add(goldAmount);
            
            titleText.text = "Göz Kamaştıran Altınlar!";
            descText.text = $"Sandığı açtın ve {goldAmount} Akçe buldun!";

            // ── YENİ: %40 İHTİMALLE İÇİNDEN NÜSHA ÇIKSIN ──
            if (Random.Range(0, 100) < 40 && yayginNushalar.Count > 0)
            {
                ItemData kazanilanNusha = yayginNushalar[Random.Range(0, yayginNushalar.Count)];
                
                // Nüshayı geçici sefer çantasına ekliyoruz
                ExpeditionManager.Instance.tempItems.Add(kazanilanNusha);
                
                descText.text += $"\n\nAltınların altında parlayan, üzeri dualarla kaplı bir kağıt buldun:\n+ {kazanilanNusha.itemName} çantaya eklendi!";
            }
            // ──────────────────────────────────────────────

            CreateButton("Harika! (Devam Et)", () => { ClosePanel(); });
        }
        else 
        {
            ResolveMimicEncounter();
        }
    });

        CreateButton("Vaktim Yok, İlerle", () => {
            ClosePanel();
        });
    }

    public void ClearAllButtons()
    {
        if (buttonContainer != null)
        {
            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ResolveMimicEncounter()
    {
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        System.Collections.Generic.List<Gladiator> availableSoldiers = new System.Collections.Generic.List<Gladiator>();

        foreach (var soldier in allSoldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead))
            {
                availableSoldiers.Add(soldier);
            }
        }

        if (availableSoldiers.Count == 0)
        {
            titleText.text = "Kıl Payı Kurtuluş";
            descText.text = "Sandık aniden devasa dişli bir ağza dönüştü! Neyse ki yutacağı kimse yoktu, hızla oradan uzaklaştın.";
            CreateButton("Hemen Uzaklaş!", () => { ClosePanel(); });
            return;
        }

        Gladiator victim = availableSoldiers[Random.Range(0, availableSoldiers.Count)];
        string victimName = victim.data != null ? victim.data.gladiatorName : "Bir asker";

        int nasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
        int survivalChance = 40 + (nasip * 10);
        survivalChance = Mathf.Clamp(survivalChance, 5, 95); 

        int survivalRoll = Random.Range(0, 100);

        if (survivalRoll < survivalChance)
        {
            // KURTULDU (Zar Başarılı)
            titleText.text = "MİMİC TUZAĞI!";
            descText.text = $"Sandık aniden devasa, salyalı bir ağza dönüştü ve {victimName}'e saldırdı!\n\nNasibiniz yaver gitti... Asker son anda kolunu kurtarmayı başardı ama herkesin yüreği ağzına geldi.";
            
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5);
            
            CreateButton("Ucuz Atlattık (Devam Et)", () => { ClosePanel(); });
        }
        else
        {
            // ÖLDÜ (Zar Başarısız)
            titleText.text = "DEHŞET VERİCİ ÖLÜM!";
            descText.text = $"Sandık aslında devasa bir Mimic canavarıydı! {victimName} ne olduğunu bile anlayamadan çığlıklar içinde tek lokmada yutuldu!\n\nOrdu bu vahşet karşısında dehşete düştü ve moral darmadağın oldu.";
            
            if (GladiatorSelector.Instance != null) GladiatorSelector.Instance.DeselectIfDead(victim.gameObject);
            Destroy(victim.gameObject);

            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-15);
            if (TopInfoBarUI.Instance != null) TopInfoBarUI.Instance.UpdateCapacity();

            CreateButton("Allah Rahmet Eylesin (Kapat)", () => { ClosePanel(); });
        }
    }
   
     public void SetupVillageEvent()
    {
        titleText.text = "İşgal Edilmiş Köy";
        if (villageSprite != null) eventImage.sprite = villageSprite;

        int tier = GetCurrentTier();
        descText.text =
            "Ufukta dumanlar tüten bir köy belirdi. Yaklaştıkça durumu anlıyorsun; " +
            "acımasız bir eşkıya grubu köyü işgal etmiş. Ahalinin çığlıkları ta buraya kadar geliyor.\n\n" +
            "Köylüler kapıda sana bakıyor. Yardım edecek misin?\n\n" +
            $"3 dalga haydut — Tier {tier}";

     CreateButton("Kılıcını Çek ve Meydana İn!", () => {

        CampManager.Instance.activeMissionArmy.Clear();
        foreach (var soldier in CampManager.Instance.GetLivingSoldiers())
        {
            CampManager.Instance.activeMissionArmy.Add(soldier.data); 
        }
Debug.Log($"[SEFER] {CampManager.Instance.activeMissionArmy.Count} asker çantaya alındı, savaşa gidiliyor!");
if (MapManager.Instance != null) MapManager.Instance.isMapOpen = false;
        // 2. Sahneyi Yükle
        UnityEngine.SceneManagement.SceneManager.LoadScene("VillageDefenseScene"); 
    });

        // Kaçmak — nasip cezalı
        AddEscapeButton(
            escapeText:        "Bulaşma, etrafından dolan",
            escapeType:        EscapeType.Costly,
            reputationPenalty: 0,   // İtibar değil nasip cezası
            onEscape: () =>
            {
                NasipManager.Instance?.SpendNasip(1);
                CampMoraleManager.Instance?.ChangeMorale(-10);
                NotificationManager.Instance?.Show(
                    "Köylüleri terk ettin. Nasip azaldı, moral bozuldu.",
                    NotificationType.Warning);
                ClosePanel();
            }
        );
    }
  
    void SetupStartEvent()
    {
        titleText.text = "Kamp";
        descText.text = "Kampda dinlen ve hazırlıklarını yap.";
        if(villageSprite != null) eventImage.sprite = villageSprite;
        
        CreateButton("Kampa Geç", () => {
            if (TutorialManager.Instance != null && TutorialManager.Instance.currentStep == TutorialStep.Intro_CampPanel)
            {
                TutorialManager.Instance.AdvanceTutorial(); 
            }
            ClosePanel();
            topPanel.SetActive(true);
            BattleManager.Instance.ReturnToCamp();
        });
    }/*
private void ResolveVillageBattle(int difficulty)
    {
        foreach(Transform child in buttonContainer) Destroy(child.gameObject);
        AudioManager.Instance.PlayWarHorn(); 
        DiceManager.Instance.RollDice(6, (zarSonucu) => 
        {
            int orduGucu = 0;
            Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
            foreach (var soldier in allSoldiers)
            {
                GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
                if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead) && soldier.data != null)
                {
                    orduGucu += soldier.data.strength; 
                }
            }

            int nasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
            int totalScore = orduGucu + zarSonucu + nasip;
            bool isWin = totalScore >= difficulty;


            string mathText = $"\n\n<size=85%><b>Ordu Gücü ({orduGucu}) + Zar ({zarSonucu}) + Nasip ({nasip}) = {totalScore}</b> / Düşman Zorluğu ({difficulty})</size>\n";

            if (isWin)
            {
                descText.text = $"KESİN ZAFER!\n\nAskerlerinle köy meydanına daldın ve işgalcileri darmadağın ettin! Sağ kalanlar kılıçlarını atıp diz çöktü.\n" + mathText;

 
                CreateButton("Köyün Kaderini Belirle", () => {
                    SetupVillageVictoryEvent(); 
                });
            }
            else
            {
                descText.text = $"AĞIR YENİLGİ!\n\nİşgalciler beklediğinden çok daha kalabalıktı! Ağır yaralar alarak ormana geri çekilmek zorunda kaldınız. Kaçarken bir askerimiz geride kaldı...\n" + mathText;

                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-20);

                CreateButton("Kayıpları Say...", () => {
                    ShowSacrificeSelection(); 
                });
            }
        });
    }
    public void SetupVillageVictoryEvent()
    {
        foreach(Transform child in buttonContainer) Destroy(child.gameObject);

        titleText.text = "Köyün Kaderi";
        descText.text = "Savaş bitti! Köyün yaşlısı titreyerek ayaklarına kapandı: \"Bizi kurtardın Uç Beyi! Allah senden razı olsun... Ama neyimiz var neyimiz yoksa bu sandıktadır, yalvarırım köyümüze dokunma!\"\n\nAskerlerin ise ganimet hırsıyla ambarlara bakıyor...";


        CreateButton("Köyü Yağmalayın! (Zulüm)", () => 
        {
            foreach(Transform child in buttonContainer) Destroy(child.gameObject);
            
            descText.text = "ZULÜM!\n\nKurtarıcı olarak girdiğin köyden zalim olarak çıkıyorsun. Askerlerin evleri yağmaladı, ambarları boşalttı. Hazinen doldu taştı ama şerefin iki paralık oldu!\n\n<size=85%>(Nasip Sıfırlandı! Ağır Moral Kaybı... +300 Akçe, +150 Erzak)</size>";
            
            MoneyManager.Instance.Add(300);
            SupplyManager.Instance.AddFood(150);
            
            if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(NasipManager.Instance.maxNasip); 
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-30); 
            
            CreateButton("Lanetlenmiş Olarak Devam Et", () => { ClosePanel(); });
        });

        // SEÇENEK 2: Adalet ve Erdem
        CreateButton("Ahaliye Dokunmayın! (Merhamet)", () => 
        {
            foreach(Transform child in buttonContainer) Destroy(child.gameObject);
            
            descText.text = "ADALET!\n\n\"Biz harami değiliz! Sadece ölen işgalcilerin silahlarını alın!\" diye gürledin. \n\nKöy halkı sevinç gözyaşlarıyla sana dualar etti. Askerlerin onurlu bir komutanın emrinde olmaktan gurur duydu.\n\n<size=85%>(Nasip Arttı! +50 Akçe, +20 Erzak)</size>";
            
            MoneyManager.Instance.Add(50);
            SupplyManager.Instance.AddFood(20);
            
            if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(2);
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(20); 
            
            CreateButton("Onurla Yola Devam Et", () => { ClosePanel(); });
        });
    }*/
   void SetupFirstBattleEvent()
    {
        titleText.text = "Çapulcu Pusu";
        descText.text = "Yol kesen eşkıyalar! Savaşmak zor olacak ama ganimetleri iyi görünüyor.";
        if(battleSprite != null) eventImage.sprite = battleSprite;
        
        int readySoldiers = GetAvailableSoldierCount();
    
        if (readySoldiers == 0)
        {
            descText.text += "\n\nSAVAŞA HAZIR ASKER YOK!\nHerkes görevde, çalışıyor veya yaralı.";
        }
        else
        {
            descText.text += $"\n\nSavaşa Hazır Asker: {readySoldiers}";
        }

        GameObject atkBtnObj = Instantiate(buttonPrefab, buttonContainer);
        atkBtnObj.GetComponentInChildren<TextMeshProUGUI>().text = "Saldır";
        Button atkBtn = atkBtnObj.GetComponent<Button>();
        
        if (readySoldiers == 0)
        {
            atkBtn.interactable = false; 
        }
        else
        {
            atkBtn.onClick.AddListener(() => {
                if (TutorialManager.Instance != null && TutorialManager.Instance.currentStep == TutorialStep.Map_FirstBattlePanel)
                {
                    TutorialManager.Instance.AdvanceTutorial(); 
                }
                //AudioManager.Instance.PlayWarHorn();
                //DayManager.Instance.NextDay(3); 
                ClosePanel();
                BattleManager.Instance.StartBattle(2, 1, BattleEnvironment.Forest, GetCurrentTier()); 
            });
        }
       CreateButton("Geri Dön (Kampa Bak)", () => {
            
           
            ClosePanel();

        
            if (MapManager.Instance != null)
            {
                MapManager.Instance.RetreatToPreviousNode(); 
            }
            
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.Show("Askerleri toparlamak için geri çekildin.", NotificationType.Warning);
            }
        });

       
    }
    public void SetupKervansarayEvent()
    {
        titleText.text = "Güvenilir Tüccar Kervanı";
        if(merchant != null) eventImage.sprite = merchant; // İstersen kervan için yeni bir sprite ekleyebilirsin

        // O anki geçici çantadaki altını ve eşyaları çekelim
        int currentLootGold = 0;
        int currentLootItems = 0;
        
        if (ExpeditionManager.Instance != null)
        {
            currentLootGold = ExpeditionManager.Instance.tempGold;
            currentLootItems = ExpeditionManager.Instance.tempItems.Count;
        }

        descText.text = "Ağır silahlı muhafızlar tarafından korunan büyük bir ticaret kervanına denk geldin. Kervanbaşı sana seslendi:\n\n\"Uç Beyi! Kampa doğru gidiyoruz. Dilersen ganimetlerini %10 komisyon karşılığında senin adına güvenle otağına ulaştırabiliriz.\"\n\n";

        if (currentLootGold <= 0 && currentLootItems <= 0)
        {
            descText.text += "Ancak şu an sefer çantanda gönderecek hiçbir ganimetin yok.";
            
            CreateButton("Teşekkür Et ve Ayrıl", () => { ClosePanel(); });
        }
        else
        {
            int fee = Mathf.RoundToInt(currentLootGold * 0.1f);
            int safeGold = currentLootGold - fee;
            
           // descText.text += $"Çantandaki Altın: {currentLootGold}\nKesinti (%10): -{fee}\nKampa Ulaşacak: {safeGold}";

            CreateButton($"Ganimetleri Yolla (Komisyon: {fee} Akçe)", () => 
            {
                // Çantayı kampa postala
                ExpeditionManager.Instance.SendLootViaCaravan();
                
                foreach(Transform child in buttonContainer) Destroy(child.gameObject);
                
                descText.text = "GÜVENDESİN!\n\nKervan muhafızları ganimetlerini teslim aldı. Artık haritada başına ne gelirse gelsin, bu eşyalar ve altınlar kampındaki hazinede seni bekliyor olacak.\n\n<size=85%>(Hafifleyen çantanla yola daha cesur devam edebilirsin!)</size>";
                
                CreateButton("Yola Devam Et", () => { ClosePanel(); });
            });

            CreateButton("Vazgeç (Ganimetleri Kendin Taşı)", () => { ClosePanel(); });
        }
    }
    void SetupBattleEvent()
    {
        titleText.text = "Çapulcu Pusu";
        descText.text = "Yol kesen eşkıyalar! Savaşmak zor olacak ama ganimetleri iyi görünüyor.";
        if(battleSprite != null) eventImage.sprite = battleSprite;
        
        int readySoldiers = GetAvailableSoldierCount();
    
        if (readySoldiers == 0)
        {
            descText.text += "\n\nSAVAŞA HAZIR ASKER YOK!\nHerkes görevde, çalışıyor veya yaralı.";
        }
        else
        {
            descText.text += $"\n\nSavaşa Hazır Asker: {readySoldiers}";
        }

        GameObject atkBtnObj = Instantiate(buttonPrefab, buttonContainer);
        atkBtnObj.GetComponentInChildren<TextMeshProUGUI>().text = "Saldır";
        Button atkBtn = atkBtnObj.GetComponent<Button>();
        
        if (readySoldiers == 0)
        {
            atkBtn.interactable = false; 
        }
        else
        {
            atkBtn.onClick.AddListener(() => {
                if (TutorialManager.Instance != null && TutorialManager.Instance.currentStep == TutorialStep.Map_FirstBattlePanel)
                {
                    TutorialManager.Instance.AdvanceTutorial();
                }
                //AudioManager.Instance.PlayWarHorn();
                //DayManager.Instance.NextDay(3); 
                ClosePanel();
                BattleManager.Instance.StartBattle(2, 1, BattleEnvironment.Tower, GetCurrentTier()); 
            });
        }

        int mevcutNasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
        
        AddEscapeButton(
            escapeText:    "Kaçmayı Dene",
            escapeType:    EscapeType.Optional,
            nasipRequired: 3,
            onEscape:      () => { ResolveEscapeContest(BattleEnvironment.Tower, 2, 1); }
        );
    }

     void SetupKuleSavas()
    {
        titleText.text = "Kule Savunması";
        descText.text = "İlerideki dost gözetleme kulesi haydutların saldırısı altında! Onlara yardım edecek misin?";

         int readySoldiers = GetAvailableSoldierCount();
    
        if (readySoldiers == 0)
        {
            descText.text += "\n\nSAVAŞA HAZIR ASKER YOK!\nHerkes görevde, çalışıyor veya yaralı.";
        }
        else
        {
            descText.text += $"\n\nSavaşa Hazır Asker: {readySoldiers}";
        }
        

       GameObject atkBtnObj = Instantiate(buttonPrefab, buttonContainer);
        atkBtnObj.GetComponentInChildren<TextMeshProUGUI>().text = "Yardım et";
        Button atkBtn = atkBtnObj.GetComponent<Button>();
        
        if (readySoldiers == 0)
        {
            atkBtn.interactable = false; 
        }
        else
        {
            atkBtn.onClick.AddListener(() => {
                if (TutorialManager.Instance != null && TutorialManager.Instance.currentStep == TutorialStep.Map_FirstBattlePanel)
                {
                    TutorialManager.Instance.AdvanceTutorial(); 
                }
                //AudioManager.Instance.PlayWarHorn();
                //DayManager.Instance.NextDay(3); 
                ClosePanel();
                BattleManager.Instance.StartBattle(3, 1, BattleEnvironment.Tower, GetCurrentTier()); 
            });
        }

        CreateButton("Beni İlgilendirmez", () => {
            
            ReputationManager.Instance.ChangeReputation(-5); 
            NotificationManager.Instance.Show("Kule düştü, itibar kaybettin.", NotificationType.Error);
            ClosePanel();
        });
    }

   private void ResolveEscapeContest(BattleEnvironment failEnv, int failEnemyCount, int failDifficulty)
    {
        foreach(Transform child in buttonContainer) Destroy(child.gameObject);
        AudioManager.Instance.PlayDice();

        DiceManager.Instance.RollDice(6, (zarSonucu) => 
        {
            int mevcutNasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
            int totalScore = mevcutNasip + zarSonucu;
            int kacisBaraji = 6; 
            bool isWin = totalScore >= kacisBaraji;

            // RPG Matematik Yazısı
            string mathText = $"\n\n<size=85%><b>=Nasip ({mevcutNasip}) + Zar ({zarSonucu}) = {totalScore}</b> / Kaçış Barajı ({kacisBaraji})</size>\n";

            if (isWin)
            {
                descText.text = $"YOLUMUZ AÇIKMIŞ!\n\nNasibimiz yaver gitti, pusuyu fark edip sessizce etraflarından dolanmayı başardık. Kan dökülmeden izimizi kaybettirdik!\n" + mathText;
                
                CreateButton("Yola Devam Et", () => { ClosePanel(); });
            }
            else
            {
                descText.text = $"NASİP KAPALIYMIŞ!\n\nKaçmaya çalışırken kuru bir dala bastık... Bizi fark ettiler! Üstelik hazırlıksız yakalandığımız için moraller bozuldu(-10).\n" + mathText;
                
                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-10); 

                CreateButton("Kılıçları Çekin! (Savaş)", () => { 
                    //AudioManager.Instance.PlayWarHorn();
                    ClosePanel();
                    BattleManager.Instance.StartBattle(failEnemyCount, failDifficulty, failEnv, GetCurrentTier()); 
                });
            }
        });
    }

    void SetupRestEvent()
    {
        titleText.text = "Yörük Obası";
        descText.text = "Dost bir oba buldun. Burada dinlenip yaraları sarabiliriz.";
        
        CreateButton("Dinlen ve İyileş (1 Gün)", () => {
            
          
            NotificationManager.Instance.Show("Askerler dinlendi, moraller düzeldi.", NotificationType.Success);
            ClosePanel();
        });
    }
public void SetupDervishEvent()
    {
        titleText.text = "Yolcu Derviş";
        descText.text = "Tozlu yolların kenarında, üstü başı yamalı ama gözleri huzurla parlayan yaşlı bir derviş oturuyor. Önündeki boş ahşap kaseye bakarak mırıldanıyor:\n\n\"Dünya malı dünyada kalır Uç Beyim... Yolda kalmışa bir lokma, bir akçe sadakan var mıdır? Veren el, alan elden üstündür.\"";
        
        // if(dervishSprite != null) eventImage.sprite = dervishSprite;

        // SEÇENEK 1: Altın Ver (Büyük Sevap)
        CreateButton("Kesesini Doldur (-50 Akçe)", () => 
        {
            if (MoneyManager.Instance.Spend(50))
            {
                foreach(Transform child in buttonContainer) Destroy(child.gameObject);
                
                descText.text = "Dervişin yüzünde sıcacık bir tebessüm belirdi. Ellerini göğe açıp sana dualar etti.\n\n\"Allah kılıcını keskin, atını kavi, nasibini gür eylesin evlat...\"\n\n<size=85%>(Nasip Arttı! Askerlerin içi huzurla doldu.)</size>";
                
                if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(2); // Nasibi artır
                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(10); // Vicdani rahatlık
                
                CreateButton("Yola Devam Et", () => { ClosePanel(); });
            }
            else
            {
                NotificationManager.Instance.Show("Yeterli Akçen yok!", NotificationType.Error);
            }
        });


        CreateButton("Erzak Paylaş (-20 Erzak)", () => 
        {
            if (SupplyManager.Instance.currentFood >= 20)
            {
                SupplyManager.Instance.SpendFood(20);
                foreach(Transform child in buttonContainer) Destroy(child.gameObject);
                
                descText.text = "Derviş uzattığın ekmeği ve suyu minnetle kabul etti.\n\n\"Sofranız bereketli olsun Beyim...\"\n\n<size=85%>(Nasip Arttı!)</size>";
                
                if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(1); 
                
                CreateButton("Yola Devam Et", () => { ClosePanel(); });
            }
            else
            {
                NotificationManager.Instance.Show("Yeterli Erzağın yok!", NotificationType.Error);
            }
        });

  
        CreateButton("Sırt Çevir (Geçip Git)", () => 
        {
            foreach(Transform child in buttonContainer) Destroy(child.gameObject);
            
            descText.text = "Askerlerine yürüyüş kararı verdin. Derviş arkandan sessizce bakakaldı. Havanın aniden soğuduğunu ve rüzgarın tersine esmeye başladığını hissettin...\n\n<size=85%>(Nasip Kapandı... Ordunun morali bozuldu.)</size>";
            
            if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(1); 
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5); 
            
            CreateButton("Yola Devam Et", () => { ClosePanel(); });
        });
    }

    public void SetupCaravanEvent()
    {
        titleText.text = "Savunmasız Kervan";
        descText.text = "Orman yolunda ilerlerken çamura saplanmış, tekerleği kırılmış zengin bir tüccar kervanı gördün. Muhafızları kaçmış, tüccar ise çaresizce mallarını kurtarmaya çalışıyor. Arabanın kasası altın ve erzakla dolup taşıyor.\n\n\"Uç Beyim! Yetiş imdadıma! Kurtların ve eşkıyaların insafına kaldım!\"";
        

        CreateButton("Yardım Eli Uzat (Askerleri Yor)", () => 
        {
            foreach(Transform child in buttonContainer) Destroy(child.gameObject);
            
            descText.text = "Askerlerine emir verdin, omuz omuza verip arabayı çamurdan çıkardınız. Tüccar minnetle ellerine sarıldı ve sana helalinden bir miktar ödül verdi.\n\n\"Hızır gibi yetiştiniz Beyim! Bu kadarı ancak helaldir!\"\n\n<size=85%>(Nasip Arttı! +100 Akçe, +10 Odun, Ancak askerler yoruldu.)</size>";
            
            if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(1);
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5); // Yorulma cezası
            MoneyManager.Instance.Add(100);
            ResourceManager.Instance.AddWood(10);
            
            CreateButton("Yola Devam Et", () => { ClosePanel(); });
        });

        CreateButton("Mallara El Koy! (Yağma)", () => 
        {
            foreach(Transform child in buttonContainer) Destroy(child.gameObject);
            
            descText.text = "ZULÜM!\n\nKılıçları çektiniz. Tüccar ağlayarak ormana kaçtı. Arabadaki tüm zenginliğe çöktünüz. Hazinen dolup taştı ama askerlerinin yüzündeki o onurlu ifade yerini hırsızlık utancına bıraktı. Gökyüzü karardı...\n\n<size=85%>(Nasip Ciddi Şekilde Düştü! Büyük Kul Hakkı... Ordunun Şerefi Zedelendi.)</size>";
            
            if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(3);
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-20);
            
            // Devasa ama haram ödül
            MoneyManager.Instance.Add(600);
            SupplyManager.Instance.AddFood(100);
            
            CreateButton("Vicdan Azabıyla Devam Et", () => { ClosePanel(); });
        });

        CreateButton("Bizi İlgilendirmez (Geç)", () => 
        {
            foreach(Transform child in buttonContainer) Destroy(child.gameObject);
            
            descText.text = "Tüccarın feryatlarına kulak tıkayıp yoluna devam ettin. Arkada onu kurtlara ve eşkıyalara yem olarak bıraktın.\n\n<size=85%>(Zaman kaybetmediniz ama vicdanlar sızladı.)</size>";
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5); 
            if (ReputationManager.Instance != null)ReputationManager.Instance.ChangeReputation(-5);
            CreateButton("Yola Devam Et", () => { ClosePanel(); });
        });
    }
    void SetupBossEvent()
    {
        titleText.text = "KIZIL KALE";
        descText.text = "Sonunda hedefe ulaştın. Kale surları göğe yükseliyor. Bu son savaş olacak.";
        if(bossSprite != null) eventImage.sprite = bossSprite;

        CreateButton("KUŞATMAYI BAŞLAT", () => {
            
            if (!HasAliveSoldiers())
            {   
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show("Savaşa sokacak hiç askerin yok! Kampa dönmelisin.", NotificationType.Error);
                return; 
            }
           // if (AudioManager.Instance != null) AudioManager.Instance.PlayWarHorn();
            BattleManager.Instance.StartBossBattle(5, BattleEnvironment.Tower); 
            ClosePanel();
        });
    }

    void SetupArcheryEvent()
    {
        titleText.text = "Okçuluk Müsabakası";
        descText.text  = "Bir Türkmen beyi senin yiğitliğini ölçmek için okçuluk müsabakasına davet etti.\n\nHangi yiğidi göndereceksin? (Hızı yüksek asker rüzgardan daha az etkilenir!)";
        if (bossSprite != null) eventImage.sprite = archerySprite;

        // Asker seçim butonları
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        bool hasValidSoldier = false;

        foreach (var soldier in allSoldiers)
        {
            if (!soldier.CompareTag("MySoldier") || soldier.data == null
                || soldier.data.currentHealth <= 0 || soldier.isOnMission
                || soldier.data.currentActivity == SoldierActivity.Working) continue;

            hasValidSoldier = true;
            string btnText  = $"{soldier.data.gladiatorName} (Hız: {soldier.data.speed})";

            CreateButton(btnText, () =>
            {
                // Askerin hız statını PlayerPrefs'e yaz — ArcheryGameManager okuyacak
                PlayerPrefs.SetInt("ArcherySoldierSpeed", soldier.data.speed);
                PlayerPrefs.Save();
                GoToArcheryScene();
                ClosePanel();
            });
        }

        if (!hasValidSoldier)
            descText.text += "\n\nGönderecek boşta askerin yok!";

        CreateButton("Teklifi Reddet (-5 İtibar)", () =>
        {
            ReputationManager.Instance.ChangeReputation(-5);
            NotificationManager.Instance.Show("Bey bu teklifi reddetmene kırıldı.", NotificationType.Warning);
            ClosePanel();
        });
    }


   void SetupAtYarisi()
    {
        titleText.text = "Büyük At Yarışı";
        if(horseSprite != null) eventImage.sprite = horseSprite;
        descText.text = $"Şehrin ileri gelenlerinden Mustafa Bey seni at yarışına davet etti. (Giriş: 30 Akçe)\n\n" +
                        $"Rakip: {atYarisiRakipIsim} (Hızı: {atYarisiRakipHiz})\n\n" +
                        $"Hangi yiğidi göndereceksin?";

        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        bool hasValidSoldier = false;

        foreach (var soldier in allSoldiers)
        {
            if (soldier.CompareTag("MySoldier") && soldier.data != null && soldier.data.currentHealth > 0 && !soldier.isOnMission && soldier.data.currentActivity != SoldierActivity.Working)
            {
                hasValidSoldier = true;
                int askerHiz = soldier.data.speed;
                string btnText = $"{soldier.data.gladiatorName} (Hız: {askerHiz})";

                CreateButton(btnText, () => {
                    if (MoneyManager.Instance.gold >= 30)
                    {
                        MoneyManager.Instance.Spend(30);
                        ClosePanel();
                        HorseRidingMiniGame.Instance.StartHorseRiding(soldier, (isWin) => 
                        {
                            eventPanel.SetActive(true);
                            foreach(Transform child in buttonContainer) Destroy(child.gameObject);

                            if (isWin) {
                                descText.text = $"ZAFER!\n\n{soldier.data.gladiatorName} rüzgar gibi esti! Yarışı birinci bitirdi.";
                                MoneyManager.Instance.Add(150); 
                                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(15);
                                AudioManager.Instance.PlayCheer(); 
                            } else {
                                descText.text = $"KAYBETTİN!\n\n{soldier.data.gladiatorName}'ın atı tökezledi, yarışı gerilerde tamamladı.";
                                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-10);
                            }
                            CreateButton("Devam Et", () => { ClosePanel(); });
                        });
                    }
                    else NotificationManager.Instance.Show("Yarışa katılmak için 30 Akçen yok!", NotificationType.Error);
                });
            }
        }

        // SOFT-LOCK KORUMASI: Sadece yetersiz durumdaysa çıkış ver, aksi takdirde kaçış yok.
        if (!hasValidSoldier || MoneyManager.Instance.gold < 30)
        {
            descText.text += "\n\nYarışa katılacak akçen veya askerin yok. Ahali sana gülerek dağıldı.";
            CreateButton("Meydandan Boynu Bükük Ayrıl", () => { ClosePanel(); });
        }
    }

    void SetupYagliGures()
    {
        titleText.text = "Yağlı Güreş!";
        if(wrestlingSprite != null) eventImage.sprite = wrestlingSprite;
        descText.text = $"Meydanda davullar çalıyor. Başpehlivanlık için er meydanına bir yiğidini sal. (Giriş: 50 Akçe)\n\n" +
                        $"Rakip Pehlivan: {guresRakipIsim} (Gücü: {guresRakipGuc})\n\n" +
                        $"Kimi yollayacaksın?";
        
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        bool hasValidSoldier = false;

        foreach (var soldier in allSoldiers)
        {
            if (soldier.CompareTag("MySoldier") && soldier.data != null && soldier.data.currentHealth > 0 && !soldier.isOnMission && soldier.data.currentActivity != SoldierActivity.Working)
            {
                hasValidSoldier = true;
                int askerGuc = soldier.data.strength;
                string btnText = $"{soldier.data.gladiatorName} (Güç: {askerGuc})";
                
                CreateButton(btnText, () => {
                    if (MoneyManager.Instance.gold >= 50)
                    {
                        MoneyManager.Instance.Spend(50);
                        ClosePanel();
                        WrestlingMiniGame.Instance.StartWrestling(soldier, guresRakipGuc, (isWin) => 
                        {
                            eventPanel.SetActive(true);
                            foreach(Transform child in buttonContainer) Destroy(child.gameObject);

                            if (isWin) {
                                descText.text = $"ZAFER!\n\n{soldier.data.gladiatorName} sırtı yere gelmeden rakibi tuş etti!";
                                MoneyManager.Instance.Add(250); 
                                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(15);
                                AudioManager.Instance.PlayCheer(); 
                            } else {
                                descText.text = $"KAYBETTİN!\n\n{soldier.data.gladiatorName} elinden geleni yaptı ama dayanamadı.";
                                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-10);
                            }
                            CreateButton("Devam Et", () => { ClosePanel(); });
                        });
                    }
                    else NotificationManager.Instance.Show("Güreşe katılmak için 50 Akçen yok!", NotificationType.Error);
                });
            }
        }

        // SOFT-LOCK KORUMASI
        if (!hasValidSoldier || MoneyManager.Instance.gold < 50)
        {
            descText.text += "\n\nMeydana çıkacak gücün veya akçen yok.";
            CreateButton("Utançla Ayrıl", () => { ClosePanel(); });
        }
    }

  

   
   private void ResolveContest(Gladiator selectedSoldier, NodeType eventType, int askerStat, int rakipStat, string rakipAd)
    {
        foreach(Transform child in buttonContainer) Destroy(child.gameObject);
        AudioManager.Instance.PlayDice();

        DiceManager.Instance.RollDice(6, (zarSonucu) => 
        {
            int totalScore = askerStat + zarSonucu;
            bool isWin = totalScore >= rakipStat;

            string statAd = eventType == NodeType.Atyarisi ? "Hızı" : "Gücü";

            // RPG Matematik Yazısı
            string mathText = $"\n\n<size=85%><b>=Askerin {statAd} ({askerStat}) + Zar ({zarSonucu}) = {totalScore}</b> / Hakem Puanı ({rakipStat})</size>\n";

            if (isWin)
            {
                string flavorText = eventType == NodeType.Atyarisi
                    ? $"{selectedSoldier.data.gladiatorName} rüzgar gibi esti! Yarışı birinci bitirdi."
                    : $"{selectedSoldier.data.gladiatorName} sırtı yere gelmeden rakibi {rakipAd}'i tuş etti!";

                descText.text = $"ZAFER!\n\nYiğidimiz beklentileri aştı ve müsabakayı kazandı!\n\n" + flavorText + mathText;

                int reward = eventType == NodeType.Atyarisi ? 150 : 250;
                MoneyManager.Instance.Add(reward); 
                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(15);
                AudioManager.Instance.PlayCheer(); 
            }
            else
            {
                string flavorText = eventType == NodeType.Atyarisi
                    ? $"{selectedSoldier.data.gladiatorName}'ın atı tökezledi, yarışı gerilerde tamamladı. Ahali bize güldü."
                    : $"{selectedSoldier.data.gladiatorName} elinden geleni yaptı ama rakibine dayanamadı. Meydandan boynu bükük ayrıldık.";

                descText.text = $"KAYBETTİN!\n\nMaalesef yiğidimiz müsabakayı kaybetti.\n\n" + flavorText + mathText;
                
                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-10);
            }

            CreateButton("Devam Et", () => { ClosePanel(); });
        });
    }

    /// <summary>
    /// Mevcut encounter sayısına göre tier döner.
    /// Tier 1: 0-4, Tier 2: 5-9, Tier 3: 10+
    /// </summary>
    private int GetCurrentTier()
    {
        if (ExpeditionManager.Instance == null) return 1;
        int count = ExpeditionManager.Instance.currentEncounterCount;
        if (count <= 4)  return 1;
        if (count <= 9)  return 2;
        return 3;
    }

    public int GetAvailableSoldierCount()
    {
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        int availableCount = 0;

        foreach (var glad in allSoldiers)
        {
            GladiatorAI ai = glad.GetComponent<GladiatorAI>();
            if (glad.CompareTag("MySoldier") && (ai == null || !ai.isDead))
            {
                bool isWorking = (glad.data != null && glad.data.currentActivity == SoldierActivity.Working);
                bool isOnMission = glad.isOnMission;

                if (!isOnMission && !isWorking)
                {
                    availableCount++;
                }
            }
        }
        return availableCount;
    }

    private bool HasAliveSoldiers()
    {
        var allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        foreach (var soldier in allSoldiers)
        {
            if (soldier.data != null && soldier.data.currentHealth > 0)
            {
                return GetAvailableSoldierCount() > 0;
            }
        }
        return false; 
    }

    public void CreateButton(string text, UnityEngine.Events.UnityAction action)
    {
        
        GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = text;
        btnObj.GetComponent<Button>().onClick.AddListener(action);
        AdjustButtonContainerWidth();
    }

    public void ClosePanel()
    {
        eventPanel.SetActive(false);
    }

    private void GoToArcheryScene()
    {
        SceneManager.LoadScene("ArcheryMiniGame");
    }

    // ══════════════════════════════════════════════════════════════════════
    // CENK ETKİNLİĞİ
    // ══════════════════════════════════════════════════════════════════════
    void SetupCenkEvent()
    {
        titleText.text = "Cenk Daveti";
        if (merchant != null) eventImage.sprite = merchant;

        // Rakip gücü tier'a göre değişir
        int tier        = GetCurrentTier();
        int opponentPts = tier == 1 ? 20 : tier == 2 ? 35 : 50;

        descText.text =
            $"Yolun üzerinde bir han var. İçeride birkaç er seni Cenk masasına çağırıyor.\n\n" +
            $"Rakip Gücü: ~{opponentPts} puan\n\n" +
            $"Kazanırsan itibar kazanırsın. Kaybedersen moral düşer.";

        CreateButton("Masaya Otur", () =>
        {
            if (CenkGameManager.Instance == null) { ClosePanel(); return; }

            ClosePanel(); // Parşömen ekranını kapat
            CenkGameManager.Instance.OnCenkFinished = (won) =>
            {
                if (won)
                {
                    int rep = 10 + (tier - 1) * 5;
                    ReputationManager.Instance?.ChangeReputation(rep);
                    NotificationManager.Instance?.Show(
                        $"Cenk'i kazandın! +{rep} İtibar", NotificationType.Success);
                }
                else
                {
                    CampMoraleManager.Instance?.ChangeMorale(-10);
                    NotificationManager.Instance?.Show(
                        "Cenk'i kaybettin. -10 Moral", NotificationType.Warning);
                }
            };
            CenkGameManager.Instance.OpenMinigame();
        });

        // Kaçış — nasibe bağlı
        AddEscapeButton(
            escapeText:    "Aldırma, devam et",
            escapeType:    EscapeType.Optional,
            nasipRequired: 2,
            onEscape:      () => { ClosePanel(); }
        );
    }
   
     void SetupNightRaidEvent()
    {
        titleText.text = "Gece Baskını!";
        if (battleSprite != null) eventImage.sprite = battleSprite;
 
        int tier = GetCurrentTier();
        descText.text =
            "Gecenin karanlığında kamp ateşinin etrafında toplanmışken uzaktan çığlıklar duyuldu.\n\n" +
            "Düşman kamp'a baskın düzenliyor! 3 dalgayı savuşturmak zorundasın.\n\n" +
            $"Tahmini tehdit: Tier {tier} — {3 + tier} düşman / dalga";
 
        CreateButton("Savunmaya Geç!", () =>
        {
            // Asker seçimini kaydet
            PlayerPrefs.SetInt("NightRaidTier", tier);
            PlayerPrefs.Save();
            ClosePanel();
            UnityEngine.SceneManagement.SceneManager.LoadScene("VillageDefenseScene");
        });
 
        // Gece baskınından kaçmak çok pahalı
        AddEscapeButton(
            escapeText:        "Karanlığa Çekil",
            escapeType:        EscapeType.Costly,
            reputationPenalty: 20,
            onEscape:          () => { ClosePanel(); }
        );
    }
 void SetupAsikEvent()
    {
        AsikAtismasi.RivalType rival = AsikAtismasi.PickRivalType();

        string rivalName = rival switch
        {
            AsikAtismasi.RivalType.GezginAsik => "Gezgin Aşık Karaçalı",
            AsikAtismasi.RivalType.PirSultan  => "Pir Sultan",
            _                                  => "Köy Ozanı Veli"
        };

        titleText.text = "Aşık Atışması!";
        descText.text  =
            $"Yolun ortasında sazını takan {rivalName} karşınıza çıktı.\n\n" +
            "Sözü kuvvetli bir asker bu atışmayı kazanabilir. " +
            "Hızı yüksek asker daha çok düşünme süresi alır.\n\n" +
            "<size=85%>Hangi yiğidi göndereceksin?</size>";

        bool hasValidSoldier = false;
        foreach (var soldier in FindObjectsByType<Gladiator>(FindObjectsSortMode.None))
        {
            if (!soldier.CompareTag("MySoldier") || soldier.data == null ||
                soldier.data.currentHealth <= 0  || soldier.isOnMission ||
                soldier.data.currentActivity == SoldierActivity.Working) continue;

            hasValidSoldier = true;
            Gladiator captured     = soldier;
            AsikAtismasi.RivalType capturedRival = rival;

            string traitNote = soldier.data.trait switch
            {
                SoldierTrait.Dindar    => " <size=75%>[Dini mısralarda 1 şık elinir]</size>",
                SoldierTrait.Yetenekli => " <size=75%>[Her doğru = +1sn]</size>",
                _                      => ""
            };

            CreateButton(
                $"{soldier.data.gladiatorName}  Hız: {soldier.data.speed}{traitNote}",
                () =>
                {
                    if (AsikAtismasi.Instance != null)
                        AsikAtismasi.Instance.StartAtisma(captured, capturedRival);
                    else
                        Debug.LogError("[MapEventManager] AsikAtismasi sahnede bulunamadı!");
                });
        }

        if (!hasValidSoldier)
            descText.text += "\n\nGönderecek boşta askerin yok!";

        CreateButton("Adamı Savuştur (-5 İtibar)", () =>
        {
            AddReward(0, -5);
            NotificationManager.Instance?.Show("Ozandan kaçtın. Biraz itibar kaybettin.", NotificationType.Warning);
            ClosePanel();
        });
    }
    // ══════════════════════════════════════════════════════════════════════
    // MERKEZİ KAÇIŞ SİSTEMİ
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Kaçış türü — her event ne kadar "zorunlu" olduğunu belirtir.
    /// </summary>
    public enum EscapeType
    {
        /// <summary>Her zaman kaçılabilir, ceza yok.</summary>
        Free,
        /// <summary>Nasip yeterince yüksekse kaçılabilir.</summary>
        Optional,
        /// <summary>Kaçmak itibar cezası verir.</summary>
        Costly,
        /// <summary>Kaçmak mümkün değil — event zorunlu.</summary>
        Mandatory
    }

    /// <summary>
    /// Event paneline kaçış butonu ekler.
    /// Nasip sistemiyle entegre — yeterli nasip yoksa buton grileşir.
    /// </summary>
    private void AddEscapeButton(
        string     escapeText,
        EscapeType escapeType,
        int        nasipRequired = 0,
        int        reputationPenalty = 0,
        System.Action onEscape = null)
    {
        if (escapeType == EscapeType.Mandatory) return; // Kaçış yok

        int currentNasip = NasipManager.Instance != null
            ? NasipManager.Instance.currentNasip : 0;

        switch (escapeType)
        {
            case EscapeType.Free:
                CreateButton(escapeText, () =>
                {
                    onEscape?.Invoke();
                });
                break;

            case EscapeType.Optional:
                bool canEscape = currentNasip >= nasipRequired;
                string optText = canEscape
                    ? $"{escapeText} (Nasip: {currentNasip}✓)"
                    : $"{escapeText} (Nasip Yeterli Değil: {currentNasip}/{nasipRequired})";

                CreateButton(optText, () =>
                {
                    if (!canEscape)
                    {
                        NotificationManager.Instance?.Show(
                            $"Nasibin bu zorluktan kaçmak için yeterli değil! ({currentNasip}/{nasipRequired})",
                            NotificationType.Warning);
                        return; // Paneli kapatma, event devam eder
                    }
                    NasipManager.Instance?.SpendNasip(1); // Kaçmak 1 nasip harcar
                    onEscape?.Invoke();
                });
                break;

            case EscapeType.Costly:
                int penalty = reputationPenalty > 0 ? reputationPenalty : 10;
                CreateButton($"{escapeText} (-{penalty} İtibar)", () =>
                {
                    ReputationManager.Instance?.ChangeReputation(-penalty);
                    NotificationManager.Instance?.Show(
                        $"Kaçtın ama itibar zedelendi. (-{penalty} İtibar)",
                        NotificationType.Warning);
                    onEscape?.Invoke();
                });
                break;
        }
    }
}