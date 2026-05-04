using UnityEngine;
using TMPro;

public class ExpeditionBagUI : MonoBehaviour
{
    public static ExpeditionBagUI Instance;

    [Header("UI Referansı")]
    public TextMeshProUGUI bagContentText; // Resimdeki "New Text" objesi

    void Awake()
    {
        Instance = this;
    }

    // ExpeditionManager tarafından her loot alındığında çağrılacak
    public void UpdateBagUI(int gold, int rep, int itemCount)
    {
        if (bagContentText == null) return;

        // ÇANTA BOŞ DURUMU
        if (gold == 0 && rep == 0 && itemCount == 0)
        {
            bagContentText.text = "<color=#3E2723><b>SEFER ÇANTASI</b></color>\n<size=80%><i>Şu an çanta boş...\nİlerleyip ganimet topla.</i></size>";
            return;
        }

        // ÇANTA DOLU DURUMU
        string content = "<color=#3E2723><b>RİSKE ATILANLAR</b></color>\n\n";

        // Altın Kontrolü
        if (gold > 0) content += $"<color=#FFD700>+{gold} Akçe</color>\n";
        else if (gold < 0) content += $"<color=red>{gold} Akçe (Zarar)</color>\n";

        // İtibar Kontrolü
        if (rep > 0) content += $"<color=#4CAF50>+{rep} İtibar</color>\n";
        else if (rep < 0) content += $"<color=red>{rep} İtibar (Kötü Şöhret)</color>\n";

        // Eşya Kontrolü
        if (itemCount > 0) content += $"<color=#00BCD4>+{itemCount} Eşya</color>\n";

        bagContentText.text = content;
    }
}