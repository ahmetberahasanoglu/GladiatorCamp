using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class WanderingStranger : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private Transform exitPoint;
    
    [Header("Durum")]
    public float waitTimeInCamp = 30f; 
    public float standUpTime = 2.0f;
    private bool isInteracting = false;
    private bool isLeaving = false;

    [Header("Teklif Detayları")]
    public string strangerName;
    [TextArea] public string offerText;
    public int cost;
    public int eventType; 

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 2.5f; 
        anim = GetComponentInChildren<Animator>();
    }

    public void Setup(Transform campPoint, Transform exit, int randomType)
    {
        exitPoint = exit;
        eventType = randomType;

        // KİMLİKLERİN EŞLEŞTİRİLMESİ
        switch (eventType)
        {
            case 0:
                strangerName = "Gizemli İksirci";
                offerText = "Uzak diyarlardan geldim. 50 Akçe karşılığında orduna geçici bir güç şerbeti verebilirim. İster misin?";
                cost = 50;
                break;
            case 1:
                strangerName = "Yaralı Savaşçı";
                offerText = "Eski birliğim pusuya düştü. Yaralarımı sarman için 100 Akçe verirsen, kılıcım senindir Uç Beyi!";
                cost = 100;
                break;
            case 2:
                strangerName = "Oduncu";
                offerText = "Kış çetin geçecek Beyim. 50 Akçe verirsen sana 30 Odun bırakırım.";
                cost = 50;
                break;
            case 3:
                // --- DERVİŞ İMTİHANI (SADAKA) ---
                strangerName = "Yolcu Derviş";
                offerText = "\"Allah rızası için yolda kalmışa bir sadaka Uç Beyim... Kesenin ucu açıksa 50 Akçe verebilir misin?\"";
                cost = 50;
                break;
            case 4:
                // --- KURNAZ TÜCCAR (KUL HAKKI İMTİHANI) ---
                strangerName = "Kurnaz Tüccar";
                offerText = "\"Beyim, ilerideki köyde açlıktan kırılıyorlar. Elimdeki 50 Erzağı onlara değil de sana satarım... ama 150 Akçe isterim! Alır mısın?\"";
                cost = 150;
                break;
            case 5:
                // --- HIZIR ALEYHİSSELAM (BEDAVA MUCİZE) ---
                strangerName = "<color=#FFD700>Nur Yüzlü İhtiyar</color>";
                offerText = "\"Yüreğin temiz, nasibin gür imiş Uç Beyi. Zalime boyun eğmedin, mazlumu ezmedin. Bu kutlu yadigar asırlardır doğru sahibini beklerdi... Al, senin olsun.\"\n\n<size=80%><color=green>(Kabul Etmek Kampa Efsanevi Bir Eşya Kazandırır!)</color></size>";
                cost = 0; // Bedava!
                break;
        }

        // --- 1. YÜRÜYÜŞ BAŞLIYOR ---
        if (anim != null) anim.SetBool("isWalking", true);
        agent.SetDestination(campPoint.position);
        StartCoroutine(CampRoutine());
    }

    IEnumerator CampRoutine()
    {
        while (agent.pathPending || agent.remainingDistance > 0.5f) yield return null;

        agent.isStopped = true;
        
        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isSitting", true);
        }

        float timer = 0;
        while (timer < waitTimeInCamp)
        {
            if (!isInteracting) timer += Time.deltaTime;
            
            if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle)
            {
                LeaveCamp();
                yield break;
            }
            yield return null;
        }

        LeaveCamp();
    }

    public void LeaveCamp()
    {
        if (isLeaving) return;
        isLeaving = true;
        isInteracting = false;
        
        if (NotificationManager.Instance != null) 
            NotificationManager.Instance.Show($"{strangerName} kamptan ayrıldı.", NotificationType.Info);

        StartCoroutine(LeaveRoutine());
    }

    IEnumerator LeaveRoutine()
    {
        if (anim != null) anim.SetBool("isSitting", false);
        yield return new WaitForSeconds(standUpTime);
        if (anim != null) anim.SetBool("isWalking", true);
        
        agent.isStopped = false;
        agent.SetDestination(exitPoint.position);
        
        StartCoroutine(DestroyWhenArrived());
    }

    IEnumerator DestroyWhenArrived()
    {
        while (agent.pathPending || agent.remainingDistance > 0.5f) yield return null;
        Destroy(gameObject);
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject() || isLeaving) return;
        if (BattleManager.Instance != null && BattleManager.Instance.state != BattleState.Idle) return;

        isInteracting = true;
        StrangerUIManager.Instance.OpenOfferPanel(this);
    }

    public void AcceptOffer()
    {
        // 1. ÖNCE KAPASİTE KONTROLÜ (Eğer gelen kişi Asker ise)
        if (eventType == 1)
        {
            Gladiator[] allSoldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
            int mySoldierCount = 0;
            foreach (var soldier in allSoldiers)
            {
                if (soldier.CompareTag("MySoldier")) mySoldierCount++;
            }

            int maxCapacity = CampManager.Instance != null ? CampManager.Instance.GetMaxSoldierCapacity() : 3;

            if (mySoldierCount >= maxCapacity)
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.Show($"Kışlada boş yatak yok! ({mySoldierCount}/{maxCapacity})", NotificationType.Warning);
                
                StrangerUIManager.Instance.ClosePanel();
                isInteracting = false; 
                return; 
            }
        }

        if (eventType == 5)
        {

            string[] legendaryItemIDs = { "Zulfikar", "Zırh-ı Cevşen", "Kıpçak Miğferi" };
            string selectedID = legendaryItemIDs[Random.Range(0, legendaryItemIDs.Length)];
            
            if (ItemDatabase.Instance != null && InventoryStorage.Instance != null)
            {
                ItemData givenItem = ItemDatabase.Instance.GetItemByID(selectedID);
                
                if (givenItem != null)
                {
                    InventoryStorage.Instance.AddItem(givenItem);
                    if (NotificationManager.Instance != null) 
                        NotificationManager.Instance.Show($"<color=yellow>MUCİZE!</color> İhtiyar kayboldu... Geriye <color=#FFD700>[{givenItem.itemID}]</color> kaldı!", NotificationType.Success);
                }
            }

            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(50);
            if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(NasipManager.Instance.maxNasip); 

            StrangerUIManager.Instance.ClosePanel();
            LeaveCamp();
            return;
        }

        if (MoneyManager.Instance.Spend(cost))
        {
            if (eventType == 0) // İKSİRCİ
            {
                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(15); 
                if (NotificationManager.Instance != null) NotificationManager.Instance.Show("İksir işe yaradı! Ordunun morali arttı (+15).", NotificationType.Success);
            }
            else if (eventType == 1) // YARALI ASKER
            {
                if (RecruitManager.Instance != null)
                {
                    GameObject newObj = Instantiate(RecruitManager.Instance.soldierPrefab, RecruitManager.Instance.soldierSpawnPoint.position, Quaternion.identity);
                    Gladiator glad = newObj.GetComponent<Gladiator>();
                    
                    JanissaryData newData = ScriptableObject.CreateInstance<JanissaryData>();
                    newData.gladiatorName = "Gezgin Savaşçı";
                    newData.strength = Random.Range(3, 6); 
                    newData.stamina = Random.Range(3, 6);
                    newData.defense = 2; 
                    newData.speed = 2;
                    newData.level = 2; 

                    glad.InitializeData(newData);
                    RecruitManager.Instance.RefreshUI();
                }
                if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Aramıza hoş geldin! Kışlaya tecrübeli bir savaşçı katıldı.", NotificationType.Success);
            }
            else if (eventType == 2) // ODUNCU
            {
                if (ResourceManager.Instance != null) ResourceManager.Instance.AddWood(30);
                if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Kampa 30 Odun bırakıldı.", NotificationType.Success);
            }
            else if (eventType == 3) // DERVİŞ (SADAKA - İYİ KARMA)
            {
                if (NasipManager.Instance != null) NasipManager.Instance.AddNasip(2); 
                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(10);
                if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Derviş sana dualar ederek ayrıldı. Nasibin arttı!", NotificationType.Success);
            }
            else if (eventType == 4) // KURNAZ TÜCCAR (KÖTÜ KARMA)
            {
                if (SupplyManager.Instance != null) SupplyManager.Instance.AddFood(50);
                if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(2); // Köyün rızkını çaldın!
                if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-10); // Askerler bu ticaretten iğrendi
                if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Erzak aldın ama köyün ahını aldın! (Nasip ve Moral Düştü)", NotificationType.Warning);
            }

            StrangerUIManager.Instance.ClosePanel();
            LeaveCamp();
        }
        else
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Yeterli Akçen yok!", NotificationType.Error);
            isInteracting = false; 
        }
    }

    public void DeclineOffer()
    {
        // Reddedildiğinde ceza veya uyarı vereceklerimiz:
        if (eventType == 3) // DERVİŞİ REDDETMEK
        {
            if (NasipManager.Instance != null) NasipManager.Instance.SpendNasip(1); 
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Dervişi boş çevirdin. Bereketi kaçtı... (Nasip Azaldı)", NotificationType.Warning);
        }
        else if (eventType == 5) // HIZIRI REDDETMEK (Oyuncu manyak olmalı)
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("İhtiyar tebessüm edip eşyalarını topladı ve ormanda sise karıştı.", NotificationType.Info);
        }

        StrangerUIManager.Instance.ClosePanel();
        LeaveCamp();
    }
}