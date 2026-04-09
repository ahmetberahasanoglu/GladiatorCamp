using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GladiatorTraining : MonoBehaviour
{
    public bool IsTraining => remainingDays > 0;

    private NavMeshAgent agent;
    private Gladiator gladiator;

    private TrainingSpot currentSpot;
    private int remainingDays = 0;
    private TrainingType currentTrainingType;

    [Header("Görsel Geri Bildirim")]
    public GameObject selectionRing;

    [Header("Efektler")]
public ParticleSystem levelUpParticle;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gladiator = GetComponent<Gladiator>();

        if (selectionRing != null) selectionRing.SetActive(false);
    }
    void OnMouseDown()
    {
        if (IsTraining)
        {
            if (NotificationManager.Instance != null) 
                NotificationManager.Instance.Show("Bu asker şu an eğitimde, rahatsız etme!", NotificationType.Warning);
            return;
        }

        // Kendi kendini yöneticiye "Beni seçti" diye bildirir
        if (TrainingUIManager.Instance != null)
        {
            TrainingUIManager.Instance.SetCurrentGladiator(this);
        }
    }
    public void SetSelectedVisual(bool isSelected)
    {
        if (selectionRing != null)
        {
            selectionRing.SetActive(isSelected);
        }
    }
   public void StartTraining(TrainingSpot spot)
    {
        if (IsTraining) return;
        if (spot == null || spot.isBusy) return;

        // --- YENİ BARRİYER: Asker camideyse eğitime GİDEMEZ ---
        var praying = gladiator.GetComponent<GladiatorPraying>();
        if (praying != null && praying.isPraying)
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Bu asker şu an ibadette! Önce duasını bitirmeli.", NotificationType.Warning);
            
            if (GladiatorSelector.Instance != null)
                GladiatorSelector.Instance.ClearSelection();
                
            return; // Kodu durdur, eğitime başlatma!
        }
        if (gladiator.data.currentActivity == SoldierActivity.Working)
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Bu asker şu an şehirde çalışıyor! Emri yerine getiremez.", NotificationType.Error);
            return; 
        }

        currentSpot = spot;
        spot.isBusy = true;

        currentTrainingType = spot.trainingType;
        remainingDays = GetRequiredDays(gladiator.data.level);
        agent.SetDestination(spot.trainingPoint.position);
        StartCoroutine(WalkAndBeginTraining());
    }

    IEnumerator WalkAndBeginTraining()
    {
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetBool("isRunning", true);
        while (agent.pathPending) yield return null;
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
        if (anim != null) anim.SetBool("isRunning", false);
        if (currentSpot != null)
        {
            // Askeri kuklaya doğru çevir
            transform.LookAt(new Vector3(currentSpot.transform.position.x, transform.position.y, currentSpot.transform.position.z));
            
            // Sürekli antrenman yapmasını sağlayan döngüyü başlat
            StartCoroutine(TrainingAnimationRoutine());
        }

        DayManager.Instance.OnNewDay += OnNewDay;
    }
IEnumerator TrainingAnimationRoutine()
    {
        Animator anim = GetComponentInChildren<Animator>();
        
        while (IsTraining)
        {
            // Eğer "Train" adında özel bir animasyonun yoksa, normal "Attack" animasyonunu da kullanabilirsin
            if (anim != null) anim.SetTrigger("Train"); 
            
            // 2 saniyede bir vur (Hızı gözüne göre ayarlarsın)
            yield return new WaitForSeconds(4.0f); 
        }
    }

    // --- 3. EKLENTİ: ANIMATION EVENT (Kılıcın Değdiği An Çalışır) ---
    // Bunu Unity'de animasyonun o "Vurma" karesine (Frame) ekleyeceğiz
    public void OnTrainingHitDummy()
    {
        if (currentSpot != null)
        {
            // TrainingSpot'un üzerinde veya içinde duran DummyInteract'i bul ve ona vur!
            DummyInteract dummy = currentSpot.GetComponentInChildren<DummyInteract>();
            if (dummy == null) dummy = currentSpot.GetComponentInParent<DummyInteract>();

            if (dummy != null)
            {
                dummy.ReceiveHit(); // Kukla sarsılsın, partikül patlasın!
            }
        }
    }
    public void StopTraining()
    {
        if (!IsTraining) return;

        remainingDays = 0; // IsTraining değerini anında false yapar

        if (currentSpot != null)
        {
            currentSpot.isBusy = false;
            currentSpot = null;
        }

        // Gün geçiş aboneliğini ve yürüyüş/vurma döngülerini iptal et
        DayManager.Instance.OnNewDay -= OnNewDay;
        StopAllCoroutines(); 
        
        // Ajanı olduğu yerde durdur
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // Animasyonu sıfırla
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetBool("isRunning", false);
            anim.Play("Idle"); // Vurmayı bırakıp direkt duruşa geçsin
        }
    }
    void OnNewDay()
    {
        if (!IsTraining) return;

        remainingDays--;

       // UITrainingProgress.Instance.UpdateProgress(remainingDays);

        if (remainingDays <= 0)
        {
            FinishTraining();
        }
    }
