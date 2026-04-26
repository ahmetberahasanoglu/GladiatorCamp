using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    private Transform target;
    private float damage;
    private bool isCrit;
    private GladiatorAI shooter;
    
    // Okun kimden kime, ne kadar hasarla uçtuğunu kuran fonksiyon
   public void Setup(Transform target, float damage, bool isCrit, GladiatorAI shooterAI)
    {
        this.target = target;
        this.damage = damage;
        this.isCrit = isCrit;
        this.shooter = shooterAI; // Atan kişiyi kaydet
        Destroy(gameObject, 5f); 
    }

    void Update()
    {
        if (target == null) 
        {
            Destroy(gameObject); // Hedef yolda ölürse ok kaybolsun (veya yere düşsün)
            return;
        }

        // Hedefin merkezine (göğsüne) doğru uç
        Vector3 targetCenter = target.position + Vector3.up * 1f; 
        transform.position = Vector3.MoveTowards(transform.position, targetCenter, speed * Time.deltaTime);
        transform.LookAt(targetCenter);

        if (Vector3.Distance(transform.position, targetCenter) < 0.5f)
    {
        GladiatorAI enemy = target.GetComponent<GladiatorAI>();
        if (enemy != null && !enemy.isDead)
        {
            enemy.TakeDamage(damage, isCrit);
            
            // YENİ: Ok hedefe saplandığı an, atıcının Efsanevi Set etkilerini hedefe bulaştır!
            if (shooter != null) 
            {
                shooter.ProcessOnHitEffects(enemy, damage); 
            }
        }
        Destroy(gameObject);
    }
    }
}