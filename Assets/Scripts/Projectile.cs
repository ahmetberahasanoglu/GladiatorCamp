using UnityEngine;
public class Projectile : MonoBehaviour
{
    public float speed = 15f;

    private Transform   _target;
    private float       _damage;
    private bool        _isCrit;
    private GladiatorAI _shooter;

    public void Setup(Transform target, float damage, bool isCrit, GladiatorAI shooterAI)
    {
        _target  = target;
        _damage  = damage;
        _isCrit  = isCrit;
        _shooter = shooterAI;
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (_target == null) { Destroy(gameObject); return; }

        Vector3 targetCenter = _target.position + Vector3.up * 1f;
        transform.position   = Vector3.MoveTowards(transform.position, targetCenter, speed * Time.deltaTime);
        transform.LookAt(targetCenter);

        if (Vector3.Distance(transform.position, targetCenter) < 0.5f)
        {
            OnHit();
            Destroy(gameObject);
        }
    }

    void OnHit()
    {
        // ── GÜVENLİK REVİZYONU: Alt objelere çarpsa bile InParent ile kök AI'ı bul ──
        GladiatorAI enemy = _target.GetComponentInParent<GladiatorAI>();
        if (enemy != null && !enemy.isDead)
        {
            enemy.TakeDamage(_damage, _isCrit);

            if (_shooter != null)
                _shooter.ProcessOnHitEffects(enemy, _damage);

            // ── ATICI ENERJİ KAZANIMI VE KONSOL TESTİ ──
            if (_shooter != null)
            {
                _shooter.GainEnergy(12f); // Okçular için isabet enerjisini 12'ye çıkardım (Dengeleme için)
                Debug.Log($"[Menzilli Enerji] Ok hedefi vurdu! Oku atan {_shooter.gameObject.name} enerji kazandı.");
            }
        }

        // ── Köylü Kontrolü ──
        VillagerNPC villager = _target.GetComponentInParent<VillagerNPC>();
        if (villager != null && !villager.IsDead)
        {
            villager.TakeDamage(_damage);
            if (_shooter != null) _shooter.GainEnergy(12f);
        }
    }
}