using UnityEngine;
using System;

public class NasipManager : MonoBehaviour
{
    public static NasipManager Instance;

    public int currentNasip = 0;
    public event Action<int> OnNasipChanged;

    void Awake()
    {
        Instance = this;
    }

    // Camiye giden asker bu fonksiyonu çağıracak
    public void AddNasip(int amount)
    {
        currentNasip += amount;
        OnNasipChanged?.Invoke(currentNasip);
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show($"Nasibimiz arttı (+{amount})", NotificationType.Info);
    }

    // Nasip kullanıldığında (örneğin zar atarken biraz harcamak istersen)
    public void SpendNasip(int amount)
    {
        currentNasip -= amount;
        if (currentNasip < 0) currentNasip = 0;
        OnNasipChanged?.Invoke(currentNasip);
    }
}