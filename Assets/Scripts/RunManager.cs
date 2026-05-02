using UnityEngine;
using TMPro;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    [Header("Sürgün UI")]
    public GameObject exilePanel;
    public TextMeshProUGUI exileDetailText;

    void Awake()
    {
        Instance = this;
        if (exilePanel != null) exilePanel.SetActive(false);
    }

    void Start()
    {
        // ReputationManager'daki "OnGameOver" eventini dinlemeye başlıyoruz!
        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.OnGameOver += TriggerExile;
        }
    }

    void OnDestroy()
    {
        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.OnGameOver -= TriggerExile;
        }
    }

    // İTİBAR 0 OLDUĞUNDA BU FONKSİYON TETİKLENİR!
    public void TriggerExile()
    {
        Debug.Log("<color=red>SÜRGÜN FERMANI GELDİ! Oyun sıfırlanıyor...</color>");

        // 1. Sürgün Panelini Aç
        if (exilePanel != null) exilePanel.SetActive(true);
        if (exileDetailText != null) exileDetailText.text = "Padişahın fermanı kampa ulaştı...\n\nİtibarın yerle bir oldu. Ordun dağıtıldı, hazinene el konuldu. Tüm kazandıklarını geride bırakıp sürgüne yollanıyorsun...\n\nGeriye sadece atalarından kalan yadigarlar kaldı.";

        // 2. Haritadaki geçici çantayı yok et
        if (ExpeditionManager.Instance != null) ExpeditionManager.Instance.FailExpedition();

        // 3. KAMPTAKİ TÜM ASKERLERİ SİL! (Roguelite Ceza)
        GameObject[] mySoldiers = GameObject.FindGameObjectsWithTag("MySoldier");
        foreach (var soldier in mySoldiers)
        {
            Destroy(soldier);
        }

        // 4. Ekonomiyi ve İtibarı Başlangıç Ayarlarına Döndür
        ResetEconomy();

        // 5. Haritayı baştan yarat ve rastgeleleştir!
        if (MapRandomizer.Instance != null) MapRandomizer.Instance.RandomizeMap();
        if (MapManager.Instance != null) MapManager.Instance.ResetMapProgress();

        // Not: İleride bu panelin üzerine "Miras (Ata Yadigarı) Dükkanına Git" butonu koyacağız!
    }

    private void ResetEconomy()
    {
        // Altını sıfırla ve başlangıç altını (örn: 200) ver
        if (MoneyManager.Instance != null)
        {
            int currentGold = MoneyManager.Instance.gold;
            MoneyManager.Instance.Spend(currentGold); // Önce kasayı boşalt
            MoneyManager.Instance.Add(1000); // Başlangıç parası
        }

        // İtibarı tekrar 80'e (veya başlangıca) ayarla
        if (ReputationManager.Instance != null)
        {
            int currentRep = ReputationManager.Instance.GetReputation();
            // GetReputation 0 dönecektir ama garanti olsun diye aradaki farkı ekliyoruz
            ReputationManager.Instance.ChangeReputation(80 - currentRep); 
        }

        // Morali ve Erzağı da sıfırlayabilirsin (SupplyManager varsa ekle)
        if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(50 - CampMoraleManager.Instance.currentMorale);

       
    if (MetaProgressionManager.Instance != null)
    {
        MetaProgressionManager.Instance.ApplyStartingRelics();
    }
    }
}