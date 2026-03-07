using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    // Animasyon eventi bu objede (Alt objede) tetiklendiğinde burası çalışır
    public void OnTrainingHitDummy()
    {
        // 1. Bir üst objeye (Parent) çıkıp GladiatorTraining kodunu bul
        GladiatorTraining trainingScript = GetComponentInParent<GladiatorTraining>();
        
        // 2. Eğer bulduysa, asıl fonksiyonu orada çalıştır!
        if (trainingScript != null)
        {
            trainingScript.OnTrainingHitDummy();
        }
    }

    // İleride savaşırken hasar verme eventin (Örn: OnHit) olursa onu da buraya eklersin:
    // public void OnHit() { GetComponentInParent<GladiatorAI>().DealDamage(); }
}