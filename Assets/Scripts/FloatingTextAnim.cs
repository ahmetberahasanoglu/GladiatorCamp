using UnityEngine;
using TMPro;

public class FloatingTextAnim : MonoBehaviour
{
    public float moveSpeed = 1.5f;   // Yukarı çıkma hızı
    public float destroyTime = 1.5f; // Ne kadar süre ekranda kalacak?
    public Vector3 offset = new Vector3(0, 0.5f, 0); // Vurulan yerin biraz üstünde çıksın

    private TextMeshPro textMesh;
    private Color textColor;
    private Transform cameraTransform;

    public void Setup(int score)
    {
        textMesh = GetComponent<TextMeshPro>();
        
        // Puanı yaz
        textMesh.text = "+" + score.ToString();

        // Puana göre renk değiştirme (İsteğe bağlı Cila)
        if (score >= 10) 
        {
            textMesh.fontSize = 6; // Büyük vuruş büyük yazı!
            textMesh.color = Color.yellow; // Altın
        }
        else if (score >= 5)
        {
            textMesh.fontSize = 5;
            textMesh.color = Color.white;
        }
        else
        {
            textMesh.fontSize = 4;
            textMesh.color = Color.gray;
        }
        
        textColor = textMesh.color;
        
        // Vurulan noktanın biraz üstüne taşı
        transform.position += offset;
    }

    void Start()
    {
        cameraTransform = Camera.main.transform;
        Destroy(gameObject, destroyTime); // Süre dolunca yok et
    }

    void Update()
    {
        // 1. Yukarı doğru süzül
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. Her zaman kameraya bak (Billboard Effect)
        if (cameraTransform != null)
        {
            transform.LookAt(transform.position + cameraTransform.forward);
        }

        // 3. Yavaşça şeffaflaş (Fade Out)
        if (textMesh != null)
        {
            float fadeSpeed = 1f / destroyTime;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
        }
    }
}