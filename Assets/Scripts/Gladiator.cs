using UnityEngine;
using UnityEngine.AI;
using System;
using TMPro;

public enum SoldierActivity { Training, Working, Idling, Praying, Healing, OnMission }

public class Gladiator : MonoBehaviour
{
    [SerializeField] private JanissaryData _templateData;
    public JanissaryData data { get; private set; }

    [Header("Kişilik Sistemi")]
    public HealthBar healthBar;
    public string candidateName;
    private NavMeshAgent agent;
    public event Action OnStatsChanged;

    [Header("Görsel Ayarlar")]
    public TextMeshProUGUI nameLabel;

    void Awake()
    {
        if (_templateData != null)
        {
            // Şablondan bu askere özel eşsiz bir kopya oluştur
            data = Instantiate(_templateData);
        }
    }

    void Start()
    {
        if (data != null)
        {
            if (string.IsNullOrEmpty(candidateName)) candidateName = data.gladiatorName;

            int staminaMultiplier = (data.trait == SoldierTrait.Obur) ? 15 : 10;
            data.maxHealth    = 100 + (data.stamina * staminaMultiplier) + (data.level * 5);
            data.currentHealth = data.maxHealth;

            if (healthBar != null) healthBar.UpdateBar(data.currentHealth, data.maxHealth);
        }

        agent = GetComponent<NavMeshAgent>();
        UpdateNameLabel();

        // ── KAYIT: Bu asker sahneye girince CampManager listesine ekle ──────
        if (CompareTag("MySoldier") && CampManager.Instance != null)
            CampManager.Instance.RegisterSoldier(this);
        // ────────────────────────────────────────────────────────────────────
    }

    void OnDestroy()
    {
        // ── KAYIT SİLME: Asker yok edilince listeden çıkar ──────────────────
        if (CampManager.Instance != null)
            CampManager.Instance.UnregisterSoldier(this);
        // ────────────────────────────────────────────────────────────────────
    }

    // ── MEVCUT KODUN GERI KALAN HER ŞEYİ AŞAĞIDA — DEĞİŞTİRİLMEDİ ──────────

    public bool IsAvailableForTask()
    {
        if (data.currentHealth <= 0) return false;
        return data.currentActivity == SoldierActivity.Idling;
    }

    public void DismissFromArmy()
    {
        ConfirmationManager.Instance.Show(
            "<color=red>Sürgün Kararı</color>",
            $"<color=yellow>{data.gladiatorName}</color> isimli askeri ordudan atmak istediğine emin misin?\n\n<size=80%>Bu işlem geri alınamaz ve kampta ufak bir moral düşüşüne sebep olur.</size>",
            () =>
            {
                SetIdle();

                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show($"{data.gladiatorName} ordudan atıldı! Kampta huzursuzluk var.", NotificationType.Error);

                if (TrainingUIManager.Instance != null)
                    TrainingUIManager.Instance.SetCurrentGladiator(null);

                // Tag sıfırla → TopBar sayarken görmez
                gameObject.tag = "Untagged";

                // UnregisterSoldier OnDestroy'da çağrılacak, ama tag
                // değiştiğinden önce manuel çağır ki liste anında temizlensin
                if (CampManager.Instance != null)
                    CampManager.Instance.UnregisterSoldier(this);

                if (TopInfoBarUI.Instance != null)
                    TopInfoBarUI.Instance.UpdateCapacity();

                Destroy(gameObject);
            }
        );
    }

    public void RecalculateMaxHealth()
    {
        if (data == null) return;

        float oldMaxHealth = data.maxHealth;

        int staminaMultiplier = (data.trait == SoldierTrait.Obur) ? 15 : 10;
        data.maxHealth = 100 + (data.stamina * staminaMultiplier) + (data.level * 5);

        float difference = data.maxHealth - oldMaxHealth;
        if (difference > 0) data.currentHealth += difference;

        data.currentHealth = Mathf.Min(data.currentHealth, data.maxHealth);

        if (healthBar != null)
            healthBar.UpdateBar(data.currentHealth, data.maxHealth);
    }

    public void SetActivity(SoldierActivity newActivity)
    {
        if (data != null) data.currentActivity = newActivity;
    }

    public void InitializeData(JanissaryData sourceData)
    {
        data = Instantiate(sourceData);
    }

    // ── Kısa yollar ──────────────────────────────────────────────────────────

    public float currentHealth
    {
        get => data != null ? data.currentHealth : 0;
        set { if (data != null) data.currentHealth = value; }
    }

    public float maxHealth
    {
        get => data != null ? data.maxHealth : 1;
        set { if (data != null) data.maxHealth = value; }
    }

    public bool isOnMission
    {
        get => data != null && data.isOnMission;
        set { if (data != null) data.isOnMission = value; }
    }

    public bool IsAvailable
    {
        get
        {
            var training = GetComponent<GladiatorTraining>();
            bool isTraining = training != null && training.IsTraining;
            return !isOnMission && !isTraining;
        }
    }

    public void SetIdle()
    {
        if (data != null)
            data.currentActivity = SoldierActivity.Idling;

        isOnMission = false;
        RefreshStats();
    }

    public void RefreshStats()
    {
        OnStatsChanged?.Invoke();
        if (healthBar != null && data != null)
            healthBar.UpdateBar(data.currentHealth, data.maxHealth);
    }

    public void UpdateNameLabel()
    {
        if (nameLabel != null && data != null)
            nameLabel.text = data.gladiatorName;
    }

    public void GoTo(Vector3 target)
    {
        if (agent != null) agent.SetDestination(target);
    }
}
