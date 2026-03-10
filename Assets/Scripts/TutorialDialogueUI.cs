using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;

public class TutorialDialogueUI : MonoBehaviour
{
    public static TutorialDialogueUI Instance;

    [Header("UI Elemanları")]
    public GameObject panelContainer; 
    public TextMeshProUGUI dialogueText;
    
    [Header("Yeni: Görünmez Buton ve İşaretçi")]
    public Button fullScreenButton; // Ekranı kaplayan şeffaf buton
    public GameObject continueIndicator; // Yazı bitince çıkacak zıplayan ok (▼)

    [Header("Karakter (Opsiyonel)")]
    public Image characterPortrait; 
    public Sprite defaultCharacterSprite; 

    [Header("Daktilo Ayarları")]
    public float typeSpeed = 0.03f; 

    private Coroutine typingCoroutine;
    private Coroutine bounceCoroutine; // Zıplama animasyonunu tutan döngü
    private string fullText;
    private bool isTyping = false;
    private Action onDialogueFinished;
    
    private Vector3 indicatorStartPos;

    void Awake()
    {
        Instance = this;
        if (panelContainer != null) panelContainer.SetActive(false);
        if (fullScreenButton != null) fullScreenButton.onClick.AddListener(OnContinueClicked);
        
        // Okun başlangıç pozisyonunu hafızaya al (zıplama için lazım)
        if (continueIndicator != null) 
        {
            continueIndicator.SetActive(false);
            indicatorStartPos = continueIndicator.GetComponent<RectTransform>().anchoredPosition;
        }
    }

    public void ShowDialogue(string text, bool hideContinueBtn = false, Action onComplete = null, Sprite speakerSprite = null)
    {
        panelContainer.SetActive(true);
        fullText = text;
        onDialogueFinished = onComplete;
        
        // Eğer oyuncunun mecburen bir binaya/node'a tıklaması gerekiyorsa, ekrana tıklamayı tamamen kapat!
        if (fullScreenButton != null) 
            fullScreenButton.gameObject.SetActive(!hideContinueBtn); 
            
        if (characterPortrait != null)
            characterPortrait.sprite = speakerSprite != null ? speakerSprite : defaultCharacterSprite;
        
        // Yeni yazı başlarken oku gizle ve zıplamayı durdur
        if (continueIndicator != null) continueIndicator.SetActive(false);
        if (bounceCoroutine != null) StopCoroutine(bounceCoroutine);
        
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeTextRoutine());
    }

    private IEnumerator TypeTextRoutine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in fullText.ToCharArray())
        {
            dialogueText.text += c;
            // AudioManager.Instance.PlayTypewriterSound(); // Daktilo sesi
            yield return new WaitForSecondsRealtime(typeSpeed); 
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        isTyping = false;
        dialogueText.text = fullText;
        
        // Yazı akması bittiyse ve bu aşamada "Devam Etme" hakkımız varsa oku göster!
        if (continueIndicator != null && fullScreenButton.gameObject.activeSelf) 
        {
            continueIndicator.SetActive(true);
            bounceCoroutine = StartCoroutine(BounceIndicatorRoutine());
        }
    }

    // --- YENİ: KOD İLE ZIPLAMA ANİMASYONU (SİNÜS DALGASI) ---
    private IEnumerator BounceIndicatorRoutine()
    {
        RectTransform rect = continueIndicator.GetComponent<RectTransform>();
        float speed = 5f;   // Zıplama hızı
        float height = 4f;  // Yukarı aşağı kaç piksel oynayacağı

        while (true)
        {
            // Zaman aktıkça Y ekseninde yumuşak bir in-çık dalgası yaratır
            float newY = indicatorStartPos.y + Mathf.Sin(Time.unscaledTime * speed) * height;
            rect.anchoredPosition = new Vector2(indicatorStartPos.x, newY);
            yield return null;
        }
    }

    public void OnContinueClicked()
    {
        if (isTyping)
        {
            // Eğer yazı bitmediyse ve ekrana tıklandıysa, yazıyı anında şak diye tamamla
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            FinishTyping();
        }
        else
        {
            // Yazı çoktan bittiyse paneli kapat ve sıradaki aşamaya geç
            panelContainer.SetActive(false);
            if (bounceCoroutine != null) StopCoroutine(bounceCoroutine);
            onDialogueFinished?.Invoke(); 
        }
    }
}