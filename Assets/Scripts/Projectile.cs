using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    private Transform target;
    private float damage;
    private bool isCrit;
    
    // Okun kimden kime, ne kadar hasarla uçtuğunu kuran fonksiyon
    public void Setup(Transform target, float damage, bool isCrit)
    {
        this.target = target;
        this.damage = damage;
        this.isCrit = isCrit;
        
        // Ok çok uzağa giderse hafızayı doldurmasın diye 5 saniye sonra yok et
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

        // Hedefe Çarptı Mı?
        if (Vector3.Distance(transform.position, targetCenter) < 0.5f)
        {
            GladiatorAI enemy = target.GetComponent<GladiatorAI>();
            if (enemy != null && !enemy.isDead)
            {
                enemy.TakeDamage(damage, isCrit);
            }
            Destroy(gameObject); // Hasarı verince oku yok et
        }
    }
}