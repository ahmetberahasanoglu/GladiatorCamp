using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;

public class TutorialDialogueUI : MonoBehaviour
{
    public static TutorialDialogueUI Instance;

    [Header("UI Elemanları")]
    public GameObject panelContainer; // Panel objesinin kendisi
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public TextMeshProUGUI buttonText; // Butonun içindeki Text objesi

    [Header("Daktilo Ayarları")]
    public float typeSpeed = 0.03f; // Harflerin ekrana gelme hızı

    private Coroutine typingCoroutine;
    private string fullText;
    private bool isTyping = false;
    private Action onDialogueFinished; // Diyalog bitince çalışacak komut

    void Awake()
    {
        Instance = this;
        if (panelContainer != null) panelContainer.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
    }

   
    public void ShowDialogue(string text, bool hideContinueBtn = false, Action onComplete = null)
    {
        panelContainer.SetActive(true);
        fullText = text;
        onDialogueFinished = onComplete;
        
        // --- YENİ: Eğitimin o anki durumuna göre butonu gizle veya göster ---
        if (continueButton != null) 
            continueButton.gameObject.SetActive(!hideContinueBtn); 
        
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeTextRoutine());
    }

    private IEnumerator TypeTextRoutine()
    {
        isTyping = true;
        dialogueText.text = "";
        buttonText.text = "Geç (>>)"; // Yazı akarken butonun metni değişir

        foreach (char c in fullText.ToCharArray())
        {
            dialogueText.text += c;
            
            // Eğer daktilo veya tüy kalem sesin varsa buraya ekleyebilirsin:
            // AudioManager.Instance.PlayTypewriterSound(); 
            
            // Time.timeScale 0 olsa bile yazının akması için Realtime kullanıyoruz
            yield return new WaitForSecondsRealtime(typeSpeed); 
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        isTyping = false;
        dialogueText.text = fullText;
        buttonText.text = "Devam Et";
    }

    // Butona tıklandığında çalışacak mantık
    public void OnContinueClicked()
    {
        if (isTyping)
        {
            // Eğer yazı hala yazılıyorsa, yazmayı kes ve metnin tamamını zınk diye ekrana bas (Skip)
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            FinishTyping();
        }
        else
        {
            // Yazı çoktan bittiyse, paneli kapat ve TutorialManager'a "Sıradaki adıma geç" mesajı yolla
            panelContainer.SetActive(false);
            
            // Eğer içine bir fonksiyon (Action) koyulmuşsa onu çalıştır
            onDialogueFinished?.Invoke(); 
        }
    }
}