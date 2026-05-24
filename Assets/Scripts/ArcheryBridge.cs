using UnityEngine;

/// <summary>
/// Okçuluk sahnesi ve ana sahne arasında asker verisini güvenle taşır.
/// PlayerPrefs yerine static değişken kullanır — sahne geçişinde kaybolmaz.
///
/// KULLANIM:
///   Okçuluk görevini başlatmadan önce:
///       ArcheryBridge.SetSoldier(selectedSoldier);
///       SceneManager.LoadScene("ArcheryScene");
///
///   ArcheryGameManager.Start() içinde:
///       int speed = ArcheryBridge.SoldierSpeed;
///       string name = ArcheryBridge.SoldierName;
///
///   Okçuluk sahnesi biterken sonucu geri yaz:
///       ArcheryBridge.SetResult(won, score);
///       SceneManager.LoadScene("MainScene");
///
///   Ana sahnede geri döndükten sonra:
///       if (ArcheryBridge.HasResult) { bool won = ArcheryBridge.LastResultWon; ... }
/// </summary>
public static class ArcheryBridge
{
    // ── Sahneye gönderilen asker verisi ─────────────────────────────────
    public static int    SoldierSpeed  { get; private set; } = 20;
    public static int    SoldierLevel  { get; private set; } = 1;
    public static string SoldierName   { get; private set; } = "Asker";

    // ── Sahneden dönen sonuç ─────────────────────────────────────────────
    public static bool HasResult      { get; private set; } = false;
    public static bool LastResultWon  { get; private set; } = false;
    public static int  LastScore      { get; private set; } = 0;

    /// <summary>Okçuluk sahnesine geçmeden önce çağır.</summary>
    public static void SetSoldier(Gladiator soldier)
    {
        if (soldier == null || soldier.data == null) return;

        SoldierSpeed = soldier.data.speed;
        SoldierLevel = soldier.data.level;
        SoldierName  = soldier.data.gladiatorName;
        HasResult    = false;
    }

    /// <summary>Okçuluk sahnesi bitince, geri dönmeden önce çağır.</summary>
    public static void SetResult(bool won, int score)
    {
        LastResultWon = won;
        LastScore     = score;
        HasResult     = true;
    }

    /// <summary>Ana sahne sonucu işledikten sonra temizle.</summary>
    public static void ClearResult() => HasResult = false;
}
