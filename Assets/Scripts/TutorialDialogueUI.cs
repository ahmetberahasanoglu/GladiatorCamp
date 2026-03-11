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
    public Button fullScreenButton; 
    public GameObject continueIndicator; 

    [Header("Balon Ayarı (YENİ)")]
    public RectTransform speechBubbleRect;
    public RectTransform speechcharRect; 
    private Vector2 defaultBubblePos;     
    private Vector2 defaultCharPos;      

    [Header("Karakter (Opsiyonel)")]
    public Image characterPortrait; 
    public Sprite defaultCharacterSprite; 

    [Header("Daktilo Ayarları")]
    public float typeSpeed = 0.03f; 

    private Coroutine typingCoroutine;
    private Coroutine bounceCoroutine;
    private string fullText;
    private bool isTyping = false;
    private Action onDialogueFinished;
    private Vector3 indicatorStartPos;

    void Awake()
    {
        Instance = this;
        if (panelContainer != null) panelContainer.SetActive(false);
        if (fullScreenButton != null) fullScreenButton.onClick.AddListener(OnContinueClicked);
        
        if (continueIndicator != null) 
        {
            continueIndicator.SetActive(false);
            indicatorStartPos = continueIndicator.GetComponent<RectTransform>().anchoredPosition;
        }
        if(speechcharRect!=null)
        defaultCharPos= speechcharRect.anchoredPosition;
        // YENİ: Balonun ilk (orijinal) pozisyonunu hafızaya al
        if (speechBubbleRect != null)
        {
            defaultBubblePos = speechBubbleRect.anchoredPosition;
        }
      
    }

    // YENİ: Fonksiyona "bubbleOffset" (Kaydırma Miktarı) eklendi
    public void ShowDialogue(string text, bool hideContinueBtn = false, Action onComplete = null, Sprite speakerSprite = null, Vector2? bubbleOffset = null)
    {
        panelContainer.SetActive(true);
        fullText = text;
        onDialogueFinished = onComplete;
        
        if (fullScreenButton != null) 
            fullScreenButton.gameObject.SetActive(!hideContinueBtn); 
            
        if (characterPortrait != null)
            characterPortrait.sprite = speakerSprite != null ? speakerSprite : defaultCharacterSprite;
        
        // YENİ: Eğer dışarıdan bir kaydırma (Offset) istendiyse balonu kaydır, istenmediyse orijinal yerine koy
        if (speechBubbleRect != null)
        {
            speechBubbleRect.anchoredPosition = bubbleOffset.HasValue ? defaultBubblePos + bubbleOffset.Value : defaultBubblePos;
        }
         speechcharRect.anchoredPosition = bubbleOffset.HasValue ? defaultCharPos + bubbleOffset.Value : defaultCharPos;
        if (continueIndicator != null) continueIndicator.SetActive(false);
        if (bounceCoroutine != null) StopCoroutine(bounceCoroutine);
        
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeTextRoutine());
    }

    // ... (TypeTextRoutine, FinishTyping, BounceIndicatorRoutine ve OnContinueClicked fonksiyonları öncekiyle tamamen aynı kalacak, onlara dokunma) ...

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