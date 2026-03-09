using UnityEngine;

public class DefeatTutorialBtn : MonoBehaviour
{
    public void Click()
    {
      if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
    {
        if (TutorialManager.Instance.currentStep == TutorialStep.Battle_ScriptedLoss)
        {
        
            TutorialManager.Instance.SetStep(TutorialStep.Rebirth_Intro);
        }
    }
    }
}
