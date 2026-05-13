using UnityEngine;

public enum CommanderSkillType { HealAll, DamageAllEnemies, StunEnemies }

[CreateAssetMenu(fileName = "NewCommanderSkill", menuName = "Game/Commander Skill")]
public class CommanderSkillData : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    public string description;
    
    public CommanderSkillType skillType; // Yeteneğin ne yapacağını belirler
    
    public float cooldownTime = 10f;
    public int powerAmount = 30; // Şifa ise ne kadar iyileştirecek, Ok ise ne kadar vuracak?
    
    public GameObject effectPrefab; // Ekranda çıkacak görsel efekt
    public bool isSingleUse = false; // Tek kullanımlık bir eşya mı?
}