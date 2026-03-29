using UnityEngine;
using UnityEngine.EventSystems;

public class BrawlEvent : MonoBehaviour
{
    private Gladiator fighter1;
    private Gladiator fighter2;
    private float timeLeft;
    private bool isResolved = false;

    // Manager bu kavgayı başlattığında çalışacak kurulum
    public void Setup(Gladiator s1, Gladiator s2, float duration)
    {
        fighter1 = s1;
        fighter2 = s2;
        timeLeft = duration;

        // Adamları durdur ve birbirlerine baktır ki kavga ettikleri anlaşılsın!
        if (fighter1 != null && fighter2 != null)
        {
            fighter1.transform.LookAt(fighter2.transform);
            fighter2.transform.LookAt(fighter1.transform);

            // Eğer varsa kılıç tokuşturma sesi çalabilirsin
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.swordHit, 0.8f);
        }
    }

    void Update()
    {
        if (isResolved) return;

        timeLeft -= Time.deltaTime;

        // Ünlem ikonunu yavaş yavaş kırmızılaştırıp büyüterek oyuncuyu paniğe sokabiliriz
        float pulse = Mathf.PingPong(Time.time * 2f, 0.2f);
        transform.localScale = Vector3.one * (1f + pulse);

        // Süre dolarsa ve oyuncu tıklamazsa kan dökülür!
        if (timeLeft <= 0)
        {
            ResolveBrawl(false);
        }
    }

    // Ünleme tıklandığında (Üzerinde Collider olması şart!)
    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject() || isResolved) return;
        
        AudioManager.Instance.PlayClick();
        ResolveBrawl(true); // Oyuncu araya girdi
    }

    private void ResolveBrawl(bool playerIntervened)
    {
        isResolved = true;

        if (fighter1 == null || fighter2 == null || fighter1.data == null || fighter2.data == null) 
        {
            Destroy(gameObject);
            return;
        }

        if (playerIntervened)
        {
            // BAŞARI: Uç Beyi araya girdi, disiplin sağlandı.
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("<color=yellow>DİSİPLİN!</color> Askerlerin arasına girdin ve kavgayı büyümeden ayırdın.", NotificationType.Info);
        }
        else
        {
            // BAŞARISIZLIK: Kan döküldü.
            fighter1.currentHealth -= 15f;
            fighter2.currentHealth -= 15f;
            
            // Can barlarını güncelle
            if (fighter1.healthBar != null) fighter1.healthBar.UpdateBar(fighter1.currentHealth, fighter1.maxHealth);
            if (fighter2.healthBar != null) fighter2.healthBar.UpdateBar(fighter2.currentHealth, fighter2.maxHealth);
            
            // Acı sesleri
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.gruntSound, 1f);
            
            // Moral düşüşü
            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-5);

            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("<color=red>KAN DÖKÜLDÜ!</color> Askerler birbirine girdi ve yaralandılar. Kamp morali düştü!", NotificationType.Error);
        }

        // İşi biten ünlemi yok et
        Destroy(gameObject);
    }
}