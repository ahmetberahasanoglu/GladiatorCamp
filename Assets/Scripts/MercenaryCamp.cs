using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Tıklanınca MercenaryCampUI panelini açar.
/// Sadece standart, sabit statlara sahip bir asker satar.
/// </summary>
public class MercenaryCamp : MonoBehaviour
{
    [Header("Asker Ayarları")]
    public int mercenaryCost = 100;
    public GameObject mercenaryPrefab;
    public Transform  spawnPoint;

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;

        MercenaryCampUI.Instance?.Open(this);
    }


    public MercenaryData GetBasicMercenary()
    {
        return new MercenaryData
        {
            soldierName = "Paralı Asker", 
            strength    = 1,             
            defense     = 1,
            speed       = 1,
            stamina     = 1,
            trait       = SoldierTrait.Siradan,
            cost        = mercenaryCost
        };
    }

    // ── Satın Alma ────────────────────────────────────────────────────────
    public bool TryHire(MercenaryData data)
    {
        // 1. Kapasite kontrolü
        int capacity     = CampManager.Instance != null ? CampManager.Instance.GetMaxSoldierCapacity() : 3;
        int soldierCount = CampManager.Instance != null ? CampManager.Instance.GetLivingSoldiers().Count : 0;

        if (soldierCount >= capacity)
        {
            NotificationManager.Instance?.Show(
                $"Koğuş dolu! Maksimum {capacity} asker alabilirsin.",
                NotificationType.Error);
            return false;
        }

        // 2. Para kontrolü
        if (MoneyManager.Instance.gold < data.cost)
        {
            NotificationManager.Instance?.Show(
                $"Yeterli akçen yok! Gerekli: {data.cost} Akçe.",
                NotificationType.Error);
            return false;
        }

        // 3. Satın al
        MoneyManager.Instance.Spend(data.cost);

        GameObject newSoldier = Instantiate(mercenaryPrefab, spawnPoint.position, Quaternion.identity);
        Gladiator glad = newSoldier.GetComponent<Gladiator>();
        if (glad != null && glad.data != null)
        {
            glad.data.gladiatorName  = data.soldierName;
            glad.data.strength       = data.strength;
            glad.data.defense        = data.defense;
            glad.data.speed          = data.speed;
            glad.data.stamina        = data.stamina;
            glad.data.trait          = data.trait;
            glad.data.level          = 1;
            glad.RecalculateMaxHealth();
        }

        NotificationManager.Instance?.Show(
            $"Bir {data.soldierName} saflarına katıldı! (-{data.cost} Akçe)",
            NotificationType.Success);

        TopInfoBarUI.Instance?.UpdateCapacity();

        // Paneli güncelle (Kapasite değiştiği için UI yenilenmeli)
        MercenaryCampUI.Instance?.Refresh();

        return true;
    }
}   