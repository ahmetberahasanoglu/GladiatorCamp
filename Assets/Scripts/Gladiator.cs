using UnityEngine;
using UnityEngine.AI;
using System;
using TMPro;
public enum SoldierActivity { Training, Working, Idling }
public class Gladiator : MonoBehaviour
{
    [SerializeField] private JanissaryData _templateData;
    public JanissaryData data {get;set;}
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
