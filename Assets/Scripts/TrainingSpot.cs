using UnityEngine;

public enum TrainingType { Strength, Speed, Defense, Morale, Stamina }

public class TrainingSpot : MonoBehaviour
{
    public Transform trainingPoint;
    public TrainingType trainingType = TrainingType.Strength;
    public int statGain = 1;
    //public float trainingTime = 5f; // seconds
    public bool isBusy = false;
}