void OnDestroy()
    {
        // Asker savaşta ölürse veya ordudan atılırsa, "Yeni Gün" aboneliğinden güvenlice çık!
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDay -= OnNewDay;
        }
    }
 void FinishTraining()
    {
        if (currentSpot == null) return;

        // 1. Temel stat kazanım miktarını al
        int kazanilanStat = currentSpot.statGain;

        // 2. Yetenekli Kontrolü (Daha güvenli bir şekilde yazıldı)
        bool isYetenekli = false;
        if (gladiator != null && gladiator.data != null)
        {
            if (gladiator.data.trait == SoldierTrait.Yetenekli)
            {
                isYetenekli = true;
                kazanilanStat *= 2; // Yetenekli asker 2 kat verim alır!
            }
        }

        // Hata ayıklama (Konsola yazdıracak)
        Debug.Log($"[EĞİTİM] Asker: {gladiator.data.gladiatorName} | Özellik: {gladiator.data.trait} | Kazanılan Puan: {kazanilanStat}");

        // 3. Hesaplanmış statı askere uygula
        ApplyStatGain(currentTrainingType, kazanilanStat, isYetenekli);

        // --- DİĞER GÜNCELLEMELER ---
        gladiator.data.level += 1;
        
        // ÖNEMLİ: Eğitim bittiği için maksimum canı da yeniliyoruz
        gladiator.RecalculateMaxHealth(); 

        currentSpot.isBusy = false;
        currentSpot = null;
        gladiator.SetIdle();
        DayManager.Instance.OnNewDay -= OnNewDay;
        
        gladiator.RefreshStats(); // UI Panelini yeniler

        if (levelUpParticle != null)
        {
            levelUpParticle.Play();
        }
    }

    // Parametreye 'bool isYetenekli' eklendi ki bildirimi daha rahat yapalım
    private void ApplyStatGain(TrainingType type, int finalAmount, bool isYetenekli)
    {
        GladiatorInventory inv = GetComponent<GladiatorInventory>();
        if (inv != null)
        {
            inv.PermanentlyIncreaseStat(type, finalAmount);
        }
        else
        {
            switch (type)
            {
                case TrainingType.Strength: gladiator.data.strength += finalAmount; break;
                case TrainingType.Speed: gladiator.data.speed += finalAmount; break;
                case TrainingType.Defense: gladiator.data.defense += finalAmount; break;
                case TrainingType.Stamina: gladiator.data.stamina += finalAmount; break;
            }
        }

        if (NotificationManager.Instance != null)
        {
            string traitVurgusu = isYetenekli ? " <color=#00FFFF>(Yetenekli!)</color>" : "";
            NotificationManager.Instance.Show($"Eğitim Tamamlandı! {type} +{finalAmount}{traitVurgusu}", NotificationType.Success);
        }
    }

    // ApplyStatGain fonksiyonunu da güncelledik ki bildirimi doğru sayıyla (Örn: +2) göstersin
    private void ApplyStatGain(TrainingType type, int finalAmount)
    {
        GladiatorInventory inv = GetComponent<GladiatorInventory>();
        if (inv != null)
        {
            inv.PermanentlyIncreaseStat(type, finalAmount);
        }
        else
        {
            switch (type)
            {
                case TrainingType.Strength: gladiator.data.strength += finalAmount; break;
                case TrainingType.Speed: gladiator.data.speed += finalAmount; break;
                case TrainingType.Defense: gladiator.data.defense += finalAmount; break;
                case TrainingType.Stamina: gladiator.data.stamina += finalAmount; break;
            }
        }

        if (NotificationManager.Instance != null)
        {
            // Eğer Yetenekli ise bunu ekranda vurgulamak oyuncunun hoşuna gider
            string traitVurgusu = (gladiator.data.trait == SoldierTrait.Yetenekli) ? " (Yetenekli!)" : "";
            NotificationManager.Instance.Show($"Eğitim Tamamlandı! {type} +{finalAmount}{traitVurgusu}", NotificationType.Success);
        }
    }

    int GetRequiredDays(int level)
    {
        if (level <= 5) return 1;
        if (level <= 10) return 2;
        return 3;
    }

    
}