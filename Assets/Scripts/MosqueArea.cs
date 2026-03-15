using UnityEngine;
using UnityEngine.EventSystems;

public class MosqueArea : MonoBehaviour
{
    [Header("İbadet Noktaları")]
    public PrayingSpot[] spots; // Caminin içindeki spotları Inspector'dan buraya sürükle

    void OnMouseDown()
    {
        // UI'a tıklanıyorsa veya harita açıksa iptal et
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (MapManager.Instance != null && MapManager.Instance.isMapOpen) return;

        // --- DÜZELTİLDİ: Senin GladiatorSelector sisteminden seçili askeri alıyoruz ---
        Gladiator selectedSoldier = null;
        if (GladiatorSelector.Instance != null)
        {
            selectedSoldier = GladiatorSelector.Instance.GetSelectedGladiator();
        }

        if (selectedSoldier != null)
        {
            AssignSoldierToPray(selectedSoldier);
        }
    }

  public void AssignSoldierToPray(Gladiator soldier)
    {
        // --- YENİ BARRİYER: TEK DOĞRU KAYNAK KONTROLÜ ---
        if (!soldier.IsAvailableForTask())
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("Bu asker müsait değil! Önce mevcut işini bitirmeli.", NotificationType.Warning);
            
            if (GladiatorSelector.Instance != null)
                GladiatorSelector.Instance.ClearSelection();
            
            return; // Kodu burada durdur
        }

        // Boş bir seccade bul
        foreach (var spot in spots)
        {
            if (!spot.isOccupied)
            {
                GladiatorPraying prayingScript = soldier.GetComponent<GladiatorPraying>();
                if (prayingScript == null) prayingScript = soldier.gameObject.AddComponent<GladiatorPraying>();

                // YENİ: Askerin durumunu "İbadette" olarak kilitle!
                soldier.SetActivity(SoldierActivity.Praying);

                prayingScript.StartPraying(spot);
                
                if (GladiatorSelector.Instance != null)
                {
                    GladiatorSelector.Instance.ClearSelection();
                }
                return;
            }
        }

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.Show("Camide boş yer yok!", NotificationType.Warning);
    }
}