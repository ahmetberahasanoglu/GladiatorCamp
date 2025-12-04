using UnityEngine;

[System.Serializable]
public class RecruitCandidate
{
    public string candidateName;
    public int potentialStrength; // Askerin başlangıç gücü
    public int potentialStamina;
    public int cost;              // Devşirme bedeli (Yol parası, ailesine ödeme vb.)
    
    // Rastgele isim ve stat üretmek için yapıcı metod
    public RecruitCandidate()
    {
        string[] names = { "Ali", "Veli", "Hasan", "Mehmet", "Osman", "Murat", "Yusuf", "Hamza" };
        candidateName = names[Random.Range(0, names.Length)];
        
        potentialStrength = Random.Range(1, 6); // 1 ile 5 arası
        potentialStamina = Random.Range(1, 6);
        
        // Güçlü asker daha pahalı olsun
        cost = 100 + (potentialStrength * 20) + (potentialStamina * 10);
    }
}