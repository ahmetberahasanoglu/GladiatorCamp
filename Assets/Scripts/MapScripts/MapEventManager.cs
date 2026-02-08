using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MapEventManager : MonoBehaviour
{
    public static MapEventManager Instance;

    [Header("UI Elemanları")]
    public GameObject eventPanel;       // Panelin kendisi
    public Image eventImage;            // Olay resmi
    public TextMeshProUGUI titleText;   // Başlık
    public TextMeshProUGUI descText;    // Açıklama
    
    public Transform buttonContainer;   // Butonların dizileceği yer
    public GameObject buttonPrefab;     // Seçim butonu kalıbı

    [Header("Olay Görselleri")]
    public Sprite villageSprite;
    public Sprite battleSprite;
    public Sprite bossSprite;

    void Awake()
    {
        Instance = this;
        eventPanel.SetActive(false); // Başlangıçta kapalı
    }

    // MapManager'dan çağrılacak fonksiyon
    public void TriggerEvent(NodeType type)
    {
        eventPanel.SetActive(true);
        
        // Önceki butonları temizle
        foreach(Transform child in buttonContainer) Destroy(child.gameObject);

        switch (type)
        {
            case NodeType.Village:
                SetupVillageEvent();
                break;
            case NodeType.Battle:
                SetupBattleEvent();
                break;
            case NodeType.Boss:
                SetupBossEvent();
                break;
        }
    }

    void SetupVillageEvent()
    {
        titleText.text = "Türkmen Köyü";
        descText.text = "Köy halkı zor durumda. Onlara yardım edersen dualarını alırsın.";
        eventImage.sprite = villageSprite;

        // SEÇENEK 1: Yardım Et (İtibar Kazan)
        CreateButton("Köy Halkına Yardım Et (+10 İtibar)", () => {
            
            if (ReputationManager.Instance != null)
                ReputationManager.Instance.ChangeReputation(10);
            
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Halk sana minnettar!", NotificationType.Success);
            
            ClosePanel();
        });

        // SEÇENEK 2: Pas Geç
        CreateButton("Yola Devam Et", () => {
            ClosePanel();
        });
    }
    

    // ... Önceki kodların ...

    void SetupBattleEvent()
    {
        titleText.text = "Çapulcu Pusu";
        descText.text = "Yol kesen eşkıyalar! Savaşmak 3 gün sürecek ama ganimetleri iyi görünüyor.";
        // ... Resim ayarları ...

        // SEÇENEK 1: Savaş (-3 Gün)
        CreateButton($"Saldır (3 Gün Sürer)", () => {
            
            // 1. Zamanı harca
            DayManager.Instance.NextDay(3); 
            
            // 2. Savaşa Git
            ClosePanel();
            BattleManager.Instance.StartBattle(5, 1); // 5 Düşman
        });

        // SEÇENEK 2: Kaç / Etrafından Dolaş (-5 Gün)
        // Savaşmazsın ama yolu uzatırsın, vakit kaybedersin
        CreateButton("Etrafından Dolaş (5 Gün Kaybet)", () => {
            DayManager.Instance.NextDay(5);
            NotificationManager.Instance.Show("Güvenli ama uzun yolu seçtin.", NotificationType.Info);
            ClosePanel();
            // Bir sonraki harita noktasına geçiş izni verilir
        });
    }

    void SetupRestEvent() // Dinlenme Noktası (Oba/Kervansaray)
    {
        titleText.text = "Yörük Obası";
        descText.text = "Dost bir oba buldun. Burada dinlenip yaraları sarabiliriz.";
        
        CreateButton("Dinlen ve İyileş (1 Gün)", () => {
            DayManager.Instance.NextDay(1);
            
            // Askerleri iyileştirme kodu (Örnek)
            // HealAllSoldiers(20); 
            
            NotificationManager.Instance.Show("Askerler dinlendi, moraller düzeldi.", NotificationType.Success);
            ClosePanel();
        });
    }

    void SetupBossEvent()
    {
        titleText.text = "KIZIL KALE";
        descText.text = "Sonunda hedefe ulaştın. Kale surları göğe yükseliyor. Bu son savaş olacak.";
        eventImage.sprite = bossSprite;

        CreateButton("KUŞATMAYI BAŞLAT", () => {
            Debug.Log("Final Savaşı!");
            ClosePanel();
        });
    }

    // Yardımcı fonksiyon: Buton oluşturma
    void CreateButton(string text, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = text;
        btnObj.GetComponent<Button>().onClick.AddListener(action);
    }

    public void ClosePanel()
    {
        eventPanel.SetActive(false);
        // Olay bitti, belki tekrar kampa/haritaya döneriz
    }
}