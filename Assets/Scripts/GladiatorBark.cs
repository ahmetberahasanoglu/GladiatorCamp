using UnityEngine;
using UnityEngine.UI; // Image kullanacağımız için eklendi
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GladiatorBark : MonoBehaviour
{
    [Header("UI Referansları")]
    public TextMeshProUGUI barkText;      // Artık UGUI kullanıyoruz
    public Image bubbleBackground;        // Konuşma balonunun arka planı

    [Header("Zaman Ayarları")]
    public float minTimeBetweenBarks = 10f; 
    public float maxTimeBetweenBarks = 25f; 
    public float displayDuration = 3.5f;    
    public float typeDelay = 0.04f;       // YENİ: Daktilo hızı (Harf başı geçen süre)

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
        
        // Başlangıçta yazıyı ve balonu tamamen kapat ki havada boş balon durmasın
        if (bubbleBackground != null) bubbleBackground.gameObject.SetActive(false);
        barkText.text = "";

        Invoke(nameof(TriggerRandomBark), Random.Range(minTimeBetweenBarks, maxTimeBetweenBarks));
    }
/*
    void LateUpdate()
    {
        // Yazı (ve balon) açıksa, Canvas'ın her zaman kameraya bakmasını (Billboard) sağla
        if (barkText != null && Camera.main != null && bubbleBackground.gameObject.activeSelf)
        {
            // Text'in bağlı olduğu ana Canvas'ı döndürüyoruz
            barkText.canvas.transform.rotation = Camera.main.transform.rotation;
        }
    }*/

    void TriggerRandomBark()
    {
        GladiatorAI ai = GetComponent<GladiatorAI>();
        if (_gladiator.isOnMission || (ai != null && ai.isDead)) return;

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
        // ... (Bir önceki mesajdaki mantık tamamen aynı kalacak, sadece ne diyeceğini seçiyor)
        
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

    // YENİ: Daktilo (Typewriter) Efektli Gösterme Rutini
    IEnumerator ShowBarkRoutine(string text)
    {
        // 1. Yazıyı yerleştir ve baloncuğu aç (Kutu olması gereken boyuta hemen ulaşır)
        barkText.text = text;
        barkText.maxVisibleCharacters = 0; // Ama harfleri görünmez yap!
        if (bubbleBackground != null) bubbleBackground.gameObject.SetActive(true);

        // 2. Daktilo efekti ile harfleri tek tek görünür yap
        int totalCharacters = text.Length;
        for (int i = 0; i <= totalCharacters; i++)
        {
            barkText.maxVisibleCharacters = i;
            
            // Eğer istersen buraya çok kısık bir tık sesi (UI click) koyabilirsin:
            // if (i % 3 == 0 && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.paperSound, 0.1f);
            
            yield return new WaitForSeconds(typeDelay);
        }

        // 3. Yazı tamamlandıktan sonra oyuncunun okuması için bekle
        yield return new WaitForSeconds(displayDuration);

        // 4. Konuşma bitti, balonu ve yazıyı kapat
        barkText.text = "";
        if (bubbleBackground != null) bubbleBackground.gameObject.SetActive(false);
    }
}