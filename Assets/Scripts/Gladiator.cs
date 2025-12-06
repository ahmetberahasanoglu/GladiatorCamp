using UnityEngine;
using UnityEngine.AI;
using System;
public class Gladiator : MonoBehaviour
{
    [SerializeField] private JanissaryData _templateData;
    public JanissaryData data {get;set;}
    private NavMeshAgent agent;
    public event Action OnStatsChanged;
    public bool isOnMission = false;
    void Awake()
    {
        if (_templateData != null)
        {
            data = Instantiate(_templateData);
        }
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
            bool isTraining = training != null && training.IsTraining;
            
            return !isOnMission && !isTraining;
        }
    }   
    public void RefreshStats()
    {
        OnStatsChanged?.Invoke();
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
       // GoTo(GameObject.Find("Target").transform.position);
    }


    public void DecreaseMorale(int amount)
    {
        data.morale -= amount;
        
        // Moral 0'ın altına düşmesin
        if (data.morale < 0) data.morale = 0;

        // UI'ı uyar (Eğer panel açıksa anlık güncellensin)
        RefreshStats(); 

        Debug.Log($"{data.gladiatorName} moral kaybetti. Yeni Moral: {data.morale}");
        
        // İLERİDE BURAYA: if (morale == 0) IsyanEt(); eklenecek.
    }

    public void GoTo(Vector3 target)
    {
        agent.SetDestination(target);
    }
}
