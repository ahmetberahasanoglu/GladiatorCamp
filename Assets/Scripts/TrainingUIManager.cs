using UnityEngine;

public class TrainingUIManager : MonoBehaviour
{
    public static TrainingUIManager Instance;

    public GladiatorTraining current; // Şu an seçili gladyatörün referansı

    void Awake()
    {
        Instance = this;
    }

    // Karakter seçildiğinde bu fonksiyon çağrılır
    public void SetCurrentGladiator(GladiatorTraining gladiator)
    {
       if (current != null)
        {
            current.SetSelectedVisual(false);
        }

        // 2. Yeni seçilen askeri kaydet
        current = gladiator;

        // 3. Eğer yeni seçilen kişi boş (null) değilse, onun ışığını AÇ
        if (current != null)
        {
            current.SetSelectedVisual(true);
        }
    }
}