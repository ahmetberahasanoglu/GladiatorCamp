using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DummyInteract : MonoBehaviour
{
    [Header("Görsel ve Ses")]
    // DÜZELTME 1: Instantiate edeceğimiz için bunu ParticleSystem yerine GameObject yapıyoruz.
    // İsmine de Prefab ekledik ki Unity editöründe ne olduğu belli olsun.
    public GameObject hitParticlesPrefab; 
    
   // public AudioSource hitSound;          
    private Animator animator;

    private bool isHit = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;
        if (!isHit) StartCoroutine(WobbleRoutine());
    }
public void ReceiveHit()
    {
        if (!isHit) StartCoroutine(WobbleRoutine());
    }
    IEnumerator WobbleRoutine()
    {
        isHit = true; 

        // DÜZELTME 2: Güvenlik kontrolü ekledik ve GameObject olarak ürettik
        if (hitParticlesPrefab != null)
        {
            GameObject fx = Instantiate(hitParticlesPrefab, transform.position + Vector3.up * 2.5f, Quaternion.identity);
            Destroy(fx, 2f);
        }

       // if (hitSound != null) hitSound.Play();
       AudioManager.Instance.PlayWood();
        if (animator != null) animator.SetTrigger("hit");
        
        yield return new WaitForSeconds(0.2f); 
        
        isHit = false; 
    }
}