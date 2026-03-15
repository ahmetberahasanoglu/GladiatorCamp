using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;
    
    // UI'ın güncellenmesi için event
    public event Action OnResourcesChanged; 

    public int wood = 0;
    [Header("Kapasite")]
    public int maxWood = 100;

    void Awake()
    {
        Instance = this;
    }

   public void AddWood(int amount)
    {
        wood += amount;
        if (wood > maxWood) wood = maxWood;
        
        OnResourcesChanged?.Invoke();
    }

    public bool SpendWood(int amount)
    {
        if (wood >= amount)
        {
            wood -= amount;
            OnResourcesChanged?.Invoke();
            return true;
        }
        return false;
    }
}