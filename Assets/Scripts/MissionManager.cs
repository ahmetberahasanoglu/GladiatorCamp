using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;
    
    // Şu an seferde olan askerlerin listesi (Kampa geri dönemezler, eğitim yapamazlar)
    public List<Gladiator> soldiersOnMission = new List<Gladiator>();

    void Awake() { Instance = this; }

    public void SendOnMission(MissionData mission, List<Gladiator> squad)
    {
        // 1. Askerleri "Meşgul" işaretle
        foreach(var soldier in squad)
        {
            soldier.GetComponent<GladiatorTraining>().enabled = false; // Eğitimi kapa
            soldiersOnMission.Add(soldier);
            // Askerleri görsel olarak kampın kapısından çıkartabilirsin.
        }

        // 2. Sayacı başlat
        StartCoroutine(MissionProcess(mission, squad));
    }

    IEnumerator MissionProcess(MissionData mission, List<Gladiator> squad)
    {
        Debug.Log(mission.missionName + " seferi başladı! Süre: " + mission.durationDays + " gün.");

        // Basitçe saniye bekleyelim (İleride DayManager ile gün bazlı yaparsın)
        yield return new WaitForSeconds(mission.durationDays * 2); 

        ResolveMission(mission, squad);
    }

    void ResolveMission(MissionData mission, List<Gladiator> squad)
    {
        int totalPower = 0;
        foreach(var s in squad) totalPower += s.data.GetTotalStats();

        // Basit Savaş Mantığı: Güç > Zorluk + Şans
        int roll = Random.Range(0, 20); // Zar atma
        bool success = (totalPower + roll) >= mission.difficulty;

        if (success)
        {
            Debug.Log("ZAFER! Şanlı ordu ganimetle döndü.");
            MoneyManager.Instance.Add(mission.goldReward);
            // Askerlere XP ver
        }
        else
        {
            Debug.Log("HEZİMET! Yaralılar var...");
            // Askerlerin canını azalt veya öldür
        }

        // Askerleri serbest bırak
        foreach(var s in squad)
        {
            s.GetComponent<GladiatorTraining>().enabled = true;
            soldiersOnMission.Remove(s);
        }
    }
}