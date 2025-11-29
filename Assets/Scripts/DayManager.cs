using UnityEngine;
using System;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    public event Action OnNewDay;

    void Awake()
    {
        Instance = this;
    }

    public void NextDay()
    {
        OnNewDay?.Invoke();
        MoneyManager.Instance.EndOfDay();
    }
}
