using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BattleState { Idle, Fighting, Won, Lost }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Ayarlar")]
    public Transform playerSpawnPoint; // Senin askerlerin duracağı yer
    public Transform enemySpawnPoint;  // Düşmanın duracağı yer
    public Camera mainCamera;          
    
    [Header("Kamera Pozisyonları")]
    public Transform campCameraPos;    // Kampı gören kamera açısı
    public Transform battleCameraPos;  // Savaş alanını gören kamera açısı

    [Header("Durum")]
    public BattleState state = BattleState.Idle;

    void Awake()
    {
        Instance = this;
    }

    // MapEventManager'dan bu fonksiyon çağrılacak
    public void StartBattle(int enemyCount, int difficulty)
    {
        Debug.Log("Savaş Başlatılıyor...");
        
        // 1. Haritayı ve UI'ı kapat
        MapManager.Instance.HideMap();
        
        // 2. Kamerayı Savaş Alanına Taşı
        StartCoroutine(MoveCameraRoutine(battleCameraPos));
        Debug.Log("kamera tasınmalı");

        // 3. Askerleri Yerleştir (Spawn)
        SpawnArmies(enemyCount);

        // 4. Komutan UI'ını (Yetenekler) Aç
        // UIManager.Instance.ShowBattleUI(); (Bunu sonra yapacağız)
        
        state = BattleState.Fighting;
    }

    void SpawnArmies(int enemyCount)
    {
        // --- SENİN ORDUN ---
        // Kampındaki 'Müsait' askerleri bul ve savaş alanına taşı/kopyala
        var soldiers = FindObjectsOfType<Gladiator>();
        int offset = 0;
        foreach(var soldier in soldiers)
        {
            if(soldier.IsAvailableForWork()) // Savaşabilir durumda mı?
            {
                // Askeri savaş alanına ışınla (veya kopyasını oluştur)
                // Basitçe yan yana dizelim:
                soldier.transform.position = playerSpawnPoint.position + new Vector3(offset, 0, 0);
                soldier.transform.rotation = Quaternion.Euler(0, 0, 0); // Düşmana baksın
                offset += 2; // 2 birim arayla diz
            }
        }

        // --- DÜŞMAN ORDUSU ---
        // Burada düşman prefablarını instantiate edeceğiz
        // for(int i=0; i<enemyCount; i++) ...
        Debug.Log($"{enemyCount} düşman askeri sahaya indi!");
    }

    // Savaş bitince kampa dönme fonksiyonu
    public void EndBattle(bool isVictory)
    {
        state = isVictory ? BattleState.Won : BattleState.Lost;
        
        // Kamerayı kampa geri al
        StartCoroutine(MoveCameraRoutine(campCameraPos));
        
        // Haritayı tekrar aç (veya ganimet ekranı)
        if(isVictory)
        {
            NotificationManager.Instance.Show("Zafer! Savaş kazanıldı.", NotificationType.Success);
            MapManager.Instance.ShowMap();
        }
    }

    // Kamera geçişi için basit animasyon
    IEnumerator MoveCameraRoutine(Transform target)
    {
        float duration = 1.0f;
        float t = 0;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while(t < duration)
        {
            t += Time.deltaTime;
            mainCamera.transform.position = Vector3.Lerp(startPos, target.position, t / duration);
            mainCamera.transform.rotation = Quaternion.Lerp(startRot, target.rotation, t / duration);
            yield return null;
        }
        
        mainCamera.transform.position = target.position;
        mainCamera.transform.rotation = target.rotation;
             Debug.Log("kamera tasınmalı");
    }
}