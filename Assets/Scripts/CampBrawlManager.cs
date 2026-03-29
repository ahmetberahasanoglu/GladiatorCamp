using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CampBrawlManager : MonoBehaviour
{
    public static CampBrawlManager Instance;

    [Header("Ayarlar")]
    public GameObject brawlIconPrefab; // Ünlem Prefabı
    public float checkInterval = 20f; 
    public float brawlDuration = 30f; 

    // YENİ: Mükerrer kavga engeli listesi
    public List<Gladiator> activeBrawlers = new List<Gladiator>();

    // YENİ: Haritanın açık olup olmadığını dışarıdan kontrol edebileceğimiz değişken
    public bool isMapOpen = false; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(BrawlCheckRoutine());
    }

    IEnumerator BrawlCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            TryStartBrawl();
        }
    }

    void TryStartBrawl()
    {
        // 1. KONTROL: Oyun duraklatılmışsa veya HARİTA AÇIKSA kavga başlatma!
        if (Time.timeScale == 0 || isMapOpen) return;

        // Savaşta falan mıyız?
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle) return;

        int morale = CampMoraleManager.Instance != null ? CampMoraleManager.Instance.currentMorale : 100;
        if (morale > 60) return; 

        int brawlChance = 80 - morale; 
        if (Random.Range(0, 100) > brawlChance) return; 

        List<Gladiator> idleSoldiers = GetIdleSoldiers();
        
        if (idleSoldiers.Count < 2) return; 

        Gladiator s1 = idleSoldiers[Random.Range(0, idleSoldiers.Count)];
        idleSoldiers.Remove(s1);
        Gladiator s2 = idleSoldiers[Random.Range(0, idleSoldiers.Count)];

        StartBrawl(s1, s2);
    }

    List<Gladiator> GetIdleSoldiers()
    {
        List<Gladiator> idles = new List<Gladiator>();
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);

        foreach (var soldier in allSoldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead) && soldier.data != null)
            {
                // YENİ KONTROL: Asker boşta mı VE halihazırda başka bir kavgada değil mi?
                if (soldier.data.currentActivity == SoldierActivity.Idling && !activeBrawlers.Contains(soldier))
                {
                    // Askerin canı çok düşükse kavgaya mecali yoktur
                    if (soldier.currentHealth > 5f) 
                    {
                        idles.Add(soldier);
                    }
                }
            }
        }
        return idles;
    }

    void StartBrawl(Gladiator s1, Gladiator s2)
    {
        // Adamları mükerrer kavga engeli listesine ekle
        activeBrawlers.Add(s1);
        activeBrawlers.Add(s2);

        // --- YENİ MANTIK: Pozisyonu Manager değil, Setup kendisi halledecek ---
        // Sadece adamların ortasında ünlemi spawn et, Setup fonksiyonu kavgayı arrange edecek
        GameObject iconObj = Instantiate(brawlIconPrefab, Vector3.zero, Quaternion.identity);
        BrawlEvent brawl = iconObj.GetComponent<BrawlEvent>();
        
        // Setup fonksiyonu kavgayı organize edecek
        brawl.Setup(s1, s2, brawlDuration);
    }
}