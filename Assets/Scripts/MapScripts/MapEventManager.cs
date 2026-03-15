using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MapEventManager : MonoBehaviour
{
    public static MapEventManager Instance;

    [Header("UI Elemanları")]
    public GameObject eventPanel;       
    public GameObject topPanel; 
    public Image eventImage;            
    public TextMeshProUGUI titleText;   
    public TextMeshProUGUI descText;    
    
    public Transform buttonContainer;   
    public GameObject buttonPrefab;     

    [Header("Olay Görselleri")]
    public Sprite villageSprite;  
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
  
    public Sprite diceSprite; 
    public Sprite wrestlingSprite; 
  

    // --- YENİ: RAKİP AYARLARI (INSPECTOR'DAN DEĞİŞTİREBİLİRSİN) ---
    [Header("Mini Oyun Rakipleri")]
    public string atYarisiRakipIsim = "Mustafa Bey'in Atı";
    public int atYarisiRakipHiz = 12; // Rakibin baraj puanı

    public string guresRakipIsim = "Gaddar Rüstem";
    public int guresRakipGuc = 15;    // Rakibin baraj puanı
    // --------------------------------------------------------------

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
            child.SetParent(null);     // Listeden kopar (Genişlik hesabı bozulmasın diye)
            Destroy(child.gameObject); // Yok et
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
        }
    }

    // --- DİĞER EVENTLERİN BURADA DURUYOR (Vahsi, Dice, Zindan, Kalkan vb.) ---
    // --- YENİ: KUTUYU İÇİNDEKİ BUTON SAYISINA GÖRE ESNETİR/DARALTIR ---
    private void AdjustButtonContainerWidth()
    {
        int count = buttonContainer.childCount;
        float targetWidth = 454f; // 1 veya 2 buton için standart genişlik
        
        // Eğer 2'den fazla buton varsa, fazladan her buton için 394 ekle
        if (count > 2)
        {
            targetWidth += (count - 2) * 244f; 
        }

        // RectTransform'u yakala ve yeni genişliği (width) zorla uygula
        RectTransform rt = buttonContainer.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(targetWidth, rt.sizeDelta.y);
    }
    void SetupVahsi()
    {
        titleText.text = "Aç Kurt Sürüsü";
        descText.text = "Ormandan geçerken etrafını aç bir kurt sürüsü sardı! Savaşmak yorucu olacak ama kaçmak da vakit kaybettirir.";

        CreateButton("Savaş (-1 Gün, +20 Altın)", () => {
           
            DayManager.Instance.NextDay(1);
           int currentPending = PlayerPrefs.GetInt("PendingGold", 0);
            PlayerPrefs.SetInt("PendingGold", currentPending + 20);
            PlayerPrefs.Save();
            NotificationManager.Instance.Show("Kurtları alt ettin ve postlarını sattın.", NotificationType.Success);
            ClosePanel();
        });

        CreateButton("Etrafından Dolaşarak Kaç (-2 Gün)", () => {
            
            DayManager.Instance.NextDay(2);
            NotificationManager.Instance.Show("Güvenli ama uzun yolu seçtin.", NotificationType.Info);
            ClosePanel();
        });
    }
    void SetupNasibEvent()
    {
        titleText.text = "Yoldaki Derviş";
        descText.text = "Üstü başı yırtık, yaşlı bir derviş yol kenarında bir ağacın dibinde oturuyor. Sana doğru bakıp titreyen bir sesle mırıldandı:\n\n<color=#4E2A03>\"Yolcu... Allah rızası için bir lokma ekmek, üç beş akçe sadaka verir misin?\"</color>";
        
        // Eğer elinde derviş veya fakir bir adam resmi yoksa village/merchant sprite'ını kullanabilirsin
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
            
            // Nasip düşer, moral bozulur
            if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(1);
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5);
            
            NotificationManager.Instance.Show("Derviş arkandan sessizce baktı. İçini büyük bir sıkıntı bastı.", NotificationType.Warning);
            ClosePanel();
        });
    }
 public void SetupMysteriousDiceEvent()
    {
        titleText.text = "Gizemli Yabancı";
        descText.text = "Karanlık bir pelerin giymiş bir adam yolunu kesti. Avucundan kemikten yapılma bir zar çıkardı.\n\n<color=#66001D>\"Şu zarı at bakalım Uç Beyi... Eğer 3'ten büyük atarsan sana tam 1000 Akçe vereceğim. Ama kaybedersen, en değerli şeylerinden birini... ordundan bir canı alırım.\"</color>";
        
        if(diceSprite != null) eventImage.sprite = diceSprite;

        CreateButton("Zarı At (Ölümcül Risk)", () => 
        {
            AudioManager.Instance.PlayDice();
            
            DiceManager.Instance.RollDice(6, (zarSonucu) => 
            {
                foreach(Transform child in buttonContainer) Destroy(child.gameObject);

                // Nasip'i çekip matematiği kuruyoruz
                int mevcutNasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
                int totalScore = zarSonucu + mevcutNasip;
                int hedef = 4; // 3'ten büyük atması lazımdı (4, 5, 6 vs)

                // RPG Matematik Yazısı (Baldur's Gate tarzı)
                string mathText = $"\n\n<size=85%><b><color=#FFD700>Zar ({zarSonucu}) + Nasip ({mevcutNasip}) = {totalScore}</color></b> / Hedef ({hedef})</size>\n";

                if (totalScore >= hedef) 
                {
                    descText.text = $"<color=green>KAZANDIN!</color>\n\nAdam şaşkınlıkla sana baktı. Elindeki devasa keseyi ayaklarının dibine fırlattı ve tek kelime etmeden karanlıkta kayboldu." + mathText;
                    
                    MoneyManager.Instance.Add(1000); 
                    if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(15);
                    AudioManager.Instance.PlayCheer(); 
                    
                    CreateButton("Devam Et", () => { ClosePanel(); });
                }
                else
                {
                    descText.text = $"<color=red>KAYBETTİN!</color>\n\nAdam korkunç bir kahkaha attı. <color=#66001D>\"Kaybettin Uç Beyi... Söz verdiğin gibi, şimdi bana bir can ver!\"</color>\n\n<color=yellow>(Kimi feda edeceksin?)</color>" + mathText;
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
 // Kaybedildiğinde feda edilecek askeri seçtiren ekran
    private void ShowSacrificeSelection()
    {
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        bool hasEligibleSoldier = false;

        foreach (var soldier in allSoldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            
            // Asker hayattaysa ve bizim askerimizse
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead) && soldier.data != null && soldier.data.currentHealth > 0)
            {
                // --- BARRİYER: KEŞİF GÖREVİNDE OLANLAR SEÇİLEMEZ! ---
                if (soldier.data.currentActivity == SoldierActivity.OnMission) continue;

                hasEligibleSoldier = true;
                
                // Askerin ismini ve canını butona yazalım
                string btnText = $"{soldier.data.gladiatorName} Feda Et";
                
                // Yakalanan 'soldier' referansını butonun içine hapsediyoruz
                Gladiator doomedSoldier = soldier; 
                CreateButton(btnText, () => {
                    ExecuteSacrifice(doomedSoldier);
                });
            }
        }

        // Eğer kampta feda edilecek kimse yoksa (herkes seferdeyse veya asker kalmadıysa)
        if (!hasEligibleSoldier)
        {
            descText.text += "\n\n<color=#66001D>\"Alacak bir can bulamıyorum... Ordun zaten tükenmiş!\"</color> diyerek alay etti ve hazinenden 500 Akçe çalıp kayboldu.";
            
            if (MoneyManager.Instance.gold >= 500) MoneyManager.Instance.Spend(500);
            else if (MoneyManager.Instance.gold > 0) MoneyManager.Instance.Spend(MoneyManager.Instance.gold);

            CreateButton("Devam Et", () => { ClosePanel(); });
        }
    }

    // Seçilen askerin kalemini kıran fonksiyon
    private void ExecuteSacrifice(Gladiator doomedSoldier)
    {
        // Butonları temizle
        foreach(Transform child in buttonContainer) Destroy(child.gameObject);

        string doomedName = doomedSoldier.data.gladiatorName;

        // Askeri öldür (Savaşta ölmekle aynı işlemleri tetikler, kampı vs. bozulmaz)
        doomedSoldier.data.currentHealth = 0;
        GladiatorAI ai = doomedSoldier.GetComponent<GladiatorAI>();
        
        if (ai != null) ai.Die();
        else Destroy(doomedSoldier.gameObject);

        // Ölüm metni ve moral düşüşü
        descText.text = $"Adam kara pelerininin içinden elini uzattı. <color=yellow>{doomedName}</color> acı dolu bir çığlık atarak oracıkta cansız yere yığıldı. Gözleri kararırken kampa ağır bir sessizlik çöktü...";
        
        if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-25);

        CreateButton("Devam Et", () => { ClosePanel(); });
    }
    void SetupZindan()
    {
        titleText.text = "Karanlık Mağara";
        descText.text = "İçeriden garip sesler geliyor. Büyük bir tehlike ama aynı zamanda büyük bir ganimet yatıyor olabilir.";
          if(villageSprite != null) eventImage.sprite = dungeonSprite;
        CreateButton("İçeri Gir (Savaş)", () => {
            
            DayManager.Instance.NextDay(1);
            ClosePanel();
            NotificationManager.Instance.Show("Zindana girdin!", NotificationType.Warning);
        });

        CreateButton("Tehlikeye Atılma (Yola Devam Et)", () => {
            
            ClosePanel();
        });
    }

    void SetupKalkan()
    {
        titleText.text = "Terk Edilmiş Karakol";
        descText.text = "Eski ve terk edilmiş bir gözetleme kulesi buldun. Askerlerin geride bıraktığı sağlam kalkanlar ve silahlar var.";
        
        CreateButton("Ganimetleri Topla (+30 Altın)", () => {
            
            DayManager.Instance.NextDay(1);
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
        titleText.text = "Ormanda Bir Tüccar";
        descText.text = "Tekerleği kırılmış bir tüccar arabası buldun. Sana bir teklifi var: 'Bana 50 Akçe ver, devlet ricaliyle aranı yapayım (İtibar).'";
        if(merchant != null) eventImage.sprite = merchant;

        CreateButton("Kabul Et (-50 Altın, +15 İtibar)", () => {
            
            DayManager.Instance.NextDay(1);
            if (MoneyManager.Instance.gold >= 50)
            {
                MoneyManager.Instance.Spend(50);
                ReputationManager.Instance.ChangeReputation(15);
                NotificationManager.Instance.Show("Takas yapıldı, itibarın arttı!", NotificationType.Success);
            }
            else
            {
                NotificationManager.Instance.Show("Yeterli altının yok!", NotificationType.Error);
            }
            ClosePanel();
        });

        CreateButton("Yola Devam Et", () => {
            
            ClosePanel(); 
        });
    }

    void SetupTreasure()
    {
        titleText.text = "Gizli Hazine";
        descText.text = "Burada büyük bir hazine yatıyor ancak biraz kürek sallamanız gerekecek. 1 Gününü alacak, sence zahmetine değecek mi?";

        CreateButton("Kazmaya Başla (-1 Gün, +100 Altın)", () => {
            
            DayManager.Instance.NextDay(1); 
            int currentPending = PlayerPrefs.GetInt("PendingGold", 0);
            PlayerPrefs.SetInt("PendingGold", currentPending + 100);
            PlayerPrefs.Save();
            NotificationManager.Instance.Show("Büyük bir define buldun!", NotificationType.Success);
            ClosePanel();
        });

        CreateButton("Vaktim Yok, İlerle", () => {
            
            ClosePanel();
        });
    }

   

    void SetupVillageEvent()
    {
        titleText.text = "Türkmen Köyü";
        descText.text = "Köy halkı zor durumda. Onlara yardım edersen dualarını alırsın.";
        if(villageSprite != null) eventImage.sprite = villageSprite;

        CreateButton("Köy Halkına Yardım Et (+10 İtibar)", () => {
            
            if (ReputationManager.Instance != null)
                ReputationManager.Instance.ChangeReputation(10);
            
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Halk sana minnettar!", NotificationType.Success);
            
            ClosePanel();
        });

        CreateButton("Yola Devam Et", () => {
            
            ClosePanel();
        });
    }

    void SetupStartEvent()
    {
        titleText.text = "Kamp";
        descText.text = "Kampda dinlen ve hazırlıklarını yap.";
        if(villageSprite != null) eventImage.sprite = villageSprite;
        
        CreateButton("Kampa Geç", () => {
            if (TutorialManager.Instance != null && TutorialManager.Instance.currentStep == TutorialStep.Intro_CampPanel)
            {
                TutorialManager.Instance.AdvanceTutorial(); // Camp_Tour'a geçirir
            }
            ClosePanel();
            topPanel.SetActive(true);
            BattleManager.Instance.ReturnToCamp();
        });
    }

   void SetupFirstBattleEvent()
    {
        titleText.text = "Çapulcu Pusu";
        descText.text = "Yol kesen eşkıyalar! Savaşmak zor olacak ama ganimetleri iyi görünüyor.";
        if(battleSprite != null) eventImage.sprite = battleSprite;
        
        int readySoldiers = GetAvailableSoldierCount();
    
        if (readySoldiers == 0)
        {
            descText.text += "\n\n<color=red>SAVAŞA HAZIR ASKER YOK!</color>\nHerkes görevde, çalışıyor veya yaralı.";
        }
        else
        {
            descText.text += $"\n\nSavaşa Hazır Asker: <color=green>{readySoldiers}</color>";
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
                    TutorialManager.Instance.AdvanceTutorial(); // Battle_ScriptedLoss'a geçirir
                }
                AudioManager.Instance.PlayWarHorn();
                //DayManager.Instance.NextDay(3); 
                ClosePanel();
                BattleManager.Instance.StartBattle(2, 1, BattleEnvironment.Forest); 
            });
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
            descText.text += "\n\n<color=red>SAVAŞA HAZIR ASKER YOK!</color>\nHerkes görevde, çalışıyor veya yaralı.";
        }
        else
        {
            descText.text += $"\n\nSavaşa Hazır Asker: <color=green>{readySoldiers}</color>";
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
                    TutorialManager.Instance.AdvanceTutorial(); // Battle_ScriptedLoss'a geçirir
                }
                AudioManager.Instance.PlayWarHorn();
                //DayManager.Instance.NextDay(3); 
                ClosePanel();
                BattleManager.Instance.StartBattle(2, 1, BattleEnvironment.Tower); 
            });
        }

        int mevcutNasip = NasipManager.Instance != null ? NasipManager.Instance.currentNasip : 0;
        
        CreateButton($"Kaçmayı Dene (Nasip: {mevcutNasip})", () => {
            // Kaçmayı denerken zar atacağız, eğer kaybedersek savaş ortamı Tower olacak şekilde gönderiyoruz
            ResolveEscapeContest(BattleEnvironment.Tower, 2, 1); 
        });
    }

     void SetupKuleSavas()
    {
        titleText.text = "Kule Savunması";
        descText.text = "İlerideki dost gözetleme kulesi haydutların saldırısı altında! Onlara yardım edecek misin?";

         int readySoldiers = GetAvailableSoldierCount();
    
        if (readySoldiers == 0)
        {
            descText.text += "\n\n<color=red>SAVAŞA HAZIR ASKER YOK!</color>\nHerkes görevde, çalışıyor veya yaralı.";
        }
        else
        {
            descText.text += $"\n\nSavaşa Hazır Asker: <color=green>{readySoldiers}</color>";
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
                    TutorialManager.Instance.AdvanceTutorial(); // Battle_ScriptedLoss'a geçirir
                }
                AudioManager.Instance.PlayWarHorn();
                //DayManager.Instance.NextDay(3); 
                ClosePanel();
                BattleManager.Instance.StartBattle(3, 1, BattleEnvironment.Tower); 
            });
        }

        CreateButton("Beni İlgilendirmez", () => {
            
            ReputationManager.Instance.ChangeReputation(-5); 
            NotificationManager.Instance.Show("Kule düştü, itibar kaybettin.", NotificationType.Error);
            ClosePanel();
        });
    }

    // --- YENİ: NASİP VE ZAR ÇÖZÜMLEME EKRANI ---
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
            string mathText = $"\n\n<size=85%><b><color=#FFD700>Nasip ({mevcutNasip}) + Zar ({zarSonucu}) = {totalScore}</color></b> / Kaçış Barajı ({kacisBaraji})</size>\n";

            if (isWin)
            {
                descText.text = $"<color=green>YOLUMUZ AÇIKMIŞ!</color>\n\nNasibimiz yaver gitti, pusuyu fark edip sessizce etraflarından dolanmayı başardık. Kan dökülmeden izimizi kaybettirdik!\n" + mathText;
                
                CreateButton("Yola Devam Et", () => { ClosePanel(); });
            }
            else
            {
                descText.text = $"<color=red>NASİP KAPALIYMIŞ!</color>\n\nKaçmaya çalışırken kuru bir dala bastık... Bizi fark ettiler! Üstelik hazırlıksız yakalandığımız için moraller bozuldu(-10).\n" + mathText;
                
                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-10); 

                CreateButton("Kılıçları Çekin! (Savaş)", () => { 
                    AudioManager.Instance.PlayWarHorn();
                    ClosePanel();
                    BattleManager.Instance.StartBattle(failEnemyCount, failDifficulty, failEnv); 
                });
            }
        });
    }

    void SetupRestEvent()
    {
        titleText.text = "Yörük Obası";
        descText.text = "Dost bir oba buldun. Burada dinlenip yaraları sarabiliriz.";
        
        CreateButton("Dinlen ve İyileş (1 Gün)", () => {
            
            DayManager.Instance.NextDay(1);
            NotificationManager.Instance.Show("Askerler dinlendi, moraller düzeldi.", NotificationType.Success);
            ClosePanel();
        });
    }
