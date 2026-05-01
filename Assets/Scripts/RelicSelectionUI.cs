using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RelicSelectionUI : MonoBehaviour
{
    public GameObject relicPanel; // Tüm ekranı kaplayan karartılmış panel
    
    [Header("Butonlar (En fazla 3 adet)")]
    public Button[] relicButtons; 
    public TextMeshProUGUI[] relicTitleTexts;
    public TextMeshProUGUI[] relicDescTexts;

    void Awake()
    {
        if (relicPanel != null) relicPanel.SetActive(false);
    }

    public void ShowOptions(List<RelicType> options)
    {
        relicPanel.SetActive(true);
        
        // Oyunun arkada akmasını durdurmak iyi bir fikirdir (Roguelike klasiği)
        // Time.timeScale = 0f; 

        // Bütün butonları önce kapat, sadece seçenek sayısı kadarını aç
        for (int i = 0; i < relicButtons.Length; i++)
        {
            relicButtons[i].gameObject.SetActive(false);
            relicButtons[i].onClick.RemoveAllListeners(); 
        }

        for (int i = 0; i < options.Count; i++)
        {
            relicButtons[i].gameObject.SetActive(true);
            RelicType currentRelic = options[i];

            // Yadigarın İsmini ve Açıklamasını Doldur
            relicTitleTexts[i].text = GetRelicName(currentRelic);
            relicDescTexts[i].text = GetRelicDescription(currentRelic);

            // Butona tıklandığında ne olacağını ayarla
            relicButtons[i].onClick.AddListener(() => OnRelicSelected(currentRelic));
        }
    }

    private void OnRelicSelected(RelicType chosenRelic)
    {
        // Time.timeScale = 1f; // Zamanı geri başlat

        // MetaProgressionManager'a bunu kaydetmesini söyle
        MetaProgressionManager.Instance.UnlockRelic(chosenRelic);

        // Paneli Kapat
        relicPanel.SetActive(false);
    }

    // İsim Sözlüğü
    private string GetRelicName(RelicType type)
    {
        switch (type)
        {
            case RelicType.ZenginAta: return "Zengin Ata";
            case RelicType.KutluMide: return "Kutlu Mide";
            case RelicType.DemirDovucu: return "Demir Döğücü";
            case RelicType.GaziKani: return "Gazi Kanı";
            case RelicType.BereketliYol: return "Bereketli Yol";
            case RelicType.CesurYurek: return "Cesur Yürek";
            default: return "Gizemli Yadigar";
        }
    }

    // Açıklama Sözlüğü
    private string GetRelicDescription(RelicType type)
    {
        switch (type)
        {
            case RelicType.ZenginAta: return "Sürgün edilsen bile, yeni sefere başlarken daima cebinde ekstra 150 Akçe olur.";
            case RelicType.KutluMide: return "Askerlerin erzak tüketimi inanılmaz derecede azalır.";
            case RelicType.DemirDovucu: return "Kamptaki eğitim ve bina tamiratları %20 daha ucuza mal olur.";
            case RelicType.GaziKani: return "Yetiştirdiğin veya satın aldığın her asker +10 ekstra max can ile saflara katılır.";
            case RelicType.BereketliYol: return "Haritadaki ganimet sandıkları ve tüccarlardan %15 daha fazla kazanç sağlarsın.";
            case RelicType.CesurYurek: return "Savaşları kaybetsen bile ordunun morali eskisinin yarısı kadar düşer.";
            default: return "Atalarından kalma gizemli bir güç.";
        }
    }
}