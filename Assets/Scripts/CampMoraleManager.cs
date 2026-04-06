using UnityEngine;
using TMPro;
using UnityEngine.UI; // YENİ: UI işlemleri için
using System.Collections.Generic;

public class CampMoraleManager : MonoBehaviour
{
    public static CampMoraleManager Instance;

    [Header("UI Ayarları")]
    public TextMeshProUGUI moraleText; 
    public Image moraleFillBar; 

    [Header("Durum")]
    [Range(0, 100)]
    public int currentMorale = 80; 

    private bool isInitialized = false;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay -= CheckDesertion;
        }
    }
    void Start()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay += CheckDesertion;
        }
        UpdateUI();
        isInitialized = true;
    }

    public void ChangeMorale(int amount)
    {
        if (amount == 0) return;
        currentMorale += amount;
        currentMorale = Mathf.Clamp(currentMorale, 0, 100);
        
        UpdateUI();

        if (isInitialized && TopInfoBarUI.Instance != null && moraleText != null)
        {
            TopInfoBarUI.Instance.FlashUI(moraleText);
        }

        if (NotificationManager.Instance != null)
        {
            if (amount > 0)
                NotificationManager.Instance.Show($"Kışla Morali Yükseldi! (+{amount})", NotificationType.Success);
            else if (amount < 0)
                NotificationManager.Instance.Show($"Kışla Morali Düştü! ({amount})", NotificationType.Error);
        }
    }
  
    public void CheckDesertion()
    {
        // 1. Kilit: Moral 20'nin üzerindeyse kimse kaçmaz, herkes halinden memnundur.
        if (currentMorale > 20) return;

        // 2. İhtimal Hesaplama (Moral ne kadar düşükse kaçma ihtimali o kadar artar)
        // Moral 0 ise %100 kaçar. Moral 20 ise %20 ihtimalle kaçar.
        int desertionChance = 100 - (int)(currentMorale * 4); // Örn: Moral 10 ise şans %60 olur
        
        if (Random.Range(0, 100) > desertionChance) return; // Şans tutmadı, bugünlük sabrettiler

        // 3. Kaçacak Adam Seçme
        Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        List<Gladiator> potentialDeserters = new List<Gladiator>();

        foreach (var soldier in allSoldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            
            // Asker hayattaysa, bizim askerimizse ve (ÇOK ÖNEMLİ) Seferde değilse!
            // Dağdaki adam kamptan kaçamaz, kampta boş duran veya çalışan kaçar.
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead) && soldier.data != null)
            {
                if (soldier.data.currentActivity != SoldierActivity.OnMission)
                {
                    potentialDeserters.Add(soldier);
                }
            }
        }

    
        if (potentialDeserters.Count > 0)
        {
            int randomIndex = Random.Range(0, potentialDeserters.Count);
            Gladiator coward = potentialDeserters[randomIndex];
            string cowardName = coward.data.gladiatorName;

            // Varsa silahını/zırhını da çalıp gitmesin, depoya geri bıraksın
            // (Eğer envanter sisteminde InventoryUIManager.Instance.UnequipItem falan kullanıyorsan buraya ekleyebilirsin)

            // Ekrana vurucu bir bildirim bas!
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show($"<color=red>İSYAN VE FİRAR!</color> Moral o kadar düşük ki, {cowardName} gece yarısı kamptan kaçtı!", NotificationType.Error);

            // Askeri sistemden temizle
            if (GladiatorSelector.Instance != null) GladiatorSelector.Instance.DeselectIfDead(coward.gameObject);
            
            // Askeri direkt yok et (Ölüm efekti çıkmasın, çünkü ölmedi, kaçtı)
            Destroy(coward.gameObject);

            // Üst panellerdeki kapasiteyi güncellemek için yarım saniye bekle (Obje silinene kadar)
            StartCoroutine(UpdateUICoroutine());
        }
    }

    private System.Collections.IEnumerator UpdateUICoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        if (TopInfoBarUI.Instance != null) TopInfoBarUI.Instance.UpdateCapacity();
        if (RecruitManager.Instance != null) RecruitManager.Instance.RefreshUI();
    }
    void UpdateUI()
    {
        if (moraleText != null)
        {
            moraleText.text = $"{currentMorale}";
            if (currentMorale <= 40) moraleText.color = Color.red;    
            else moraleText.color = Color.black; 
        }

        // YENİ: Barı Güncelle
        if (moraleFillBar != null)
        {
            // Moral max 100 olduğu için direkt 100'e bölüyoruz
            moraleFillBar.fillAmount = Mathf.Clamp01((float)currentMorale / 100f);
        }
    }
}