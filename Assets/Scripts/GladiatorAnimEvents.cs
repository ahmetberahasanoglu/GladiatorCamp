using UnityEngine;

public class GladiatorAnimEvents : MonoBehaviour
{
    private GladiatorAI _ai;

    void Awake()
    {
        _ai = GetComponentInParent<GladiatorAI>();
    }

  
    public void PlaySwoosh()
    {
        if (AudioManager.Instance != null) 
            AudioManager.Instance.PlaySFX(AudioManager.Instance.swordSwoosh, 0.8f);
    }

    // Animasyonda kılıcdüşmana çarptığı an tetiklenecek
    public void DealDamage()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.swordHit, 0.8f);
        if (_ai != null) 
            _ai.ExecuteAttackEvent(); 
    }
}