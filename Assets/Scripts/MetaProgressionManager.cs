using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Yadigarların (Kalıcı Özelliklerin) Listesi
public enum RelicType 
{ 
    None,
    ZenginAta,     // Yeni sefere başlarken +150 Ekstra Altın
    KutluMide,     // Askerler %20 daha az erzak tüketir (veya erzaklar +10 gelir)
    DemirDovucu,   // Zırh yükseltmeleri veya eğitim %20 ucuzlar
    GaziKani,      // Askerler max canlarının +10'u ile doğar/başlar
    BereketliYol,  // Haritada %15 daha fazla altın bulunur
    CesurYurek     // Moraller savaş kaybedilse bile daha az düşer
}

// JSON'a dönüştürülecek Veri Sınıfı (Sadece bu sınıf kaydedilir)
[System.Serializable]
public class MetaSaveData
{
    public List<RelicType> unlockedRelics = new List<RelicType>();
}

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance;

    private string saveFilePath;
    public MetaSaveData currentSaveData = new MetaSaveData();

    // UI Paneli için referans (Sonraki adımda bağlayacağız)
    public RelicSelectionUI relicSelectionUI;
public int pendingRelicPicks = 0;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // JSON dosyasının bilgisayardaki/telefondaki güvenli yolunu belirle
        saveFilePath = Path.Combine(Application.persistentDataPath, "AtaYadigarlari.json");
        LoadMetaProgress();
    }

    // --- JSON KAYIT VE YÜKLEME ---
    public void SaveMetaProgress()
    {
        string json = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Miras (Ata Yadigarları) JSON'a Kaydedildi: " + saveFilePath);
    }

    public void LoadMetaProgress()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentSaveData = JsonUtility.FromJson<MetaSaveData>(json);
            Debug.Log("Miras Yüklendi! Toplam Yadigâr: " + currentSaveData.unlockedRelics.Count);
        }
        else
        {
            Debug.Log("Kayıt bulunamadı. Yeni bir miras defteri açılıyor.");
            currentSaveData = new MetaSaveData();
        }
    }

    // Belirli bir yadigara sahip miyiz? (Diğer scriptler buradan soracak)
    public bool HasRelic(RelicType type)
    {
        return currentSaveData.unlockedRelics.Contains(type);
    }
    public void AddPendingRelics(int count)
    {
        pendingRelicPicks += count;
        CheckPendingRelics(); // Kuyruğu kontrol etmeye başla
    }

    // Kuyrukta seçim hakkı varsa ekranı açar
    public void CheckPendingRelics()
    {
        if (pendingRelicPicks > 0)
        {
            TriggerRelicChoice();
        }
        else
        {
            if (relicSelectionUI != null) relicSelectionUI.ClosePanel();
        }
    }

    // --- YENİ YADİGAR SEÇİMİ (3 SEÇENEK) ---
    // ExpeditionManager'dan her 5 encounter'da bir burası çağrılacak
   private void TriggerRelicChoice()
    {
        List<RelicType> allRelics = System.Enum.GetValues(typeof(RelicType)).Cast<RelicType>().ToList();
        allRelics.Remove(RelicType.None);

        List<RelicType> availableRelics = allRelics.Where(r => !HasRelic(r)).ToList();

        if (availableRelics.Count == 0)
        {
            Debug.Log("Tüm yadigarlar açıldı!");
            pendingRelicPicks = 0; // Hepsini açtıysa kuyruğu sıfırla
            CheckPendingRelics();
            return;
        }

        List<RelicType> options = new List<RelicType>();
        int optionsCount = Mathf.Min(3, availableRelics.Count);

        for (int i = 0; i < optionsCount; i++)
        {
            int randomIndex = Random.Range(0, availableRelics.Count);
            options.Add(availableRelics[randomIndex]);
            availableRelics.RemoveAt(randomIndex); 
        }

        if (relicSelectionUI != null) relicSelectionUI.ShowOptions(options);
    }

    // Oyuncu seçimi yaptığında çağrılır
    public void UnlockRelic(RelicType chosenRelic)
    {
        if (!HasRelic(chosenRelic))
        {
            currentSaveData.unlockedRelics.Add(chosenRelic);
            SaveMetaProgress();
            
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Ata Yadigarı Kuşanıldı: " + chosenRelic.ToString(), NotificationType.Success);
            
            // YENİ: Seçim yapıldı, sıradakine geç!
            pendingRelicPicks--;
            CheckPendingRelics();
        }
    }

    public void ApplyStartingRelics()
    {
        // Örn: Zengin Ata yadigarı varsa kasaya para ekle
        if (HasRelic(RelicType.ZenginAta) && MoneyManager.Instance != null)
        {
            MoneyManager.Instance.Add(150);
            Debug.Log("Zengin Ata Yadigarı Devrede: +150 Başlangıç Altını!");
        }

        // Diğer başlangıç etkilerini buraya ekleyeceğiz...
    }
}