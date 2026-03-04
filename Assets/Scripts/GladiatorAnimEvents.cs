using UnityEngine;

public class GladiatorAnimEvents : MonoBehaviour
{
    private GladiatorAI _ai;

    void Awake()
    {
        // Kendisi Child objede olduğu için, Parent'taki ana yapay zeka kodunu bulur
        _ai = GetComponentInParent<GladiatorAI>();
    }

    // Animasyonda kılıcın savrulmaya başladığı an tetiklenecek
    public void PlaySwoosh()
    {
        if (AudioManager.Instance != null) 
            AudioManager.Instance.PlaySFX(AudioManager.Instance.swordSwoosh, 0.8f);
    }

    // Animasyonda kılıcın düşmana tam "GÜM" diye çarptığı an tetiklenecek
    public void DealDamage()
    {
        if (_ai != null) 
            _ai.ExecuteMeleeHit(); // Ana koda "VUR" emri gönderir
    }
}