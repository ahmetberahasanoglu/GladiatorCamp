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

    void HealIdleSoldiers()
    {
        // Sahnede olan tüm askerleri bul
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        bool anyoneHealed = false;

        foreach (Gladiator soldier in allSoldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            
            // Eğer asker ölü değilse (veya AI'ı yoksa) VE canı maksimumdan azsa
            if ((ai == null || !ai.isDead) && soldier.currentHealth < soldier.maxHealth)
            {
                // 1. Şifayı uygula
                soldier.currentHealth += healAmount;
                
                // 2. Maksimum canı aşmasını engelle
                if (soldier.currentHealth > soldier.maxHealth) 
                {
                    soldier.currentHealth = soldier.maxHealth;
                }

                // 3. UI Barını Güncelle (Senin DayManager'da kullandığın kodun aynısı!)
                if (soldier.healthBar != null)
                {
                    soldier.healthBar.UpdateBar(soldier.currentHealth, soldier.maxHealth);
                }

                anyoneHealed = true;

                // 4. Şifa Efekti (Yeşil Artı)
                if (healParticlePrefab != null)
                {
                    // Askerin boyundan biraz yüksekte (Vector3.up * 2f) efekti yarat
                    GameObject fx = Instantiate(healParticlePrefab, soldier.transform.position + Vector3.up * 2f, Quaternion.identity);
                    Destroy(fx, 1.5f); // Çöpe dönüşmesin diye 1.5 sn sonra sil
                }
            }
        }

        // Opsiyonel: Biri iyileştiyse kampa ufak bir şifa sesi veya rüzgar sesi çalınabilir
        // if (anyoneHealed && audioSource != null) audioSource.Play();
    }
}