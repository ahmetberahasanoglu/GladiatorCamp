using UnityEngine;
using UnityEngine.AI;
using System;
using TMPro;
public enum SoldierActivity { Training, Working, Idling }
public class Gladiator : MonoBehaviour
{
    [SerializeField] private JanissaryData _templateData;
    public JanissaryData data {get;set;}

    public HealthBar healthBar;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float maxHealth;
    public string candidateName;
    private NavMeshAgent agent;
    public event Action OnStatsChanged;
    public bool isOnMission = false;
    [Header("Çalışma Durumu")]
    public SoldierActivity currentActivity = SoldierActivity.Idling; // Varsayılan: Talim
    public int dailyWage = 50; // Bu askerin günlük kazandırdığı para (Seviyesine göre artırılabilir


    [Header("Görsel Ayarlar")]
    public TextMeshProUGUI nameLabel;
    void Awake()
    {
        if (_templateData != null)
        {
            data = Instantiate(_templateData);
        }
    }
    public void SetActivity(SoldierActivity newActivity)
    {
        currentActivity = newActivity;
    }
    public void InitializeData(JanissaryData sourceData)
    {
        data = Instantiate(sourceData);
    }
    public bool IsAvailable 
    {
        get 
        {
            var training = GetComponent<GladiatorTraining>();
            bool isTraining = training != null && training.IsTraining;// && currentActivity.Training

            return !isOnMission && !isTraining;
        }
    }   
    public bool IsAvailableForWork()
    {    
        return true; 
    }
    public void RefreshStats()
    {
        OnStatsChanged?.Invoke();
    }
    void Start()
    {
        if (data != null)
        {
            // İsim ataması (Data'dan veya özel)
            if (string.IsNullOrEmpty(candidateName)) candidateName = data.gladiatorName;

            // --- CAN HESABI ---
            // Formül: Stamina * 10 + (Level * 5)
            // Örn: 10 Stamina, 1 Level = 105 Can
            maxHealth = (data.stamina * 10) + (data.level * 5);
            currentHealth = maxHealth;
            if (healthBar != null) healthBar.UpdateBar(currentHealth, maxHealth);
        }
      
        agent = GetComponent<NavMeshAgent>();
        UpdateNameLabel();
       // GoTo(GameObject.Find("Target").transform.position);
    }


    public void DecreaseMorale(int amount)
    {
        data.morale -= amount;
        
        // Moral 0'ın altına düşmesin
        if (data.morale < 0) data.morale = 0;

        // UI'ı uyar (Eğer panel açıksa anlık güncellensin)
        RefreshStats(); 
         NotificationManager.Instance.Show($"{data.gladiatorName} moral kaybetti. Yeni Moral: {data.morale}", NotificationType.Error);
        
        // İLERİDE BURAYA: if (morale == 0) IsyanEt(); eklenecek.
    }
public void UpdateNameLabel()
    {
        if (nameLabel != null)
        {
            nameLabel.text = data.gladiatorName; // Veya soldierName
            
            // İstersen altına rütbesini veya mesleğini de yazdırabilirsin
            // nameLabel.text = $"{candidateName}\n<size=70%>{soldierClass}</size>";
        }
    }
    public void GoTo(Vector3 target)
    {
        agent.SetDestination(target);
    }
}
