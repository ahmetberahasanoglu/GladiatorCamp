using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WorkSlotUI : MonoBehaviour
{
    [Header("UI Elemanları")]
    public TextMeshProUGUI nameText;     // "Ahmet"
    public TextMeshProUGUI wageText;     // "Gelir: 50"
    public Toggle workToggle;            // Checkbox (Tik kutusu)
    public Image statusIcon;             // Opsiyonel: Askerin sınıf ikonu vs.

    private Gladiator _soldier;

    public void Setup(Gladiator soldier)
    {
        _soldier = soldier;
        

        // İsim ve Ücreti Yaz
        nameText.text = soldier.data.gladiatorName;
        wageText.text = $"+{soldier.dailyWage} Akçe";

        // 1. Önce Toggle'ın dinleyicisini temizle (Eski eventler kalmasın)
        workToggle.onValueChanged.RemoveAllListeners();

        // 2. Askerin şu anki durumuna göre kutuyu dolu veya boş yap
        // Eğer Working ise kutu tikli (isOn = true) olsun
        workToggle.isOn = (_soldier.currentActivity == SoldierActivity.Working);

        // 3. Kullanıcı kutuya tıkladığında ne olacağını belirle
        workToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    // Kutucuğa her tıklandığında bu çalışır
    void OnToggleChanged(bool isChecked)
    {
        if (isChecked)
        {
            // Tik atıldı -> Çalışmaya gönder
            _soldier.SetActivity(SoldierActivity.Working);
             NotificationManager.Instance.Show(
                    $"{_soldier.data.gladiatorName} bugün çalışacak .", 
                    NotificationType.Info
                );
        }
        else
        {
            // Tik kaldırıldı -> Talime dönsün
            _soldier.SetActivity(SoldierActivity.Training);
            NotificationManager.Instance.Show(
                    $"{_soldier.data.gladiatorName} talime döndü.", 
                    NotificationType.Info
                );
    
        }
    }
}