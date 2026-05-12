using UnityEngine;

public class BeastAnimEvents : MonoBehaviour
{
    private GladiatorAI _ai;

    void Awake()
    {
        _ai = GetComponentInParent<GladiatorAI>();
    }


    public void PlaySwoosh()
    {
        if (AudioManager.Instance != null) 
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.bearHit, 0.9f);
        }
    }


    public void DealDamage()
    {
        if (AudioManager.Instance != null)
        {
   
            AudioManager.Instance.PlaySFX(AudioManager.Instance.beastHit, 0.9f);
        }

        if (_ai != null) 
            _ai.ExecuteAttackEvent(); 
    }

    public void PlayRoar()
    {
        if (AudioManager.Instance != null)
        {
             AudioManager.Instance.PlaySFX(AudioManager.Instance.bearRoar, 1.0f);
        }
    }
}