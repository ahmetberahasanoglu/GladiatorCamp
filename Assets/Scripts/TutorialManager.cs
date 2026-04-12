using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;

// --- GÜNCELLENMİŞ EĞİTİM ADIMLARI ---
public enum TutorialStep
{
    None, Intro_Target, Intro_CampNode, Intro_CampPanel, 
    Camp_Tour,Camp_Tour1,Camp_Tour2, Camp_GoToWar, Map_FirstBattle, Map_FirstBattlePanel, 
    Battle_ScriptedLoss, 
    Rebirth_Intro,      // Savaştan dönüldü, asker yok
    Rebirth_Recruit,    // Devşirme Binası
    Rebirth_Blacksmith, // Demirci
    Rebirth_Equip,      // Asker Eşya Takma
    Rebirth_Training,   // Talimhane
    Rebirth_Cenk,       // Cenk Oyunu
    Rebirth_TempUI,     // Sıcaklık Arayüzü
    Rebirth_Campfire,   // Kamp Ateşi ve Moral
    Completed           // Bitiş
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Test Ayarları")]
    public bool testModu_EgitimiSifirla = false; 

    [Header("Eğitim Durumu")]
    public TutorialStep currentStep = TutorialStep.None;
    public bool isTutorialActive = false;

    [Header("Arayüz (UI) Referansları")]
    public GameObject topBarGoldUI;    
    public GameObject itibarUI;    
    public GameObject borcUI;    
    public GameObject nasipUI;    
    public GameObject moralUI;    
    public GameObject kizilKaleNodeUI; 
    public GameObject firstCampNodeUI; 
    public GameObject firstBattleNodeUI; 
    public GameObject topBarTempUI; // YENİ: TopBar'daki Sıcaklık (°C) göstergesi

    [Header("3D Obje Referansları ve Çember Boyutları")]
    public Transform warTable3D; 
    [Range(0.5f, 5f)] public float warTableMarkerScale = 1.2f; 

    // YENİ UYANIŞ (REBIRTH) BİNALARI
    public Transform devsirme3D;
    [Range(0.5f, 5f)] public float devsirmeMarkerScale = 2.0f;

    public Transform blacksmith3D;
    [Range(0.5f, 5f)] public float blacksmithMarkerScale = 2.0f;

    public Transform talimhane3D;
    [Range(0.5f, 5f)] public float talimhaneMarkerScale = 2.5f;

    public Transform cenkOyunu3D;
    [Range(0.5f, 5f)] public float cenkOyunuMarkerScale = 1.5f;

    public Transform campfire3D;
    [Range(0.5f, 5f)] public float campfireMarkerScale = 1.8f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (testModu_EgitimiSifirla)
        {
            PlayerPrefs.DeleteKey("TutorialCompleted");
            PlayerPrefs.Save();
        }

