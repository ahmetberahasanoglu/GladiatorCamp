using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// YENİ: Node'un 3 farklı durumunu tanımlıyoruz
public enum NodeState { Locked, Accessible, Cleared }

public class MapNode : MonoBehaviour
{
    [Header("Ayarlar")]
    public NodeType nodeType;
    public List<MapNode> outgoingPaths = new List<MapNode>(); 

    [Header("Durum")]
    public NodeState currentState = NodeState.Locked; // Varsayılan olarak hepsi kilitli başlar

    [Header("Görsel")]
    public Button nodeButton;
    public Image iconImage;

    void Start()
    {
        if (nodeButton != null)
        {
            nodeButton.onClick.RemoveAllListeners();
            nodeButton.onClick.AddListener(OnNodeClicked);
        }
    }

    // YENİ: Node'un rengini ve butonunu durumuna göre güncelleyen fonksiyon
    public void SetState(NodeState newState)
    {
        currentState = newState;

        if (iconImage == null || nodeButton == null) return;

        switch (currentState)
        {
            case NodeState.Locked:
                // Kilitli: Neredeyse siyah ve soluk
                iconImage.color = new Color(0.2f, 0.2f, 0.2f, 0.6f); 
                nodeButton.interactable = false;
                break;
            
            case NodeState.Accessible:
                // Gidilebilir: Tamamen parlak, orijinal renk
                iconImage.color = new Color(1f, 1f, 1f, 1f); 
                nodeButton.interactable = true;
                break;
            
            case NodeState.Cleared:
                // Geçilmiş: Yarı saydam, gölgede kalmış gibi
                iconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); 
                nodeButton.interactable = false;
                break;
        }
    }

    public void OnNodeClicked()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
        {
            if (TutorialManager.Instance.currentStep == TutorialStep.Intro_CampNode)
            {
                if (gameObject != TutorialManager.Instance.firstCampNodeUI) return; 
                TutorialManager.Instance.AdvanceTutorial(); 
            }
            else if (TutorialManager.Instance.currentStep == TutorialStep.Map_FirstBattle)
            {
                if (gameObject != TutorialManager.Instance.firstBattleNodeUI) return; 
                TutorialManager.Instance.AdvanceTutorial(); 
            }
            else
            {
                return; 
            }
        }
        
        if (MapManager.Instance != null)
        {
            MapManager.Instance.SelectNode(this);
        }
    }

    void OnDrawGizmos()
    {
        if (outgoingPaths == null || outgoingPaths.Count == 0) return;

        Gizmos.color = Color.yellow; 

        foreach (var node in outgoingPaths)
        {
            if (node != null)
            {
                Gizmos.DrawLine(transform.position, node.transform.position);
                Vector3 direction = (node.transform.position - transform.position).normalized;
                Gizmos.DrawSphere(Vector3.Lerp(transform.position, node.transform.position, 0.2f), 10f);
            }
        }
    }
}