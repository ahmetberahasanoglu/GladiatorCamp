using UnityEngine;

public class GladiatorAnimEvents : MonoBehaviour
{
    private GladiatorAI _ai;
    private Gladiator _gladiator; // Askerin verilerine (data) ulaşmak için

    void Awake()
    {
        _ai = GetComponentInParent<GladiatorAI>();
        _gladiator = GetComponentInParent<Gladiator>();
    }

    // Animasyonda silahı savurduğu an tetiklenecek
    public void PlaySwoosh()
    {
        if (_gladiator == null || _gladiator.data == null || AudioManager.Instance == null) return;

        // Silah türüne göre savrulma (swoosh) sesi seçimi
        switch (_gladiator.data.weaponClass)
        {
            case WeaponClass.Sword:
                AudioManager.Instance.PlaySFX(AudioManager.Instance.swordSwoosh, 0.8f);
                break;
            case WeaponClass.Axe:
                // AudioManager.Instance.PlaySFX(AudioManager.Instance.axeSwoosh, 0.8f);
                 AudioManager.Instance.PlaySFX(AudioManager.Instance.swordSwoosh, 0.8f);
                break;
            case WeaponClass.Spear:
                // AudioManager.Instance.PlaySFX(AudioManager.Instance.spearSwoosh, 0.8f);
                 AudioManager.Instance.PlaySFX(AudioManager.Instance.swordSwoosh, 0.8f);
                break;
            case WeaponClass.Bow:
                // Ok fırlama sesi (Swoosh yerine okun havayı yarma sesi)
                // AudioManager.Instance.PlaySFX(AudioManager.Instance.bowTwang, 0.8f);
                 AudioManager.Instance.PlaySFX(AudioManager.Instance.swordSwoosh, 0.8f);
                break;
            case WeaponClass.Unarmed:
                // Yumruk atma sesi
                // AudioManager.Instance.PlaySFX(AudioManager.Instance.punchSwoosh, 0.8f);
                 AudioManager.Instance.PlaySFX(AudioManager.Instance.swordSwoosh, 0.8f);
                break;
        }
    }

    // Animasyonda kılıcın/silahın düşmana çarptığı an tetiklenecek
    public void DealDamage()
    {
        if (_gladiator != null && _gladiator.data != null && AudioManager.Instance != null)
        {
            // ÖNEMLİ: Yay (Ok) gibi menzilli silahlarda "çarpma" sesi burada ÇALMAZ! 
            // Çünkü ok havada uçacaktır, çarpma sesi ok hedefe değdiğinde Projectile.cs içinden çalmalıdır.
            if (!_gladiator.data.isRanged)
            {
                switch (_gladiator.data.weaponClass)
                {
                    case WeaponClass.Sword:
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.swordHit, 0.8f);
                        break;
                    case WeaponClass.Axe:
                        // Balta çarpma (daha tok, kemik kırma sesi)
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.swordHit, 0.8f);
                        // AudioManager.Instance.PlaySFX(AudioManager.Instance.axeHit, 0.8f);
                        break;
                    case WeaponClass.Spear:
                        // Mızrak saplanma sesi
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.swordHit, 0.8f);
                        // AudioManager.Instance.PlaySFX(AudioManager.Instance.spearHit, 0.8f);
                        break;
                    case WeaponClass.Unarmed:
                        // Yumruk sesi
                        // AudioManager.Instance.PlaySFX(AudioManager.Instance.punchHit, 0.8f);
                        break;
                }
            }
        }
        
        if (_ai != null) 
            _ai.ExecuteAttackEvent(); 
    }
}