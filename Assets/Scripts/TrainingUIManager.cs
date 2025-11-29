using UnityEngine;
using UnityEngine.UI;

public class TrainingUIManager : MonoBehaviour
{
    public static TrainingUIManager Instance;

    public GladiatorTraining current; // þu an seçili gladyatörün GladiatorTraining bileþeni

    // Her buton için ilgili TrainingSpot referansý (Inspector'da sürükle)
    public TrainingSpot strengthSpot;
    public TrainingSpot speedSpot;
    public TrainingSpot defenseSpot;
    public TrainingSpot moraleSpot;
    public TrainingSpot staminaSpot;

    public Button strengthBtn;
    public Button speedBtn;
    public Button defenseBtn;
    public Button moraleBtn;
    public Button staminaBtn;

    void Awake()
    {
        Instance = this;
    }

    public void SetCurrentGladiator(GladiatorTraining gladiator)
    {
        current = gladiator;
        UpdateButtons(current != null);
    }

    public void TrainStrength()
    {
        if (current == null || strengthSpot == null) return;
        if (MoneyManager.Instance.Spend(MoneyManager.Instance.trainingCost))
            current.StartTraining(strengthSpot);
    }

    public void TrainSpeed()
    {
        if (current == null || speedSpot == null) return;
        if (MoneyManager.Instance.Spend(MoneyManager.Instance.trainingCost))
            current.StartTraining(speedSpot);
    }

    public void TrainDefense()
    {
        if (current == null || defenseSpot == null) return;
        if (MoneyManager.Instance.Spend(MoneyManager.Instance.trainingCost))
            current.StartTraining(defenseSpot);
    }

    public void TrainMorale()
    {
        if (current == null || moraleSpot == null) return;
        if (MoneyManager.Instance.Spend(MoneyManager.Instance.trainingCost))
            current.StartTraining(moraleSpot);
    }

    public void TrainStamina()
    {
        if (current == null || staminaSpot == null) return;
        if (MoneyManager.Instance.Spend(MoneyManager.Instance.trainingCost))
            current.StartTraining(staminaSpot);
    }

    void UpdateButtons(bool state)
    {
        strengthBtn.interactable = state;
        speedBtn.interactable = state;
        defenseBtn.interactable = state;
        moraleBtn.interactable = state;
        staminaBtn.interactable = state;
    }
}
