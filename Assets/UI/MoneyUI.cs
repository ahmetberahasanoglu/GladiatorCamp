using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;

    void Start()
    {
        MoneyManager.Instance.OnGoldChanged += UpdateUI;
        UpdateUI(MoneyManager.Instance.gold);
    }

    void UpdateUI(int gold)
    {
        goldText.text = gold + " G";
    }
}
