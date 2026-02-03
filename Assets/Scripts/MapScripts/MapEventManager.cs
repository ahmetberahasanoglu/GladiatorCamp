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
        titleText.text = "Bir Türkmen Köyü";
        descText.text = "Köy halkı sizi coşkuyla karşıladı. Beyleri size erzak sunuyor ancak kış yaklaşıyor, askerlerin dinlenmeye ihtiyacı var.";
        eventImage.sprite = villageSprite;

        // Seçenek 1: Erzak Al
        CreateButton("Erzak Al (+50 Yemek)", () => {
            // ResourceManager.AddFood(50);
            ClosePanel();
        });

        // Seçenek 2: Asker Topla
        CreateButton("Gönüllü Topla (2 Asker)", () => {
            // RecruitManager.AddRandomSoldier(2);
            ClosePanel();
        });
    }

    void SetupBattleEvent()
    {
        titleText.text = "Bizans Karakolu";
        descText.text = "Yolun üzerinde küçük bir Bizans birliği kamp kurmuş. Sayıları az ama zırhları sağlam görünüyor.";
        eventImage.sprite = battleSprite;

        CreateButton("Hücum Et! (Savaş Başlat)", () => {
            // SceneManager.LoadScene("BattleScene");
            // Veya mevcut sahnedeki savaş sistemini başlat
            Debug.Log("Savaş Başlıyor...");
            ClosePanel();
            // MapManager.Instance.HideMap(); // Haritayı kapat
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