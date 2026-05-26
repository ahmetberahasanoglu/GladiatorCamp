using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Kamp ateşi:
///   - Günü bitirirken yanıksa +5 moral, söndürülmüşse -3 moral verir.
///   - Yakma maliyeti: woodCostPerDay kadar odun.
///   - passiveHealPerDay > 0 ise, ateş yanıkken her gün tüm askerlere küçük
///     bir can yeniler (şifacıyla çakışmaması için çok düşük tutulmalı).
/// </summary>
public class CampfireInteract : MonoBehaviour
{
    [Header("Ateş Ayarları")]
    public ParticleSystem sparksParticle;

    [Header("Odun Maliyeti")]
    [Tooltip("Ateşi yakmak için gereken odun miktarı")]
    public int woodCostPerDay = 3;

    [Header("Günlük Moral Etkisi")]
    [Tooltip("Ateş YANIYOR iken gün sonunda verilecek moral")]
    public int moraleIfBurning  =  5;
    [Tooltip("Ateş SÖNÜK iken gün sonunda uygulanacak moral cezası")]
    public int moraleIfExtinguished = -3;

    [Header("Pasif İyileşme (İsteğe Bağlı)")]
    [Tooltip("0 bırakırsan pasif iyileşme olmaz; şifacıyla çakışmaz.")]
    public int passiveHealPerDay = 0;

    [Header("Sesler")]
    public AudioSource flareSound;
    public AudioClip extinguishSound;

    [Header("Döngü Sesleri")]
    public AudioSource campfireLoopA;
    public AudioSource campfireLoopB;
    public float crossfadeTime = 0.3f;

    private AudioSource activeLoop;
    private AudioSource nextLoop;
    private Coroutine loopCoroutine;

    public bool IsBurning { get; private set; } = true;

    private bool isOnCooldown = false;

    void Start()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay += OnNewDay;
        }

        if (IsBurning)
        {
            if (sparksParticle != null) sparksParticle.Play();
            StartLoop();
        }
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay -= OnNewDay;
    }

    // ── Her gün bitişinde tetiklenir ─────────────────────────────────────
    void OnNewDay()
    {
        if (IsBurning)
        {
            // Moral bonusu
            CampMoraleManager.Instance?.ChangeMorale(moraleIfBurning);

            // Pasif iyileşme (şifacı yoksa hafif bir destek)
            if (passiveHealPerDay > 0)
                HealAllSoldiersPassive(passiveHealPerDay);

            // Sonraki gün için ateş söner — oyuncu tekrar yakmalı
            ExtinguishFire(silent: true);
        }
        else
        {
            // Ateş sönükse moral cezası
            CampMoraleManager.Instance?.ChangeMorale(moraleIfExtinguished);
            NotificationManager.Instance?.Show(
                "Kamp ateşi sönük geçti. Askerler üşüdü.",
                NotificationType.Warning);
        }
    }

    // ── Tıklama ──────────────────────────────────────────────────────────
    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject() || isOnCooldown) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;

        StartCoroutine(ToggleFireRoutine());
    }

    IEnumerator ToggleFireRoutine()
    {
        isOnCooldown = true;

        if (IsBurning)
        {
            ExtinguishFire(silent: false);
        }
        else
        {
            TryIgniteFire();
        }

        yield return new WaitForSeconds(0.5f);
        isOnCooldown = false;
    }

    // ── Yakma ────────────────────────────────────────────────────────────
    void TryIgniteFire()
    {
        // Odun kontrolü
        if (ResourceManager.Instance == null || ResourceManager.Instance.wood < woodCostPerDay)
        {
            NotificationManager.Instance?.Show(
                $"Ateş yakmak için {woodCostPerDay} odun gerekiyor!",
                NotificationType.Error);
            return;
        }

        // CampSurvivalManager varsa ona da danış (sıcaklık sistemi)
        if (CampSurvivalManager.Instance != null)
        {
            bool success = CampSurvivalManager.Instance.StokeFire();
            if (!success) return;
        }

        // Odunu harca
        ResourceManager.Instance.SpendWood(woodCostPerDay);

        IsBurning = true;
        if (sparksParticle != null) sparksParticle.Play();
        if (flareSound != null) flareSound.Play();
        StartLoop();

        NotificationManager.Instance?.Show(
            $"Kamp ateşi yakıldı. (-{woodCostPerDay} Odun) Sabaha kadar ısınıyorsunuz.",
            NotificationType.Success);
    }

    // ── Söndürme ─────────────────────────────────────────────────────────
    void ExtinguishFire(bool silent)
    {
        IsBurning = false;
        if (sparksParticle != null) sparksParticle.Stop();
        StopFireLoops();

        if (!silent)
        {
            if (extinguishSound != null && flareSound != null)
                flareSound.PlayOneShot(extinguishSound);

            NotificationManager.Instance?.Show(
                "Ateşi söndürdün. Sabah soğuk olacak.",
                NotificationType.Warning);
        }
    }

    // ── Pasif İyileşme ────────────────────────────────────────────────────
    void HealAllSoldiersPassive(int amount)
    {
        // CampManager cache kullanıyorsa oradan al
        if (CampManager.Instance != null)
        {
            foreach (var soldier in CampManager.Instance.GetLivingSoldiers())
            {
                // Sadece tamamen ölü olmayan, şifahanede OLMAYAN askerlere uygula
                var healing = soldier.GetComponent<GladiatorHealing>();
                if (healing != null && healing.IsHealing) continue; // Şifacıyla çakışma yok

                soldier.currentHealth = Mathf.Min(
                    soldier.currentHealth + amount,
                    soldier.maxHealth);
                soldier.RefreshStats();
            }
        }
    }

    // ── Ses Döngüsü ──────────────────────────────────────────────────────
    void StartLoop()
    {
        if (campfireLoopA == null || campfireLoopB == null) return;
        if (loopCoroutine != null) StopCoroutine(loopCoroutine);

        activeLoop = campfireLoopA;
        nextLoop   = campfireLoopB;

        activeLoop.volume = 1f;
        nextLoop.volume   = 0f;
        activeLoop.Play();
        nextLoop.Stop();

        loopCoroutine = StartCoroutine(LoopRoutine());
    }

    void StopFireLoops()
    {
        if (loopCoroutine != null) { StopCoroutine(loopCoroutine); loopCoroutine = null; }
        campfireLoopA?.Stop();
        campfireLoopB?.Stop();
    }

    IEnumerator LoopRoutine()
    {
        while (IsBurning)
        {
            if (activeLoop.clip == null) yield break;

            float waitTime = activeLoop.clip.length - crossfadeTime;
            if (waitTime > 0) yield return new WaitForSeconds(waitTime);
            if (!IsBurning) yield break;

            nextLoop.volume = 0f;
            nextLoop.Play();

            float t = 0f;
            while (t < crossfadeTime && IsBurning)
            {
                t += Time.deltaTime;
                float n = t / crossfadeTime;
                activeLoop.volume = 1f - n;
                nextLoop.volume   = n;
                yield return null;
            }

            activeLoop.Stop();
            (activeLoop, nextLoop) = (nextLoop, activeLoop);
        }
    }
}
