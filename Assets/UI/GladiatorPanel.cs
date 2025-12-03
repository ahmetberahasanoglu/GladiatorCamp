using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GladiatorPanel : MonoBehaviour
{
    public TextMeshProUGUI infoText;


    public void ShowInfo(JanissaryData data)
    {
        gameObject.SetActive(true);
        infoText.text =
            data.gladiatorName + "\n" +
            "STR: " + data.strength + "\n" +
            "DEF: " + data.defense + "\n" +
            "SPD: " + data.speed + "\n" +
            "MOR: " + data.morale+"\n" +
            "STA: " + data.stamina+"\n" +
            "Level" + data.level+"\n" +
            "Value" + data.GetSellValue();
    }
}
