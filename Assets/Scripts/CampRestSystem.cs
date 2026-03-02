using UnityEngine;
using System.Collections;

public class CampRestSystem : MonoBehaviour
{
    [Header("İyileşme Ayarları")]
    public float healInterval = 10f; // Kaç saniyede bir can dolsun? (Gerçek zaman)
    public float healAmount = 1f;    // Her seferinde kaç can versin? (Senin sisteminde can float olduğu için float yaptık)

    [Header("Görsel Efekt")]
    public GameObject healParticlePrefab; // Askerin kafasında çıkacak yeşil '+' efekti

    void Start()
    {
        // Oyun başlar başlamaz gerçek zamanlı şifa döngüsünü başlat
        StartCoroutine(HealingRoutine());
    }

    IEnumerator HealingRoutine()
    {
        while (true)
        {
            // Belirlenen süre kadar (gerçek zamanlı) bekle
            yield return new WaitForSeconds(healInterval);

            HealIdleSoldiers();
        }
    }

   // CampRestSystem.cs içindeki HealIdleSoldiers fonksiyonu
    void HealIdleSoldiers()
    {
        // 1. Kilit: Savaşta mıyız? Savaşta dinlenme olmaz.
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle) return;

        // 2. Kilit: KAMP SOĞUK MU?
        // Sıcaklık 50'den düşükse askerler üşür ve yaraları iyileşemez!
        if (CampSurvivalManager.Instance != null && CampSurvivalManager.Instance.currentTemperature < 50)
        {
            return; // Kod burada durur, aşağıya (şifa kısmına) inmez.
        }

        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        bool anyoneHealed = false;

        foreach (Gladiator soldier in allSoldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            
            if ((ai == null || !ai.isDead) && soldier.CompareTag("MySoldier") && soldier.currentHealth < soldier.maxHealth)
            {
                // Şifayı uygula (10 saniyede bir 2 can çok dengelidir)
                soldier.currentHealth += healAmount; 
                
                if (soldier.currentHealth > soldier.maxHealth) soldier.currentHealth = soldier.maxHealth;

                if (soldier.healthBar != null) soldier.healthBar.UpdateBar(soldier.currentHealth, soldier.maxHealth);

                anyoneHealed = true;

                // Yeşil Artı Efekti
                if (healParticlePrefab != null)
                {
                    GameObject fx = Instantiate(healParticlePrefab, soldier.transform.position + Vector3.up * 2f, Quaternion.identity);
                    Destroy(fx, 1.5f); 
                }
            }
        }
    }
}