using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 originalPos;

    void Awake() 
    { 
        Instance = this; 
    }

    void OnEnable() 
    { 
        originalPos = transform.localPosition; 
    }

    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines(); // Üst üste binerse sıfırla
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos; // Süre bitince kamerayı yerine oturt
    }
}