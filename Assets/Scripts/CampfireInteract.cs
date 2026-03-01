using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CampfireInteract : MonoBehaviour
{
    [Header("Ateş Ayarları")]
    public ParticleSystem sparksParticle; // Tıklayınca fırlayacak kıvılcımlar
    public AudioSource flareSound;        // Ateşin harlanma sesi ("Pof!")


    private bool isSparking= true;


   

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        FlareRoutine();
    }

    void FlareRoutine()
    {

        if (isSparking) {
            sparksParticle.Stop();
            isSparking=false;
            flareSound.Play();
        }
        else
        {
            sparksParticle.Play();
            isSparking=true;
            flareSound.Play();
        }

    
    }
}