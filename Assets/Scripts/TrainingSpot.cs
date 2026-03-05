using UnityEngine;
using TMPro;

public enum TrainingType { Strength, Speed, Defense, Morale, Stamina }

public class TrainingSpot : MonoBehaviour
{
    public Transform trainingPoint;
    public TrainingType trainingType = TrainingType.Strength;
    public int statGain = 1;
    public bool isBusy = false;

    [Header("Görsel Geri Bildirim")]
    public GameObject hoverTextObj; // Üzerine gelince çıkacak olan yazı
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f); 
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseEnter()
    {
        if (isBusy) return; 

        transform.localScale = hoverScale;

        if (hoverTextObj != null) 
        {
            hoverTextObj.SetActive(true);
            hoverTextObj.GetComponentInChildren<TextMeshPro>().text = $"{trainingType} Eğitimi\n({MoneyManager.Instance.trainingCost} Akçe)";
        }
    }

    void OnMouseExit()
    {
        transform.localScale = originalScale;
        if (hoverTextObj != null) hoverTextObj.SetActive(false);
    }

    void OnMouseDown()
    {
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;
        if (isBusy) 
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Bu alan şu an dolu!", NotificationType.Warning);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
            return;
        }

        // 1. KONTROL: Seçili bir gladyatör var mı?
        GladiatorTraining currentGladiator = TrainingUIManager.Instance.current;
        
        if (currentGladiator == null)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Önce bir asker seçmelisin!", NotificationType.Warning);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
            return;
        }

        if (currentGladiator.IsTraining)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Bu asker zaten eğitimde!", NotificationType.Warning);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
            return;
        }

        if (currentGladiator.GetComponent<GladiatorHealing>() != null && currentGladiator.GetComponent<GladiatorHealing>().IsHealing)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Bu asker şu an şifahanede, eğitim yapamaz!", NotificationType.Warning);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
            return;
        }

        // --- GÜNCELLENEN KISIM: HAM STAT (BASE STAT) HESAPLAMA ---
        int maxAllowedStat = CampManager.Instance != null ? CampManager.Instance.GetBuildingValue("talimhane") : 15;
        
        JanissaryData data = currentGladiator.GetComponent<Gladiator>().data;
        GladiatorInventory inv = currentGladiator.GetComponent<GladiatorInventory>();

        if (data != null)
        {
            int currentTotalStat = 0;
            int equipmentBonus = 0;

            // Önce toplam statı ve o statı etkileyen ekipmanların bonuslarını topluyoruz
            switch (trainingType)
            {
                case TrainingType.Strength: 
                    currentTotalStat = data.strength; 
                    if (inv != null && inv.weapon != null) equipmentBonus += inv.weapon.bonusStrength;
                    break;
                case TrainingType.Defense: 
                    currentTotalStat = data.defense; 
                    if (inv != null) {
                        if (inv.armor != null) equipmentBonus += inv.armor.bonusDefense;
                        if (inv.helmet != null) equipmentBonus += inv.helmet.bonusDefense;
                        if (inv.shield != null) equipmentBonus += inv.shield.bonusDefense;
                    }
                    break;
                case TrainingType.Speed: 
                    currentTotalStat = data.speed; 
                    if (inv != null) {
                        if (inv.armor != null) equipmentBonus += inv.armor.bonusSpeed;
                        if (inv.helmet != null) equipmentBonus += inv.helmet.bonusSpeed;
                        if (inv.shield != null) equipmentBonus += inv.shield.bonusSpeed;
                    }
                    break;
                case TrainingType.Stamina: 
                    currentTotalStat = data.stamina; 
                    if (inv != null) {
                        if (inv.armor != null) equipmentBonus += inv.armor.bonusStamina;
                        if (inv.helmet != null) equipmentBonus += inv.helmet.bonusStamina;
                        if (inv.shield != null) equipmentBonus += inv.shield.bonusStamina;
                    }
                    break;
            }

            // Toplam stattan, eşyalardan gelen sahte gücü çıkartıyoruz!
            int baseStatValue = currentTotalStat - equipmentBonus;

            if (baseStatValue >= maxAllowedStat)
            {
                if (NotificationManager.Instance != null) 
                    NotificationManager.Instance.Show($"Askerin saf yeteneği sınırda! (Maks: {maxAllowedStat}). Daha fazlası için Talimhaneyi geliştirin.", NotificationType.Warning);
                
                if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
                return; // Eğitime gönderme, işlemi iptal et!
            }
        }
        // -----------------------------------------------------------

        // 2. KONTROL: Paramız yetiyor mu?
        if (MoneyManager.Instance.Spend(MoneyManager.Instance.trainingCost))
        {
            currentGladiator.StartTraining(this);
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
            
            TrainingUIManager.Instance.SetCurrentGladiator(null);

            OnMouseExit(); 
        }
        else
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Eğitim için yeterli akçen yok!", NotificationType.Error);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
        }
    }
}