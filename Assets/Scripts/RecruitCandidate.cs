using UnityEngine;

[System.Serializable]
public class RecruitCandidate
{
    public string candidateName;
    public int potentialStrength;
    public int potentialStamina;
    public SoldierTrait trait; 
    public int cost;

    public void InitializeRandomly()
    {
        string[] names = { "Ali", "Veli", "Hasan", "Mehmet", "Osman", "Murat", "Yusuf", "Hamza", "Kemal", "Selim", "Mustafa", "Fatih" };
        candidateName = names[Random.Range(0, names.Length)];
        
        potentialStrength = Random.Range(1, 6); 
        potentialStamina = Random.Range(1, 6);
        
        // 1. Rastgele Kişilik Ata
  int roll = Random.Range(0, 100);
        if (roll < 40) trait = SoldierTrait.Siradan;      
        else if (roll < 60) trait = SoldierTrait.Obur;    
        else if (roll < 80) trait = SoldierTrait.Dindar;  
        else trait = SoldierTrait.Yetenekli;          
        
        // 2. Temel Fiyat Hesaplaması
      // Fiyat Hesaplaması
        cost = 100 + (potentialStrength * 20) + (potentialStamina * 15);

        switch(trait)
        {
            case SoldierTrait.Yetenekli: cost += 150; break;
            case SoldierTrait.Siradan: cost -= 40; break;
            case SoldierTrait.Obur: cost -= 20; break; // Ucuz ama erzak masrafı çok
            case SoldierTrait.Dindar: cost += 50; break;
        }
        
        cost = Mathf.Max(50, cost);
    }
}