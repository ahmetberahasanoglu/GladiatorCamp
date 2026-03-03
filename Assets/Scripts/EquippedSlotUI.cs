using UnityEngine;
using UnityEngine.UI;

public class EquippedSlotUI : MonoBehaviour
{
    public ItemType slotType; // Inspector'dan seç: Weapon, Armor vs.
    public Image iconImage;   // Eşyanın resmi
    public Sprite emptySprite; // Boşken görünecek siluet (Gri kılıç izi vs.)
    
    // YENİ: Slota tıklanmasını sağlayacak buton referansı
    public Button unequipButton; 
    
    private ItemData _currentItem; // Şu an takılı olan eşyayı hafızada tut

    public void Setup(ItemData item)
    {
        _currentItem = item;

        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white; // Görünür yap
            
            // YENİ: Eşya varsa buton aktif olsun, tıklanabilsin
            if (unequipButton != null) unequipButton.interactable = true; 
        }
        else
        {
            iconImage.sprite = emptySprite; // Boş siluet
            // iconImage.color = new Color(1,1,1,0.5f); // İstersen silueti biraz soluk yapabilirsin
            
            // YENİ: Eşya yoksa boş yere tıklanmasın
            if (unequipButton != null) unequipButton.interactable = false; 
        }

        // Buton dinleyicisini temizle ve yeniden ekle
        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveAllListeners();
            unequipButton.onClick.AddListener(OnUnequipClicked);
        }
    }

    // YENİ: Slota tıklandığında çalışacak fonksiyon
    void OnUnequipClicked()
    {
        if (_currentItem != null && InventoryUIManager.Instance != null)
        {
            // Yöneticiden bu slot türündeki eşyayı çıkartmasını iste
            InventoryUIManager.Instance.UnequipItem(slotType);
            
            // Tatlı bir tıklama veya kılıç kın sesi ekleyebilirsin
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }
    }
}