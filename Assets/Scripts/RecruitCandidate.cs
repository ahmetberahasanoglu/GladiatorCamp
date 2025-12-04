using UnityEngine;

[System.Serializable]
public class RecruitCandidate
{
    public string candidateName;
    public int potentialStrength;
    public int potentialStamina;
    public int cost;

    // Constructor'ı BOŞ bıraktık veya sildik.
    // Unity artık hata vermeyecek.
    
    public void InitializeRandomly()
    {
        string[] names = { "Ali", "Veli", "Hasan", "Mehmet", "Osman", "Murat", "Yusuf", "Hamza", "Kemal", "Selim" };
        candidateName = names[Random.Range(0, names.Length)];
        
        potentialStrength = Random.Range(1, 6); // 1 ile 5 arası
        potentialStamina = Random.Range(1, 6);
        
        // Fiyat hesaplaması
        cost = 100 + (potentialStrength * 20) + (potentialStamina * 10);
    }
}