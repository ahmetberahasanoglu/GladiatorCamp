using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MapEventManager : MonoBehaviour
{
    public static MapEventManager Instance;

    [Header("UI Elemanları")]
    public GameObject eventPanel;       // Panelin kendisi
    public GameObject topPanel; 
    public Image eventImage;            // Olay resmi
    public TextMeshProUGUI titleText;   // Başlık
    public TextMeshProUGUI descText;    // Açıklama
    
    public Transform buttonContainer;   // Butonların dizileceği yer
    public GameObject buttonPrefab;     // Seçim butonu kalıbı

    [Header("Olay Görselleri")]
    public Sprite villageSprite;
    public Sprite battleSprite;
    public Sprite bossSprite;
    public Sprite merchant;
    // İhtiyacın olursa buraya vahşi, zindan vb. spritelar da ekleyebilirsin.

    void Awake()
    {
        Instance = this;
        if (eventPanel != null) eventPanel.SetActive(false); 
    }

    // MapManager'dan çağrılacak fonksiyon
    public void TriggerEvent(NodeType type)
    {
        eventPanel.SetActive(true);
        AudioManager.Instance.PlayPaper();
        // Önceki butonları temizle (Yeni event için tertemiz bir sayfa)
        foreach(Transform child in buttonContainer) 
        {
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
        }
    }

    // --------------------------------------------------------
    // YENİ EKLENEN VE DOLDURULAN EVENTLER
    // --------------------------------------------------------

    void SetupVahsi()
    {
        titleText.text = "Aç Kurt Sürüsü";
        descText.text = "Ormandan geçerken etrafını aç bir kurt sürüsü sardı! Savaşmak yorucu olacak ama kaçmak da vakit kaybettirir.";
        // eventImage.sprite = vahsiSprite; // Varsa ekle

        CreateButton("Savaş (-1 Gün, +20 Altın)", () => {
            DayManager.Instance.NextDay(1);
           // MoneyManager.Instance.Add(20); // Postlarını sattın
           int currentPending = PlayerPrefs.GetInt("PendingGold", 0);
            PlayerPrefs.SetInt("PendingGold", currentPending + 20);
            PlayerPrefs.Save();
            NotificationManager.Instance.Show("Kurtları alt ettin ve postlarını sattın.", NotificationType.Success);
            ClosePanel();
            // İleride buraya mini savaş sahnesi de eklenebilir
        });

        CreateButton("Etrafından Dolaşarak Kaç (-2 Gün)", () => {
            DayManager.Instance.NextDay(2);
            NotificationManager.Instance.Show("Güvenli ama uzun yolu seçtin.", NotificationType.Info);
            ClosePanel();
        });
    }
// Bu olayı haritadaki "?" (Soru İşareti) nodelarından birine bağlayabilirsin
    public void SetupMysteriousDiceEvent()
    {
        titleText.text = "Gizemli Yabancı";
        descText.text = "Karanlık bir pelerin giymiş bir adam yolunu kesti. Avucundan kemikten yapılma bir zar çıkardı.\n\n<color=yellow>\"Şu zarı at bakalım Uç Beyi... Eğer 3'ten büyük atarsan sana bir sırrımı (ve altınlarımı) vereceğim.\"</color>";
        // if (mysteriousSprite != null) eventImage.sprite = mysteriousSprite; // Varsa görselini koy

        // --- 1. SEÇENEK: ZARI AT ---
        // --- 1. SEÇENEK: ZARI AT ---
        CreateButton("Zarı At (Risk Al)", () => 
        {
            AudioManager.Instance.PlayDice();
            // YENİ: Zarın hedefi 4 (Çünkü 3'ten büyük olmalı: 4, 5, 6)
            DiceManager.Instance.RollDice(4, (zarSonucu) => 
            {
                if (zarSonucu >= 4) // Kuralı da buna göre düzenledik
                {
                    NotificationManager.Instance.Show($"Zar {zarSonucu} geldi! Adam sana kese fırlattı (+150 Akçe)", NotificationType.Success);
                    MoneyManager.Instance.Add(150); 
                    if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(5);
                }
                else
                {
                    NotificationManager.Instance.Show($"Zar {zarSonucu} geldi... Adam gülerek karanlıkta kayboldu.", NotificationType.Warning);
                    if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5);
                }

                ClosePanel(); 
            });
        });

        // --- 2. SEÇENEK: GÖRMEZDEN GEL ---
        CreateButton("Adamı Kov", () => 
        {
            NotificationManager.Instance.Show("Kumara ayıracak vaktim yok deyip yoluna devam ettin.", NotificationType.Info);
            ClosePanel();
        });
    }
    void SetupZindan()
    {
        titleText.text = "Karanlık Mağara";
        descText.text = "İçeriden garip sesler geliyor. Büyük bir tehlike ama aynı zamanda büyük bir ganimet yatıyor olabilir.";
        
        CreateButton("İçeri Gir (Savaş)", () => {
            DayManager.Instance.NextDay(1);
            ClosePanel();
            // BattleManager.Instance.StartBattle(6, 1); // İleride zindan savaşı eklenecek
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
            //MoneyManager.Instance.Add(30);
            NotificationManager.Instance.Show("Eşyaları alıp sattın.", NotificationType.Success);
            ClosePanel();
        });

        CreateButton("Burası Tekinsiz, Ayrıl", () => {
            ClosePanel();
        });
    }

    void SetupTuccar() // Eski "ShowForestEvent" kodunu buraya entegre ettim
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

        CreateButton("Yoluna Devam Et", () => {
            ClosePanel(); 
        });
    }

    // --------------------------------------------------------
    // DÜZELTİLEN EVENTLER (Kopyala-Yapıştır Hataları Giderildi)
    // --------------------------------------------------------

    void SetupTreasure()
    {
        titleText.text = "Gizli Hazine";
        descText.text = "Burada büyük bir hazine yatıyor ancak biraz kürek sallamanız gerekecek. 1 Gününü alacak, sence zahmetine değecek mi?";

        CreateButton("Kazmaya Başla (-1 Gün, +100 Altın)", () => {
            DayManager.Instance.NextDay(1); 
            //MoneyManager.Instance.Add(100);
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

    void SetupKuleSavas()
    {
        titleText.text = "Kule Savunması";
        descText.text = "İlerideki dost gözetleme kulesi haydutların saldırısı altında! Onlara yardım edecek misin?";

        CreateButton("Savunmaya Yardım Et", () => {
            if (!HasAliveSoldiers())
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show("Savaşa sokacak hiç askerin yok! Kampa dönmelisin.", NotificationType.Error);
                return; // Kodu burada kes, savaşa GİRME!
            }
            if (AudioManager.Instance != null) AudioManager.Instance.PlayWarHorn();
            DayManager.Instance.NextDay(1); 
            ClosePanel();
            topPanel.SetActive(true);
            // BattleManager.Instance.StartBattle(5, 1); // 5 Düşman
            NotificationManager.Instance.Show("Savaş Başlıyor!", NotificationType.Warning);
        });

        CreateButton("Beni İlgilendirmez", () => {
            ReputationManager.Instance.ChangeReputation(-5); // Yardım etmediği için itibar düşer
            NotificationManager.Instance.Show("Kule düştü, itibar kaybettin.", NotificationType.Error);
            ClosePanel();
        });
    }

    // --------------------------------------------------------
    // MEVCUT DOĞRU EVENTLER
    // --------------------------------------------------------

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
    
    void SetupAtYarisi()
    {
        titleText.text = "At Yarışı";
        descText.text = "Şehrin ileri gelenlerinden Mustafa bey seni at yarışına davet etti.";

        CreateButton("Yarış", () => {
            ClosePanel();
            topPanel.SetActive(true);
            // At yarışı sahnesine veya mekaniğine geçiş
        });

        CreateButton("Reddet", () => {
            ClosePanel();
        });
    }

    void SetupStartEvent()
    {
        titleText.text = "Kamp Ekranı";
        descText.text = "Bir kampta dinlen ve hazırlıklarını yap.";
        if(villageSprite != null) eventImage.sprite = villageSprite;
        
        CreateButton("Kampa Geç", () => {
            ClosePanel();
            topPanel.SetActive(true);
            BattleManager.Instance.ReturnToCamp();
        });
    }
    // Savaşa girebilecek en az 1 tane yaşayan askerimiz var mı?
    private bool HasAliveSoldiers()
    {
        var allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        
        foreach (var soldier in allSoldiers)
        {
            // Eğer askerin datası varsa ve canı 0'dan büyükse (yaşıyorsa)
            if (soldier.data != null && soldier.data.currentHealth > 0)
            {
                return GetAvailableSoldierCount() > 0;
            }
        }
        
        return false; // Kimse hayatta değil
    }
   void SetupBattleEvent()
    {
        titleText.text = "Çapulcu Pusu";
        descText.text = "Yol kesen eşkıyalar! Savaşmak 3 gün sürecek ama ganimetleri iyi görünüyor.";
        if(battleSprite != null) eventImage.sprite = battleSprite;
        
        int readySoldiers = GetAvailableSoldierCount();
    
        if (readySoldiers == 0)
        {
            descText.text += "\n\n<color=red>SAVAŞA HAZIR ASKER YOK!</color>\nHerkes görevde, çalışıyor veya yaralı.";
            // Not: CreateButton kısmındaki "Saldır" butonunu devre dışı bırakmalıyız.
        }
        else
        {
            descText.text += $"\n\nSavaşa Hazır Kılıç: <color=green>{readySoldiers}</color>";
        }

        // Saldır Butonu (Eğer asker yoksa basılamaz)
        GameObject atkBtnObj = Instantiate(buttonPrefab, buttonContainer);
        atkBtnObj.GetComponentInChildren<TextMeshProUGUI>().text = "Saldır (3 Gün Sürer)";
        Button atkBtn = atkBtnObj.GetComponent<Button>();
        
        if (readySoldiers == 0)
        {
            atkBtn.interactable = false; // Asker yoksa tuş sönük kalır!
        }
        else
        {
            atkBtn.onClick.AddListener(() => {
                AudioManager.Instance.PlayWarHorn();
                DayManager.Instance.NextDay(3); 
                ClosePanel();
                BattleManager.Instance.StartBattle(2, 1); 
            });
        }

        // Kaçma Butonu (Her zaman basılabilir)
        CreateButton("Etrafından Dolaş (5 Gün Kaybet)", () => {
            DayManager.Instance.NextDay(5);
            NotificationManager.Instance.Show("Güvenli ama uzun yolu seçtin.", NotificationType.Info);
            ClosePanel();
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
            Debug.Log("Final Savaşı!");
            ClosePanel();
            // Final sahnesine geçiş eklenecek
        });
    }

    void SetupArcheryEvent()
    {
        titleText.text = "Okçuluk Müsabakası";
        descText.text = "Bir Türkmen beyi senin yiğitliğini ölçmek için okçuluk müsabakasına davet etti. Gidecek misin?";
        if(bossSprite != null) eventImage.sprite = bossSprite; // Okçuluk için ayrı sprite eklenebilir

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
    // YARDIMCI FONKSİYONLAR
    // --------------------------------------------------------
    // Savaşa girebilecek durumdaki (Boşta, kampta, hayatta olan) askerleri sayar
    public int GetAvailableSoldierCount()
    {
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        int availableCount = 0;

        foreach (var glad in allSoldiers)
        {
            GladiatorAI ai = glad.GetComponent<GladiatorAI>();
            
            // 1. Bizim askerimiz mi ve hayatta mı?
            if (glad.CompareTag("MySoldier") && (ai == null || !ai.isDead))
            {
                // 2. KESİN KONTROL: Görevde değil (isOnMission) VE İşte çalışmıyor (Activity != Working) olmalı
                bool isWorking = (glad.data != null && glad.data.currentActivity == SoldierActivity.Working);
                bool isOnMission = glad.isOnMission;

                // Eğer asker görevde değilse ve işte çalışmıyorsa savaşa hazır demektir
                if (!isOnMission && !isWorking)
                {
                    availableCount++;
                }
            }
        }
        
        return availableCount;
    }

 
    void CreateButton(string text, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = text;
        btnObj.GetComponent<Button>().onClick.AddListener(action);
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