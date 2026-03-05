using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GladiatorBark : MonoBehaviour
{
    [Header("UI Referansları")]
    public TextMeshProUGUI barkText;      
    public Image bubbleBackground;        

    [Header("Zaman Ayarları")]
    public float minTimeBetweenBarks = 10f; 
    public float maxTimeBetweenBarks = 25f; 
    public float displayDuration = 3.5f;    
    public float typeDelay = 0.04f;       

    [Header("Replikler (Aklından Geçenler)")]
    public List<string> idleBarks;       
    public List<string> trainingBarks;   
    public List<string> workingBarks;    
    public List<string> lowMoraleBarks;  
    public List<string> winterBarks;     

    private Gladiator _gladiator;
    private Coroutine _barkRoutine;

    void Start()
    {
        _gladiator = GetComponent<Gladiator>();
        
        if (bubbleBackground != null) bubbleBackground.gameObject.SetActive(false);
        barkText.text = "";

        Invoke(nameof(TriggerRandomBark), Random.Range(minTimeBetweenBarks, maxTimeBetweenBarks));
    }

    void Update()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.state == BattleState.Fighting)
        {
            if (bubbleBackground != null && bubbleBackground.gameObject.activeSelf)
            {
                if (_barkRoutine != null) StopCoroutine(_barkRoutine);
                barkText.text = "";
                bubbleBackground.gameObject.SetActive(false);
            }
        }
    }

    void TriggerRandomBark()
    {
        GladiatorAI ai = GetComponent<GladiatorAI>();

        if (_gladiator.isOnMission || (ai != null && ai.isDead) || 
           (BattleManager.Instance != null && BattleManager.Instance.state == BattleState.Fighting)) 
        {
            Invoke(nameof(TriggerRandomBark), Random.Range(minTimeBetweenBarks, maxTimeBetweenBarks));
            return;
        }

        string selectedLine = DetermineBarkLine();

        if (!string.IsNullOrEmpty(selectedLine))
        {
            if (_barkRoutine != null) StopCoroutine(_barkRoutine);
            _barkRoutine = StartCoroutine(ShowBarkRoutine(selectedLine));
        }

        Invoke(nameof(TriggerRandomBark), Random.Range(minTimeBetweenBarks, maxTimeBetweenBarks));
    }

    string DetermineBarkLine()
    {
        if (DayManager.Instance != null && DayManager.Instance.currentDay >= DayManager.Instance.maxDays - 5)
            if (Random.value > 0.5f && winterBarks.Count > 0) return winterBarks[Random.Range(0, winterBarks.Count)];

        if (CampMoraleManager.Instance != null && CampMoraleManager.Instance.currentMorale < 40)
            if (Random.value > 0.4f && lowMoraleBarks.Count > 0) return lowMoraleBarks[Random.Range(0, lowMoraleBarks.Count)];

        if (_gladiator.data != null && _gladiator.data.currentActivity == SoldierActivity.Working)
            if (workingBarks.Count > 0) return workingBarks[Random.Range(0, workingBarks.Count)];
        
        var training = GetComponent<GladiatorTraining>();
        if (training != null && training.IsTraining)
            if (trainingBarks.Count > 0) return trainingBarks[Random.Range(0, trainingBarks.Count)];

        var healing = GetComponent<GladiatorHealing>();
        if (healing != null && healing.IsHealing)
            return "Merhem sızıyı alıyor ama izi kalacak."; 

        if (idleBarks.Count > 0) return idleBarks[Random.Range(0, idleBarks.Count)];

        return "";
    }

    IEnumerator ShowBarkRoutine(string text)
    {
        barkText.text = text;
        barkText.maxVisibleCharacters = 0; 
        if (bubbleBackground != null) bubbleBackground.gameObject.SetActive(true);

        int totalCharacters = text.Length;
        for (int i = 0; i <= totalCharacters; i++)
        {
            barkText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typeDelay);
        }

        yield return new WaitForSeconds(displayDuration);

        barkText.text = "";
        if (bubbleBackground != null) bubbleBackground.gameObject.SetActive(false);
    }
}