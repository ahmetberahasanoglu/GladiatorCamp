using UnityEngine;
using UnityEngine.AI;
using System;
using TMPro;

public enum SoldierActivity { Training, Working, Idling,Praying,Healing,OnMission }

public class Gladiator : MonoBehaviour
{
    [SerializeField] private JanissaryData _templateData;
    public JanissaryData data { get; private set; }
    [Header("Kişilik Sistemi")]
   
    public HealthBar healthBar;
    public string candidateName;
    private NavMeshAgent agent;
    public event Action OnStatsChanged;

    [Header("Görsel Ayarlar")]
    public TextMeshProUGUI nameLabel;

    void Awake()
    {
        if (_templateData != null)
        {
            // Şablondan bu askere özel eşsiz bir kopya oluşturuyoruz
            data = Instantiate(_templateData);
        }
    }

    void Start()
    {
        if (data != null)
        {
            if (string.IsNullOrEmpty(candidateName)) candidateName = data.gladiatorName;

             int staminaMultiplier = (data.trait == SoldierTrait.Obur) ? 15 : 10;
            data.maxHealth = 100 + (data.stamina * staminaMultiplier) + (data.level * 5);
            
            data.currentHealth = data.maxHealth;

            if (healthBar != null) healthBar.UpdateBar(data.currentHealth, data.maxHealth);
        }
      
        agent = GetComponent<NavMeshAgent>();
        UpdateNameLabel();
      
    }
    



public bool IsAvailableForTask()
{
    if (data.currentHealth <= 0) return false;

    return data.currentActivity == SoldierActivity.Idling;
}

   // --- GÜNCELLENMİŞ: ORDUDAN KOVMA FONKSİYONU (Gladiator.cs İçinde) ---
    public void DismissFromArmy()
    {
        ConfirmationManager.Instance.Show(
            "<color=red>Sürgün Kararı</color>", 
            $"<color=yellow>{data.gladiatorName}</color> isimli askeri ordudan atmak istediğine emin misin?\n\n<size=80%>Bu işlem geri alınamaz ve kampta ufak bir moral düşüşüne sebep olur.</size>", 
            () => 
            {
                // Askeri şifahaneden veya eğitimden güvenlice çek
                SetIdle();

                // Geri bildirim ver
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.Show($"{data.gladiatorName} ordudan atıldı! Kampta huzursuzluk var.", NotificationType.Error);
                }

                if (TrainingUIManager.Instance != null)
                {
                    TrainingUIManager.Instance.SetCurrentGladiator(null);
                }

                // --- SİHİRLİ DOKUNUŞ (UI GÜNCELLEMESİ İÇİN) ---
                
                // 1. Askerin etiketini sil ki TopBar sayarken ve masraf hesaplarken bunu görmesin
                gameObject.tag = "Untagged"; 
                
                // 2. TopBar'ı ZORLA GÜNCELLE (Bu kod Kapasiteyi, Yemeği ve Maaşı anında düşürecektir)
                if (TopInfoBarUI.Instance != null)
                {
                    TopInfoBarUI.Instance.UpdateCapacity();
                }

                // ----------------------------------------------

                // Askeri sahneden sil (Frame sonunda silinecek ama artık sayılmıyor)
                Destroy(gameObject);
            }
        );
    }
  // --- GÜNCELLENMİŞ: STAT VEYA SEVİYE ARTINCA/AZALINCA CANI YENİDEN HESAPLAR ---
    public void RecalculateMaxHealth()
    {
        if (data == null) return;

        float oldMaxHealth = data.maxHealth;

        int staminaMultiplier = (data.trait == SoldierTrait.Obur) ? 15 : 10;
        data.maxHealth = 100 + (data.stamina * staminaMultiplier) + (data.level * 5);

        float difference = data.maxHealth - oldMaxHealth;
        
       
        if (difference > 0)
        {
            data.currentHealth += difference;
        }

  
        data.currentHealth = Mathf.Min(data.currentHealth, data.maxHealth);

        // Can barını güncelle
        if (healthBar != null) 
        {
            healthBar.UpdateBar(data.currentHealth, data.maxHealth);
        }
    }

// İş atama ve bitirme için yardımcı bir fonksiyon (Yine Gladiator.cs içine):
  public void SetActivity(SoldierActivity newActivity)
    {
        if (data != null) data.currentActivity = newActivity;
    }

    public void InitializeData(JanissaryData sourceData)
    {
        data = Instantiate(sourceData);
        // İleride Save sisteminden veri yüklerken bu fonksiyonu kullanacağız
    }

    // Kısa yollar (Böylece diğer scriptlerdeki kodları tek tek değiştirmene gerek kalmaz)
    public float currentHealth 
    { 
        get => data != null ? data.currentHealth : 0; 
        set { if (data != null) data.currentHealth = value; } 
    }
    
    public float maxHealth 
    { 
        get => data != null ? data.maxHealth : 1; 
        set { if (data != null) data.maxHealth = value; } 
    }

    public bool isOnMission
    {
        get => data != null && data.isOnMission;
        set { if (data != null) data.isOnMission = value; }
    }

  
    public bool IsAvailable 
    {
        get 
        {
            var training = GetComponent<GladiatorTraining>();
            bool isTraining = training != null && training.IsTraining;
            return !isOnMission && !isTraining;
        }
    }   
public void SetIdle()
    {
        // 1. Asıl kilitlenmeyi çözen kısım (Aktiviteyi Boşta yap)
        if (data != null) 
        {
            data.currentActivity = SoldierActivity.Idling;
        }

        // 2. Eğer seferdeyse onu da iptal et
        isOnMission = false;

        // IsTraining ve IsHealing değişkenleri remainingDays 0 olduğunda 
        // kendi kendine false olacağı için onlara dokunmuyoruz!

        // 3. UI Panellerini güncelle
        RefreshStats(); 
    }
    public void RefreshStats()
    {
        OnStatsChanged?.Invoke();
        if (healthBar != null && data != null) 
        {
            healthBar.UpdateBar(data.currentHealth, data.maxHealth);
        }
    }
/*
    public void DecreaseMorale(int amount)
    {
        data.morale -= amount;
        if (data.morale < 0) data.morale = 0;

        RefreshStats(); 
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.Show($"{data.gladiatorName} moral kaybetti. Yeni Moral: {data.morale}", NotificationType.Error);
        }
    }
*/
    public void UpdateNameLabel()
    {
        if (nameLabel != null && data != null)
        {
            nameLabel.text = data.gladiatorName; 
        }
    }

    public void GoTo(Vector3 target)
    {
        if (agent != null) agent.SetDestination(target);
    }
}