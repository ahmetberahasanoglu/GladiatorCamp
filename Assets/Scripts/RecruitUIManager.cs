using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

// Kart verilerini tutacak yapı
[System.Serializable]
public struct RecruitCardUI
{
    public GameObject cardObject; 
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText; 
    public TextMeshProUGUI traitText; 
    public TextMeshProUGUI costText;  
    public Button buyButton;
}

public class RecruitUIManager : MonoBehaviour
{
    public GameObject recruitPanel;

    [Header("3'lü Kademeli Kartlar")]
    // Eski contentParent ve slotPrefab SİLİNDİ, yerine 3'lü sabit kart listesi geldi.
    public List<RecruitCardUI> candidateCards; 

    void Start()
    {
        recruitPanel.SetActive(false);

        // RecruitManager'dan gelen "Asker Sayısı Değişti" sinyalini dinle
        // Biri satın alındığında kartlar anında güncellenecek
        if (RecruitManager.Instance != null)
        {
            RecruitManager.Instance.OnSoldierCountChanged += RefreshList;
        }
    }

    void OnDestroy()
    {
        // Hata almamak için obje yok olduğunda dinlemeyi bırak
        if (RecruitManager.Instance != null)
        {
            RecruitManager.Instance.OnSoldierCountChanged -= RefreshList;
        }
    }

    // Senin orijinal Aç/Kapat buton fonksiyonun birebir duruyor!
    public void TogglePanel()
    {
        recruitPanel.SetActive(!recruitPanel.activeSelf);
        if (recruitPanel.activeSelf)
        {
            RefreshList();
        }
    }

    // Eski Instantiate mantığı GİTTİ, yerine 3'lü Kart kontrolü GELDİ
    void RefreshList()
    {
        if (RecruitManager.Instance == null) return;

        List<RecruitCandidate> currentCandidates = RecruitManager.Instance.dailyCandidates;

        for (int i = 0; i < candidateCards.Count; i++)
        {
            RecruitCardUI card = candidateCards[i];

            // Eğer havuzda asker varsa kartı doldur ve aç
            if (i < currentCandidates.Count)
            {
                RecruitCandidate candidate = currentCandidates[i];
                card.cardObject.SetActive(true);

                card.nameText.text = candidate.candidateName;
                int staminaMultiplier = (candidate.trait == SoldierTrait.Obur) ? 15 : 10;
                int expectedHP = 100 + (candidate.potentialStamina * staminaMultiplier) + 5; 

                card.statsText.text = $"HP: <color=green>{expectedHP}</color>\n STR: {candidate.potentialStrength} | STA: {candidate.potentialStamina}";
                card.costText.text = $"{candidate.cost} Akçe";
                
                JanissaryData temp = ScriptableObject.CreateInstance<JanissaryData>();
                temp.trait = candidate.trait;
                card.traitText.text = temp.GetTraitDescription();

                // Parası yetmiyorsa buton basılamaz (gri) olsun
                bool canAfford = MoneyManager.Instance != null && MoneyManager.Instance.gold >= candidate.cost;
                card.buyButton.interactable = canAfford;

                // Butona satın alma görevini ata
                card.buyButton.onClick.RemoveAllListeners();
                card.buyButton.onClick.AddListener(() => {
                    RecruitManager.Instance.RecruitSoldier(candidate);
                    // Not: RecruitSoldier kodu çalıştığında "OnSoldierCountChanged" tetiklenir, 
                    // o da otomatik olarak bu RefreshList'i tekrar çağırıp satın alınan kartı ekrandan siler.
                });
            }
            else
            {
                // Aday satılmışsa o kartı gizle (Yerinde boşluk kalır, çok şık durur)
                card.cardObject.SetActive(false);
            }
        }
    }
}