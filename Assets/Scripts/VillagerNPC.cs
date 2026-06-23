using UnityEngine;
using System;

public class VillagerNPC : MonoBehaviour
{
    [Header("Can")]
    public float maxHealth = 40f;
    public float currentHealth = 40f;

    [Header("Görsel & Animasyon")]
    public UnityEngine.UI.Slider healthBar; 
    public GameObject deadEffect; 
    public Animator animator; // YENİ: Animasyon kontrolü

    public event Action OnVillagerDied;
    private bool _isDead = false;
    public bool IsDead => _isDead;

    void Start()
    {
        currentHealth = maxHealth;
        
        // YENİ: Başlar başlamaz çömelip korkudan titreme animasyonunu tetikle
        if (animator != null)
        {
            animator.SetBool("isCowering", true); 
        }
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);

        if (healthBar != null) healthBar.value = currentHealth / maxHealth;

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (deadEffect != null) Instantiate(deadEffect, transform.position, Quaternion.identity);
        
        // YENİ: Ölüm animasyonunu tetikle
        if (animator != null) animator.SetTrigger("Die");

        OnVillagerDied?.Invoke();

        // 1.5 saniye bekle ki oyuncu ölüm animasyonunu görebilsin
        Destroy(gameObject, 1.5f);
    }
}