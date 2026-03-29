using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CampBrawlManager : MonoBehaviour
{
    public static CampBrawlManager Instance;

    [Header("Ayarlar")]
    public GameObject brawlIconPrefab; // Senin hazırladığın ve içine BrawlEvent.cs attığın Ünlem Prefabı
    public float checkInterval = 20f; // Kaç saniyede bir kavga ihtimali zarı atılsın? (Gerçek zamanlı)
    public float brawlDuration = 30f; // Kavgaya müdahale etmek için kaç saniyesi var?

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Döngüyü başlat
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
        // 1. Savaşta falan mıyız? Savaşta kendi içlerinde kavga etmezler.
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle) return;

        // 2. Moral Kontrolü (Moral 60'ın üzerindeyse herkes mutludur, kavga çıkmaz)
        int morale = CampMoraleManager.Instance != null ? CampMoraleManager.Instance.currentMorale : 100;
        if (morale > 60) return; 

        // 3. Kavga Çıkma İhtimali (Moral ne kadar düşükse ihtimal o kadar yüksek)
        int brawlChance = 80 - morale; // Örn: Moral 30 ise %50 ihtimal, Moral 10 ise %70 ihtimal.
        if (Random.Range(0, 100) > brawlChance) return; // Şans tutmadı, şimdilik sakinler

        // 4. Boşta takılan askerleri bul
        List<Gladiator> idleSoldiers = GetIdleSoldiers();
        
        // Kavga için en az 2 kişi lazım!
        if (idleSoldiers.Count < 2) return; 

        // 5. İki rastgele kurban seç
        Gladiator s1 = idleSoldiers[Random.Range(0, idleSoldiers.Count)];
        idleSoldiers.Remove(s1);
        Gladiator s2 = idleSoldiers[Random.Range(0, idleSoldiers.Count)];

        // Kavgayı Başlat!
        StartBrawl(s1, s2);
    }

    List<Gladiator> GetIdleSoldiers()
    {
        List<Gladiator> idles = new List<Gladiator>();
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);

        foreach (var soldier in allSoldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            
            // Asker hayattaysa, bizim askerimizse ve kampta boş boş duruyorsa...
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead) && soldier.data != null)
            {
                // Görevde, talimde veya çalışıyorsa kavga etmeye mecali yoktur. Boşta olması lazım.
                if (soldier.data.currentActivity == SoldierActivity.Idling)
                {
                    idles.Add(soldier);
                }
            }
        }
        return idles;
    }

    void StartBrawl(Gladiator s1, Gladiator s2)
    {
        // Ünlemi tam ikisinin ortasında ve kafalarının biraz üzerinde çıkar
        Vector3 centerPoint = (s1.transform.position + s2.transform.position) / 2f + Vector3.up * 3f;
        
        GameObject iconObj = Instantiate(brawlIconPrefab, centerPoint, Quaternion.identity);
        BrawlEvent brawl = iconObj.GetComponent<BrawlEvent>();
        
        brawl.Setup(s1, s2, brawlDuration);
    }
}