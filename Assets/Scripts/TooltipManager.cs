using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    /*
    public static TooltipManager Instance;

    public GameObject tooltipPanel;
    public TextMeshProUGUI headerText; // Eşya Adı
    public TextMeshProUGUI bodyText;   // Statlar (Güç: +5 vs)
    
    void Awake() { Instance = this; }

    void Start() { HideTooltip(); }

    void Update()
    {
        // Kutucuk mouse'u takip etsin
        if (tooltipPanel.activeSelf)
        {
            tooltipPanel.transform.position = Input.mousePosition;
        }
    }

    public void ShowTooltip(ItemData item, GladiatorInventory currentStats)
    {
        tooltipPanel.SetActive(true);
        headerText.text = item.itemID;
        
        // KIYASLAMA MANTIĞI
        // Şu an üstümde ne var?
        ItemData equipped = null;
        if (item.type == ItemType.Weapon) equipped = currentStats.weapon;
        else if (item.type == ItemType.Armor) equipped = currentStats.armor;
        // ... diğer tipler ...

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"Fiyat: {item.price}");
        sb.AppendLine("----------------");
        
        // Statları kıyaslayarak yaz
        sb.Append(GetStatLine("STR", item.bonusStrength, equipped ? equipped.bonusStrength : 0));
        sb.Append(GetStatLine("DEF", item.bonusDefense, equipped ? equipped.bonusDefense : 0));
        sb.Append(GetStatLine("SPD", item.bonusSpeed, equipped ? equipped.bonusSpeed : 0));

        bodyText.text = sb.ToString();
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    // Yardımcı: Renkli Kıyaslama Satırı Üretir
    string GetStatLine(string statName, int newVal, int oldVal)
    {
        if (newVal == 0 && oldVal == 0) return ""; // İkisi de 0 ise yazma

        int diff = newVal - oldVal;
        string color = diff > 0 ? "green" : (diff < 0 ? "red" : "white");
        string sign = diff > 0 ? "+" : "";

        // Örnek Çıktı: STR: 15 (+5) [Yeşil]
        return $"{statName}: {newVal} <color={color}>({sign}{diff})</color>\n";
    }*/
}