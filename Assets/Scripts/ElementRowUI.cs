using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ElementRowUI : MonoBehaviour

{
    public static ElementRowUI instance;
    public Image            elementIcon;

    public TextMeshProUGUI   elementName;

    public TextMeshProUGUI   countText;

    public Image             matchArrow;    // İsteğe bağlı — null olabilir

      void Awake() => instance = this;

} 