public void SetupDervishEvent()
    {
        titleText.text = "Yolcu Derviş";
        descText.text = "Tozlu yolların kenarında, üstü başı yamalı ama gözleri huzurla parlayan yaşlı bir derviş oturuyor. Önündeki boş ahşap kaseye bakarak mırıldanıyor:\n\n<color=#FFD700>\"Dünya malı dünyada kalır Uç Beyim... Yolda kalmışa bir lokma, bir akçe sadakan var mıdır? Veren el, alan elden üstündür.\"</color>";
        
        // if(dervishSprite != null) eventImage.sprite = dervishSprite;

        // SEÇENEK 1: Altın Ver (Büyük Sevap)
        CreateButton("Kesesini Doldur (-50 Akçe)", () => 
        {
            if (MoneyManager.Instance.Spend(50))
            {
                foreach(Transform child in buttonContainer) Destroy(child.gameObject);
                
                descText.text = "Dervişin yüzünde sıcacık bir tebessüm belirdi. Ellerini göğe açıp sana dualar etti.\n\n<color=green>\"Allah kılıcını keskin, atını kavi, nasibini gür eylesin evlat...\"</color>\n\n<size=85%><color=yellow>(Nasip Arttı! Askerlerin içi huzurla doldu.)</color></size>";
                
                if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(2); // Nasibi artır
                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(10); // Vicdani rahatlık
                
                CreateButton("Yola Devam Et", () => { ClosePanel(); });
            }
            else
            {
                NotificationManager.Instance.Show("Yeterli Akçen yok!", NotificationType.Error);
            }
        });

        // SEÇENEK 2: Erzak Ver (Orta Sevap)
        CreateButton("Erzak Paylaş (-20 Erzak)", () => 
        {
            if (SupplyManager.Instance.currentFood >= 20)
            {
                SupplyManager.Instance.SpendFood(20);
                foreach(Transform child in buttonContainer) Destroy(child.gameObject);
                
                descText.text = "Derviş uzattığın ekmeği ve suyu minnetle kabul etti.\n\n<color=green>\"Sofranız bereketli olsun Beyim...\"</color>\n\n<size=85%><color=yellow>(Nasip Arttı!)</color></size>";
                
                if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(1); 
                
                CreateButton("Yola Devam Et", () => { ClosePanel(); });
            }
            else
            {
                NotificationManager.Instance.Show("Yeterli Erzağın yok!", NotificationType.Error);
            }
        });

        // SEÇENEK 3: Reddet (Nasip Kapanır)
        CreateButton("Sırt Çevir (Geçip Git)", () => 
        {
            foreach(Transform child in buttonContainer) Destroy(child.gameObject);
            
            descText.text = "Askerlerine yürüyüş kararı verdin. Derviş arkandan sessizce bakakaldı. Havanın aniden soğuduğunu ve rüzgarın tersine esmeye başladığını hissettin...\n\n<size=85%><color=red>(Nasip Kapandı... Ordunun morali bozuldu.)</color></size>";
            
            if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(1); // Kul hakkı / Cimrilik
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5); 
            
            CreateButton("Yola Devam Et", () => { ClosePanel(); });
        });
    }

    public void SetupCaravanEvent()
    {
        titleText.text = "Savunmasız Kervan";
        descText.text = "Orman yolunda ilerlerken çamura saplanmış, tekerleği kırılmış zengin bir tüccar kervanı gördün. Muhafızları kaçmış, tüccar ise çaresizce mallarını kurtarmaya çalışıyor. Arabanın kasası altın ve erzakla dolup taşıyor.\n\n<color=#FFD700>\"Uç Beyim! Yetiş imdadıma! Kurtların ve eşkıyaların insafına kaldım!\"</color>";
        
        // SEÇENEK 1: Yardım Et (Helal Yol)
        CreateButton("Yardım Eli Uzat (Askerleri Yor)", () => 
        {
            foreach(Transform child in buttonContainer) Destroy(child.gameObject);
            
            descText.text = "Askerlerine emir verdin, omuz omuza verip arabayı çamurdan çıkardınız. Tüccar minnetle ellerine sarıldı ve sana helalinden bir miktar ödül verdi.\n\n<color=green>\"Hızır gibi yetiştiniz Beyim! Bu kadarı ancak helaldir!\"</color>\n\n<size=85%><color=yellow>(Nasip Arttı! +100 Akçe, +10 Odun, Ancak askerler yoruldu.)</color></size>";
            
            if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(1);
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5); // Yorulma cezası
            MoneyManager.Instance.Add(100);
            ResourceManager.Instance.AddWood(10);
            
            CreateButton("Yola Devam Et", () => { ClosePanel(); });
        });

        // SEÇENEK 2: Yağmala (Zulüm)
        CreateButton("Mallara El Koy! (Yağma)", () => 
        {
            foreach(Transform child in buttonContainer) Destroy(child.gameObject);
            
            descText.text = "<color=red>ZULÜM!</color>\n\nKılıçları çektiniz. Tüccar ağlayarak ormana kaçtı. Arabadaki tüm zenginliğe çöktünüz. Hazinen dolup taştı ama askerlerinin yüzündeki o onurlu ifade yerini hırsızlık utancına bıraktı. Gökyüzü karardı...\n\n<size=85%><color=red>(Nasip Ciddi Şekilde Düştü! Büyük Kul Hakkı... Ordunun Şerefi Zedelendi.)</color></size>";
            
            if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(3); // Çok büyük nasip kaybı
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-20); // Şeref kaybı
            
            // Devasa ama haram ödül
            MoneyManager.Instance.Add(600);
            SupplyManager.Instance.AddFood(100);
            
            CreateButton("Vicdan Azabıyla Devam Et", () => { ClosePanel(); });
        });

        // SEÇENEK 3: Görmezden Gel (Tarafsız)
        CreateButton("Bizi İlgilendirmez (Geç)", () => 
        {
            foreach(Transform child in buttonContainer) Destroy(child.gameObject);
            
            descText.text = "Tüccarın feryatlarına kulak tıkayıp yoluna devam ettin. Arkada onu kurtlara ve eşkıyalara yem olarak bıraktın.\n\n<size=85%><color=gray>(Zaman kaybetmediniz ama vicdanlar sızladı.)</color></size>";
            
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
            if (AudioManager.Instance != null) AudioManager.Instance.PlayWarHorn();
            BattleManager.Instance.StartBossBattle(5, BattleEnvironment.Tower); 
            ClosePanel();
        });
    }

    void SetupArcheryEvent()
    {
        titleText.text = "Okçuluk Müsabakası";
        descText.text = "Bir Türkmen beyi senin yiğitliğini ölçmek için okçuluk müsabakasına davet etti. Gidecek misin?";
        if(bossSprite != null) eventImage.sprite = archerySprite; 

        CreateButton("Git (1 Gün)", () => {
            
            DayManager.Instance.NextDay(1);
            GoToArcheryScene();
            ClosePanel();
        });

        CreateButton("Teklifi Reddet (-5 İtibar)", () => {
            
            ReputationManager.Instance.ChangeReputation(-5);
            NotificationManager.Instance.Show("Bey bu teklifi reddetmene kırıldı.", NotificationType.Warning);
            ClosePanel();
        });
    }

    // --------------------------------------------------------
    // MASAÜSTÜ RPG SİSTEMİ (AT YARIŞI & GÜREŞ)
    // --------------------------------------------------------

    void SetupAtYarisi()
    {
        titleText.text = "Büyük At Yarışı";
          if(villageSprite != null) eventImage.sprite = horseSprite;
        // Rakibin adını ve hızını açıkça yazıyoruz
        descText.text = $"Şehrin ileri gelenlerinden Mustafa Bey seni at yarışına davet etti. (Giriş: 30 Akçe)\n\n" +
                        $"<color=#66001D>Rakip: {atYarisiRakipIsim} (Hızı: {atYarisiRakipHiz})</color>\n\n" +
                        $"<color=yellow>Hangi yiğidi göndereceksin? (Askerinin hızı üzerine 6'lık zar atılacaktır.)</color>";

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
                        DayManager.Instance.NextDay(1); 
                        
                        // Zarı atmak üzere ResolveContest'e gönder
                        ResolveContest(soldier, NodeType.Atyarisi, askerHiz, atYarisiRakipHiz, atYarisiRakipIsim);
                    }
                    else
                    {
                        NotificationManager.Instance.Show("Yarışa katılmak için 30 Akçen yok!", NotificationType.Error);
                    }
                });
            }
        }

        if (!hasValidSoldier) descText.text += "\n\n<color=red>Gönderecek boşta askerin yok!</color>";
        CreateButton("Vaktim Yok (Ayrıl)", () =>{
            
         ClosePanel();
         });
    }

    void SetupYagliGures()
    {
        titleText.text = "Yağlı Güreş!";
          if(villageSprite != null) eventImage.sprite = wrestlingSprite;
        // Rakibin adını ve gücünü açıkça yazıyoruz
        descText.text = $"Meydanda davullar çalıyor. Başpehlivanlık için er meydanına bir yiğidini sal. (Giriş: 50 Akçe)\n\n" +
                        $"<color=#66001D>Rakip Pehlivan: {guresRakipIsim} (Gücü: {guresRakipGuc})</color>\n\n" +
                        $"<color=#66001D>Kimi yollayacaksın? (Askerinin gücü üzerine 6'lık zar atılacaktır.)</color>";
        
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
                        DayManager.Instance.NextDay(1);
                        
                        ResolveContest(soldier, NodeType.YagliGures, askerGuc, guresRakipGuc, guresRakipIsim);
                    }
                    else
                    {
                        NotificationManager.Instance.Show("Güreşe katılmak için 50 Akçen yok!", NotificationType.Error);
                    }
                });
            }
        }

        if (!hasValidSoldier) descText.text += "\n\n<color=red>Gönderecek boşta askerin yok!</color>";
        CreateButton("Bize Göre Değil (Ayrıl)", () => {
        
        ClosePanel();
        });
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
            string mathText = $"\n\n<size=85%><b><color=#FFD700>Askerin {statAd} ({askerStat}) + Zar ({zarSonucu}) = {totalScore}</color></b> / Hakem Puanı ({rakipStat})</size>\n";

            if (isWin)
            {
                string flavorText = eventType == NodeType.Atyarisi
                    ? $"{selectedSoldier.data.gladiatorName} rüzgar gibi esti! Yarışı birinci bitirdi."
                    : $"{selectedSoldier.data.gladiatorName} sırtı yere gelmeden rakibi {rakipAd}'i tuş etti!";

                descText.text = $"<color=green>ZAFER!</color>\n\nYiğidimiz beklentileri aştı ve müsabakayı kazandı!\n\n" + flavorText + mathText;

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

                descText.text = $"<color=red>KAYBETTİN!</color>\n\nMaalesef yiğidimiz müsabakayı kaybetti.\n\n" + flavorText + mathText;
                
                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-10);
            }

            CreateButton("Devam Et", () => { ClosePanel(); });
        });
    }

    // --------------------------------------------------------
    // YARDIMCI FONKSİYONLAR
    // --------------------------------------------------------
    
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

    void CreateButton(string text, UnityEngine.Events.UnityAction action)
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
}