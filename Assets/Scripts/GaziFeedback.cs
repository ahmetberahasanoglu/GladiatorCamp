using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Bir asker Gazi unvanı aldığında gösterilen özel tören ekranı.
/// GladiatorAI.MakeGazi() bu sınıfı tetikler.
/// </summary>
public class GaziFeedback : MonoBehaviour
{
    public static GaziFeedback Instance;

    [Header("Panel")]
    public GameObject gaziPanel;
    public CanvasGroup canvasGroup;         // Fade in/out için

    [Header("İçerik")]
    public TextMeshProUGUI soldierNameText; // "HAMZA"
    public TextMeshProUGUI titleText;       // "artık bir GAZİ!"
    public TextMeshProUGUI bonusText;       // "+2 Seviye · +Moral · Kamp İtibarı arttı"
    public TextMeshProUGUI flavorText;      // Rastgele atmosferik cümle
    public Image           soldierPortrait; // Opsiyonel — askerin görseli

    [Header("Animasyon")]
    public float fadeInTime  = 0.4f;
    public float holdTime    = 2.8f;
    public float fadeOutTime = 0.6f;



    private static readonly string[] FlavorLines =
    {
        "Kılıcı kandan, kalbi şükrandan doludur.",
        "Ölümün gözüne baktı, geri döndü.",
        "Adı artık sadece bir ad değil, bir unvan.",
        "Rüzgar bile yolundan çekilir şimdi.",
        "Bu yara izi, en güzel madalyadır.",
        "Ocak söndü mü? Hayır. Gazi var, ocak tütüyor.",
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gaziPanel != null) gaziPanel.SetActive(false);
    }

    /// <summary>GladiatorAI.MakeGazi() burayı çağırır.</summary>
    public void ShowGaziCeremony(Gladiator gladiator)
    {
        StartCoroutine(GaziRoutine(gladiator));
    }

    private IEnumerator GaziRoutine(Gladiator gladiator)
    {
        // Zaman dondur (opsiyonel — savaş bitişinde zaten duruyor)
        // Time.timeScale = 0f;

        gaziPanel.SetActive(true);

        // İçerik doldur
        if (soldierNameText != null)
            soldierNameText.text = gladiator.data.gladiatorName.ToUpper();

        if (titleText != null)
            titleText.text = "artık bir <color=#FFD700>GAZİ</color>!";

        if (bonusText != null)
            bonusText.text = $"+2 Seviye  ·  +{gladiator.data.level} Max Can  ·  Kamp Morali +5";

        if (flavorText != null)
            flavorText.text = FlavorLines[Random.Range(0, FlavorLines.Length)];

        if ( AudioManager.Instance != null)
            AudioManager.Instance.PlayGazi();

        // Kamp morali bonus
        if (CampMoraleManager.Instance != null)
            CampMoraleManager.Instance.ChangeMorale(5);

        // Fade in
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float t = 0f;
            while (t < fadeInTime)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeInTime);
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(holdTime);

        // Fade out
        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < fadeOutTime)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeOutTime);
                yield return null;
            }
        }

        gaziPanel.SetActive(false);
        // Time.timeScale = 1f;
    }
}