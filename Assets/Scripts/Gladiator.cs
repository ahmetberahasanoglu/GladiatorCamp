using UnityEngine;
using UnityEngine.AI;
using System;
using TMPro;

public enum SoldierActivity { Training, Working, Idling,Praying,Healing,OnMission }

public class Gladiator : MonoBehaviour
{
    [SerializeField] private JanissaryData _templateData;
    public JanissaryData data { get; private set; }

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
            // Şablondan bu askere özel eşsiz bir kopya oluşturuyoruz
            data = Instantiate(_templateData);
        }
    }

    void Start()
    {
        if (data != null)
        {
            if (string.IsNullOrEmpty(candidateName)) candidateName = data.gladiatorName;

            // --- CAN HESABI ARTIK DATA'NIN İÇİNDE ---
            data.maxHealth = (data.stamina * 10) + (data.level * 5);
            
            // Eğer yeni üretilen bir askerse canını fullüyoruz
            // (İleride Save'den yüklüyorsak bu satırı atlayacağız ki yaralı kalsın)
            data.currentHealth = data.maxHealth; 

            if (healthBar != null) healthBar.UpdateBar(data.currentHealth, data.maxHealth);
        }
      
        agent = GetComponent<NavMeshAgent>();
        UpdateNameLabel();
    }
public bool IsAvailableForTask()
{
    // Asker ölüyse zaten müsait değildir
    if (data.currentHealth <= 0) return false;

    // SADECE BOŞTA (Idle) ise yeni bir işe gönderilebilir!
    return data.currentActivity == SoldierActivity.Idling;
}

// İş atama ve bitirme için yardımcı bir fonksiyon (Yine Gladiator.cs içine):
  public void SetActivity(SoldierActivity newActivity)
    {
        if (data != null) data.currentActivity = newActivity;
    }

    public void InitializeData(JanissaryData sourceData)
    {
        data = Instantiate(sourceData);
        // İleride Save sisteminden veri yüklerken bu fonksiyonu kullanacağız
    }

    // Kısa yollar (Böylece diğer scriptlerdeki kodları tek tek değiştirmene gerek kalmaz)
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

    public void RefreshStats()
    {
        OnStatsChanged?.Invoke();
        if (healthBar != null && data != null) 
        {
            healthBar.UpdateBar(data.currentHealth, data.maxHealth);
        }
    }
/*
    public void DecreaseMorale(int amount)
    {
        data.morale -= amount;
        if (data.morale < 0) data.morale = 0;

        RefreshStats(); 
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.Show($"{data.gladiatorName} moral kaybetti. Yeni Moral: {data.morale}", NotificationType.Error);
        }
    }
*/
    public void UpdateNameLabel()
    {
        if (nameLabel != null && data != null)
        {
            nameLabel.text = data.gladiatorName; 
        }
    }

    public void GoTo(Vector3 target)
    {
        if (agent != null) agent.SetDestination(target);
    }
}