        int tutFinished = PlayerPrefs.GetInt("TutorialCompleted", 0);
        if (tutFinished == 0) StartTutorial();
    }

    public void StartTutorial()
    {
        isTutorialActive = true;
        SetStep(TutorialStep.Intro_Target);
    }

    public void SetStep(TutorialStep newStep)
    {
        currentStep = newStep;
        if (TutorialHighlighter.Instance != null) TutorialHighlighter.Instance.RemoveHighlight();

        switch (currentStep)
        {
            case TutorialStep.Intro_Target:
                StartCoroutine(HighlightUIDelayed(kizilKaleNodeUI));
                ShowDialogue("Sınır boylarına hoş geldin. Sen bir uç beyisin, paşadan aldığın emre göre kış bastırmadan Kızıl Kale'yi düşürmelisin.", false);
                break;

            case TutorialStep.Intro_CampNode:
                StartCoroutine(HighlightUIDelayed(firstCampNodeUI));
                ShowDialogue("Fakat ordu aç, kılıçlar paslı. Önce şu düzlüğe otağımızı kuralım. (Haritadaki Kampa Tıkla)", true);
                break;

            case TutorialStep.Intro_CampPanel:
                ShowDialogue("Otağımıza girmek için paneldeki 'Kampa Geç' butonuna tıkla.", true,new Vector2(0, 400f));
                break;

            case TutorialStep.Camp_Tour:
                StartCoroutine(HighlightUIDelayed(topBarGoldUI));
                ShowDialogue("Burası merkezimiz. Yukarıda Altınımız, Erzağımız ve Odun sayımız var.", false);
                break;
            case TutorialStep.Camp_Tour1:
                StartCoroutine(HighlightUIDelayed(itibarUI));
                StartCoroutine(HighlightUIDelayed(borcUI));
                ShowDialogue("Maaşları ödemek için altının kalmazsa borç alabilirsin ama itibarına dikkat et. ", false);
                break;
            case TutorialStep.Camp_Tour2:
                StartCoroutine(HighlightUIDelayed(nasipUI));
                ShowDialogue("Adil bir bey olursan nasibin açılır. ", false);
                break;

            case TutorialStep.Camp_GoToWar:
                StartCoroutine(Highlight3DDelayed(warTable3D, warTableMarkerScale));
                ShowDialogue("Şimdi gücümüzü sınama vakti. Savaş masasına tıkla ve haritayı aç.", true);
                break;

            case TutorialStep.Map_FirstBattle:
                StartCoroutine(HighlightUIDelayed(firstBattleNodeUI));
                ShowDialogue("Haydut grubunun üzerine yürü! (Savaş Noktasına Tıkla)", true);
                break;

            case TutorialStep.Map_FirstBattlePanel:
                ShowDialogue("Düşman karşımızda! Tereddüt etme, 'Saldır' emrini ver!", true,new Vector2(0, 350f));
                break;

            case TutorialStep.Battle_ScriptedLoss:
                ShowDialogue("Kılıçları çekin!", true); 
                StartCoroutine(HideDialogueAfterSeconds(1f));
                break;

            // ==========================================
            // YENİ: UYANIŞ VE KAMP TANITIMI (REBIRTH)
            // ==========================================
            case TutorialStep.Rebirth_Intro:
                // Karanlık ekran, vurgu yok
                ShowDialogue("Ağır bir yara aldık... Tek bir askerimiz bile sağ kalmadı. Ama metin ol, otağı yeniden ayağa kaldıracağız.", false);
                break;

            case TutorialStep.Rebirth_Recruit:
                StartCoroutine(Highlight3DDelayed(devsirme3D, devsirmeMarkerScale));
                ShowDialogue("İlk işimiz harabe binaları onarıp yeni yiğitler bulmak. Devşirme çadırından altın karşılığı ordunu baştan kurabilirsin.", false);
                break;

            case TutorialStep.Rebirth_Blacksmith:
                StartCoroutine(Highlight3DDelayed(blacksmith3D, blacksmithMarkerScale));
                ShowDialogue("Fakat askerleri ekipmansız ölüme gönderemeyiz. Demircide onlara sağlam zırhlar ve kılıçlar dövdürmelisin.", false);
                break;

            case TutorialStep.Rebirth_Equip:
                // Burada sadece metin veriyoruz, ekranın ortasında okuyacak
                ShowDialogue("Ekipmanları ürettikten sonra, askerinin üzerine tıklayarak envanterini açmalı ve eşyaları bizzat kuşandırmalısın.", false);
                break;

            case TutorialStep.Rebirth_Training:
                StartCoroutine(Highlight3DDelayed(talimhane3D, talimhaneMarkerScale));
                ShowDialogue("Tecrübesiz erler savaşta çabuk düşer. Onları Talimhane'de eğiterek güçlendirmeyi ihmal etme.", false);
                break;

            case TutorialStep.Rebirth_Cenk:
                StartCoroutine(Highlight3DDelayed(cenkOyunu3D, cenkOyunuMarkerScale));
                ShowDialogue("Boş vakitlerinde askerlerinle Cenk oynayarak hem moral hem de fazladan altın kazanabilirsin.", false);
                break;

            case TutorialStep.Rebirth_TempUI:
                StartCoroutine(HighlightUIDelayed(topBarTempUI));
                ShowDialogue("Unutma, kış kapıda... Yukarıdaki sıcaklık göstergesini takip et. Hava soğudukça odun yakıp kampı ısıtmak zorundasın.", false);
                break;

            case TutorialStep.Rebirth_Campfire:
                StartCoroutine(Highlight3DDelayed(campfire3D, campfireMarkerScale));
                StartCoroutine(HighlightUIDelayed(moralUI));
                ShowDialogue("Son olarak, ordunun morali her şeydir. Kamp ateşi etrafında dinlenmelerini sağla ki canları dolsun, moralleri yüksek kalsın.", false);
                break;

            case TutorialStep.Completed:
                CompleteTutorial();
                break;
        }
    }

    private IEnumerator HighlightUIDelayed(GameObject targetUI)
    {
        yield return new WaitForSeconds(0.1f);
        if (TutorialHighlighter.Instance != null && targetUI != null)
        {
            TutorialHighlighter.Instance.HighlightUI(targetUI);
        }
    }

    private IEnumerator Highlight3DDelayed(Transform target3D, float scale)
    {
        yield return new WaitForSeconds(0.2f);
        if (TutorialHighlighter.Instance != null && target3D != null)
        {
            TutorialHighlighter.Instance.Highlight3D(target3D, scale);
        }
    }
private IEnumerator HideDialogueAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        if (TutorialDialogueUI.Instance != null && TutorialDialogueUI.Instance.panelContainer != null)
        {
            TutorialDialogueUI.Instance.panelContainer.SetActive(false);
        }
    }
    public void AdvanceTutorial()
    {
        if (!isTutorialActive) return;
        
        int nextStepIndex = (int)currentStep + 1;
        
        if (nextStepIndex >= Enum.GetValues(typeof(TutorialStep)).Length - 1) 
            SetStep(TutorialStep.Completed);
        else 
            SetStep((TutorialStep)nextStepIndex);
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;
        currentStep = TutorialStep.Completed;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        
        if (TutorialDialogueUI.Instance != null) TutorialDialogueUI.Instance.panelContainer.SetActive(false);
        if (TutorialHighlighter.Instance != null) TutorialHighlighter.Instance.RemoveHighlight();
        
        // Final Kapanış Mesajı
        if (NotificationManager.Instance != null) 
            NotificationManager.Instance.Show("Eğitim Bitti! Artık kampın kontrolü sende Uç Beyim.", NotificationType.Success);
    }

   
    private void ShowDialogue(string text, bool hideContinueBtn, Vector2? offset = null)
    {
        if (TutorialDialogueUI.Instance != null)
        {
            TutorialDialogueUI.Instance.ShowDialogue(text, hideContinueBtn, () => 
            {
                if (!hideContinueBtn) AdvanceTutorial(); 
            }, null, offset);
        }
    }
}