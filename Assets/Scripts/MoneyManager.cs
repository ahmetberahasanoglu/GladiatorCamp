using UnityEngine;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public int gold = 200;

    public int trainingCost = 30;
    public int dailyIncome = -20;//YEMEK PARASI

    public event Action<int> OnGoldChanged;

    void Awake()
    {
        Instance = this;
    }

    public bool Spend(int amount)
    {
        if (gold < amount) return false;

        gold -= amount;
        OnGoldChanged?.Invoke(gold);
        return true;
    }

    public void Add(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    public void EndOfDay()
    {
        gold += dailyIncome;
        OnGoldChanged?.Invoke(gold);
    }
    public void RefreshUI()
    {
        OnGoldChanged?.Invoke(gold);
    }
}
