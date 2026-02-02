using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("UI Bağlantıları")]
    public GameObject mapPanel;       // Haritanın tamamını açıp kapatmak için
    public RectTransform playerIcon;  // Haritada bizi temsil eden "Atlı" veya "Otağ" ikonu

    [Header("Oyun Durumu")]
    public MapNode currentNode;       // Şu an üzerinde olduğumuz nokta (Null ise henüz başlamadık)
    public bool isMapOpen = true;     // Harita şu an açık mı?

    void Awake()
    {
        Instance = this;
    }

    // --- HARİTA HAREKET MANTIĞI ---

    // Bu fonksiyonu MapNode.cs içindeki OnNodeClicked çağırıyor
    public void SelectNode(MapNode targetNode)
    {
        // 1. Oraya gitmeye iznimiz var mı?
        if (!IsMoveValid(targetNode))
        {
            Debug.Log("Oraya gidemezsin! Sadece bağlı olduğun ileri noktalara gidebilirsin.");
            // Buraya "Hata Sesi" ekleyebilirsin
            return;
        }

        // 2. Hareket Onaylandı: Verileri Güncelle
        currentNode = targetNode;

        // 3. Görsel: Oyuncu ikonunu o noktaya taşı
        StartCoroutine(MovePlayerIconRoutine(targetNode.GetComponent<RectTransform>().anchoredPosition));

        // 4. Olayı Başlat (Köy, Savaş vb.)
        TriggerEvent(targetNode);
    }

    // Hareketin geçerli olup olmadığını kontrol eder
    bool IsMoveValid(MapNode target)
    {
        // Durum A: Oyun yeni başladı, henüz hiçbir yerde değiliz.
        // Sadece ilk katmandaki (Layer 0) noktalara gidebiliriz.
        if (currentNode == null)
        {
            return target.layerIndex == 0;
        }

        // Durum B: Zaten bir noktadayız.
        // Sadece şu anki noktanın "Gidilebilirler (accessibleNodes)" listesindekilere gidebiliriz.
        return currentNode.accessibleNodes.Contains(target);
    }

    // --- GÖRSEL EFEKTLER ---

    // İkonun kayarak gitmesi için ufak bir animasyon
    IEnumerator MovePlayerIconRoutine(Vector2 targetPos)
    {
        float duration = 0.5f; // Yarım saniyede gitsin
        float timer = 0;
        Vector2 startPos = playerIcon.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            playerIcon.anchoredPosition = Vector2.Lerp(startPos, targetPos, timer / duration);
            yield return null;
        }
        playerIcon.anchoredPosition = targetPos;
    }

    // --- OLAY YÖNETİMİ ---

    void TriggerEvent(MapNode node)
    {
        Debug.Log($"Gidilen Yer: {node.nodeType}. Gün İlerliyor...");

        // ÖNCE: Zamanı ilerlet (Yolculuk maliyeti)
        if(DayManager.Instance != null)
        {
            // Basitlik için her yolculuk 1 gün sürsün
            // İleride "Dağ yolu 3 gün" gibi detaylandırabiliriz
            DayManager.Instance.NextDay();//kac gün fln 
        }

        // SONRA: Noktanın türüne göre paneli aç
        switch (node.nodeType)
        {
            case NodeType.Village:
                Debug.Log("Köy Paneli Açılıyor...");
                // UIManager.Instance.ShowVillagePanel();
                break;

            case NodeType.Battle:
                Debug.Log("Savaş Hazırlığı Başlıyor...");
                // SceneManager.LoadScene("BattleScene"); veya BattleManager.Instance.SetupBattle();
                break;

            case NodeType.Boss:
                Debug.Log("KIZIL KALE! FİNAL SAVAŞI!");
                break;
                
            case NodeType.RestArea:
                Debug.Log("Dinlenme alanı. Askerler iyileşti.");
                // HealSoldiers();
                break;
        }
        
        // Not: Olay tetiklendikten sonra genelde harita kapanır ve olay ekranı gelir.
        // HideMap(); 
    }

    // --- UI KONTROLÜ ---
    public void ShowMap() 
    {
        mapPanel.SetActive(true);
        isMapOpen = true;
    }

    public void HideMap() 
    {
        mapPanel.SetActive(false);
        isMapOpen = false;
    }
}