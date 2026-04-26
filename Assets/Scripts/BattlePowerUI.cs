using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattlePowerUI : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Slider powerBalanceSlider; 
    public TextMeshProUGUI playerTotalHpText;
    public TextMeshProUGUI enemyTotalHpText;
    public TextMeshProUGUI speedButtonText;

    [Header("Ayarlar")]
    public float sliderSpeed = 5f; // Barın yumuşak kayma hızı
    public int currentSpeedLevel = 1;

public void ChangeGameSpeed()
{
    currentSpeedLevel++;
    
    if(currentSpeedLevel > 2)
    {
        currentSpeedLevel = 1; 
    }

    Time.timeScale = currentSpeedLevel;
    speedButtonText.text = currentSpeedLevel + "x";
}
public void ResetGameSpeed()
    {
        currentSpeedLevel = 1;
        Time.timeScale = 1f;
        if(speedButtonText != null) speedButtonText.text = "1x";
    }

    // YENİ: GÜVENLİK KİLİDİ
    // Bu UI objesi kapanırsa, yok edilirse veya sahne değişirse otomatik tetiklenir.
    // Böylece kamp ekranına asla 2x hızında dönmezsin.
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
    void Update()
    {
        // 1. KİLİT: Savaşta değilsek boşuna hesaplama yapıp bilgisayarı yorma
        if (BattleManager.Instance != null && BattleManager.Instance.state == BattleState.Idle) return;

        float playerTotalHp = 0;
        float enemyTotalHp = 0;

        // 2. Sahnedeki tüm savaşçıları bul
        Gladiator[] allFighters = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);

        foreach (var fighter in allFighters)
        {
            // Ölüleri sayma!
            GladiatorAI ai = fighter.GetComponent<GladiatorAI>();
            if (ai != null && ai.isDead) continue;

            // Bizim askerlerimizin canını topla
            if (fighter.CompareTag("MySoldier"))
            {
                playerTotalHp += fighter.currentHealth;
            }
            // Düşman askerlerinin canını topla (Düşmanların Tag'i "Enemy" olduğunu varsayıyorum)
            else if (fighter.CompareTag("EnemySoldier")) 
            {
                enemyTotalHp += fighter.currentHealth;
            }
        }

        // 3. Yazıları Güncelle
        if (playerTotalHpText != null) playerTotalHpText.text = Mathf.RoundToInt(playerTotalHp).ToString();
        if (enemyTotalHpText != null) enemyTotalHpText.text = Mathf.RoundToInt(enemyTotalHp).ToString();

        // 4. Slider'ı Güncelle (Sihrin Gerçekleştiği Yer)
        if (powerBalanceSlider != null)
        {
            float totalPower = playerTotalHp + enemyTotalHp;
            
            if (totalPower > 0)
            {
                // Bizim gücümüzün toplam güce oranı. Örn: Bizde 600, onlarda 400 can varsa oran 0.6 olur.
                float targetValue = playerTotalHp / totalPower;
                
                // Mathf.Lerp ile barın aniden değil, yumuşak ve tatmin edici bir şekilde kaymasını sağlıyoruz
                powerBalanceSlider.value = Mathf.Lerp(powerBalanceSlider.value, targetValue, Time.deltaTime * sliderSpeed);
            }
            else
            {
                powerBalanceSlider.value = 0.5f; // İki taraf da ölüyse bar tam ortada durur
            }
        }
    }
}