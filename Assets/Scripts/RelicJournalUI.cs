using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RelicJournalUI : MonoBehaviour
{
    public static RelicJournalUI Instance;

    [Header("Sol Sayfa (Katalog)")]
    public Transform leftGridContainer;     // İkonların dizileceği GridLayout parent'ı
    public GameObject relicIconPrefab;      // Sol sayfadaki küçük buton prefabı

    [Header("Sağ Sayfa (Dinamik Detaylar)")]
    public TextMeshProUGUI rightTitleText;  // Mirasın Adı
    public TextMeshProUGUI rightDescText;   // Mirasın Açıklaması
    public Image rightIllustrationImage;    // Sağdaki o büyük çizim alanı

    [Header("Kapatma Butonu")]
    public Button closeButton;

    void Awake()
    {
        Instance = this;
        if (closeButton != null) closeButton.onClick.AddListener(CloseJournal);
    }

    /// <summary>
    /// Defter panelini açar ve tüm verileri tazeleyerek ilk elementi seçer.
    /// </summary>
    public void OpenJournal()
    {
        gameObject.SetActive(true);
        PopulateJournal();
    }

    public void CloseJournal()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Sol sayfayı CommanderStorage'dan okuyarak doldurur.
    /// </summary>
   void PopulateJournal()
    {
        // 1. Eski ikonları temizle
        foreach (Transform child in leftGridContainer) Destroy(child.gameObject);

        if (CommanderStorage.Instance == null || CommanderStorage.Instance.activeRelics.Count == 0)
        {
            ClearRightPage();
            return;
        }

        List<RelicData> relics = CommanderStorage.Instance.activeRelics;

        // 2. Her bir miras için sol sayfada buton oluştur
        for (int i = 0; i < relics.Count; i++)
        {
            RelicData relic = relics[i];
            if (relic == null) continue;

            GameObject iconObj = Instantiate(relicIconPrefab, leftGridContainer);
            
            // ── KRİTİK DÜZELTME: Prefabın dış çerçevesini DEĞİL, içindeki "Icon" isimli çocuk imajı bul! ──
            Transform iconChild = iconObj.transform.Find("Icon"); 
            if (iconChild != null)
            {
                Image imgComp = iconChild.GetComponent<Image>();
                if (imgComp != null) imgComp.sprite = relic.icon; // Küçük mühür ikonu buraya biniyor
            }
            else
            {
                // Eğer çocuk objenin adı "Icon" değilse, prefabın içindeki alt Image bileşenini korumalı ara:
                Image[] imgs = iconObj.GetComponentsInChildren<Image>();
                if (imgs.Length > 1) imgs[1].sprite = relic.icon; // 0 kök çerçevedir, 1 içindeki ikondur
            }

            // Butona tıklanınca sağ sayfaya DOĞRU RELIC datasını gönder
            Button btn = iconObj.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectRelic(relic));
            }

            // İlk açılışta ilk mirası otomatik seç ve göster
            if (i == 0)
            {
                SelectRelic(relic);
            }
        }
    }

    /// <summary>
    /// Sol sayfadan bir mirasa tıklandığında sağ sayfayı doldurur.
    /// </summary>
    public void SelectRelic(RelicData relic)
    {
        if (relic == null) return;

        if (rightTitleText != null) rightTitleText.text = relic.relicName;
        if (rightDescText != null) rightDescText.text = relic.description;
        
        if (rightIllustrationImage != null)
        {
            if (relic.icon != null) // Varsa büyük eskiz çizimi, yoksa kendi ikonu
            {
                rightIllustrationImage.gameObject.SetActive(true);
                rightIllustrationImage.sprite = relic.icon; // Büyük çizim sprite'ınız
            }
            else
            {
                rightIllustrationImage.gameObject.SetActive(false);
            }
        }
    }

    void ClearRightPage()
    {
        if (rightTitleText != null) rightTitleText.text = "Miras Bulunmuyor";
        if (rightDescText != null) rightDescText.text = "Seferlerde ilerleyerek kutsal emanetleri ve komutan miraslarını edinin.";
        if (rightIllustrationImage != null) rightIllustrationImage.gameObject.SetActive(false);
    }
}