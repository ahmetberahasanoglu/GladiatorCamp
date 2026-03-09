using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

// Eğitim aşamalarımızı tanımlayan Enum listesi
public enum TutorialStep
{
    None,               // Eğitim bitti veya yok
    Intro_Map,          // Adım 1: Haritaya ilk bakış ve Kampa tıklama zorunluluğu
    Camp_Tour,          // Adım 2: Kampa giriş, kaynakların (Altın, Odun, Nasip vb.) tanıtımı
    Camp_GoToWar,       // Adım 3: Savaş masasına/haritaya tıklayıp ilk sefere çıkış
    Battle_ScriptedLoss,// Adım 4: Haritadaki tek açık node'a tıklama ve kaybedilecek savaş
    Camp_Rebirth,       // Adım 5: Kampa dönüş, bozgun sonrası Demirci, Talimhane ve Cami tanıtımı
    Completed           // Eğitim Tamamlandı (Serbest Oyun)
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Eğitim Durumu")]
    public TutorialStep currentStep = TutorialStep.None;
    public bool isTutorialActive = false;

    // Şimdilik sadece konsola veya basit bir UI textine basacağız, 
    // bir sonraki adımda o sinematik diyalog panelini buraya bağlayacağız.
    [Header("Görsel/UI Referansları (Şimdilik Boş Bırakılabilir)")]
    public GameObject tutorialDialoguePanel;
    public TextMeshProUGUI tutorialText;
    public Button nextButton;

    void Awake()
    {
        // Singleton yapısı
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Harita ve Kamp sahneleri arasında silinmesin
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Oyuncu daha önce eğitimi tamamlamış mı kontrol et
        int tutFinished = PlayerPrefs.GetInt("TutorialCompleted", 0);
        
        if (tutFinished == 0)
        {
            // Eğitim tamamlanmamış, baştan başlat!
            StartTutorial();
        }
    }

    public void StartTutorial()
    {
        isTutorialActive = true;
        SetStep(TutorialStep.Intro_Map);
    }

    // Aşamayı değiştiren ve o aşamaya özel olayları tetikleyen ana fonksiyon
    public void SetStep(TutorialStep newStep)
    {
        currentStep = newStep;
        Debug.Log($"<color=cyan>TUTORIAL ADIMI: {currentStep}</color>");

        switch (currentStep)
        {
            case TutorialStep.Intro_Map:
                // TODO: Haritadaki tüm node'ları kilitle, sadece Kamp Node'unu açık ve parlak yap.
                ShowDialogue("Uç Beyim, sınır boylarına hoş geldin. Kış bastırmadan, 100 gün içinde şu lanet Kızıl Kale'yi düşürmeliyiz. Önce otağımızı kuralım. (Kampa Tıkla)");
                break;

            case TutorialStep.Camp_Tour:
                // TODO: Kamp açıldığında TopBar'ı maskele, kaynakları parlatarak anlat.
                ShowDialogue("Burası merkezimiz. Yukarıda Altınımız, Erzağımız ve Askerlerimizin durumu var. Savaşmak için paraya ve yemeğe ihtiyacımız olacak.");
                break;

            case TutorialStep.Camp_GoToWar:
                // TODO: Kampın ortasındaki savaş masasını parlat.
                ShowDialogue("Şimdi gücümüzü sınama vakti. Harita masasına tıkla ve ilk çapulcu grubunun üzerine yürüyelim.");
                break;

            case TutorialStep.Battle_ScriptedLoss:
                // TODO: Haritada sadece savaş node'unu açık tut. Savaşı garantili kaybettir.
                ShowDialogue("İzleri burada bitiyor... Kılıçları çekin!");
                break;

            case TutorialStep.Camp_Rebirth:
                // TODO: Savaştan yenik dönüldü. Demirciyi, Talimhaneyi ve Camiyi sırayla parlat.
                ShowDialogue("Ağır bir yara aldık Beyim... Kuru cesaretle Kızıl Kale alınmazmış. Kılıçlarımız kör, yüreklerimiz daralmış. Demirciyi onar, askerleri talime sok ve sefere çıkmadan önce camide Nasibimizi ara.");
                break;

            case TutorialStep.Completed:
                CompleteTutorial();
                break;
        }
    }

    // Eğitimi bir sonraki aşamaya geçirmek için dışarıdan (butonlardan vs.) çağrılır
    public void AdvanceTutorial()
    {
        if (!isTutorialActive) return;

        int nextStepIndex = (int)currentStep + 1;
        
        if (nextStepIndex >= Enum.GetValues(typeof(TutorialStep)).Length - 1)
        {
            SetStep(TutorialStep.Completed);
        }
        else
        {
            SetStep((TutorialStep)nextStepIndex);
        }
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;
        currentStep = TutorialStep.Completed;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        
        if (tutorialDialoguePanel != null) tutorialDialoguePanel.SetActive(false);
        Debug.Log("<color=green>EĞİTİM TAMAMLANDI! Serbest oyun başladı.</color>");
    }

    // Şimdilik test amaçlı, ileride sinematik panelimizi açacak
    private void ShowDialogue(string text)
    {
        if (tutorialDialoguePanel != null && tutorialText != null)
        {
            tutorialDialoguePanel.SetActive(true);
            tutorialText.text = text;
        }
        else
        {
            Debug.LogWarning($"DİYALOG: {text}");
        }
    }
}