using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ItemData item = (ItemData)target;

        // Temel verileri çiz
        item.itemID = EditorGUILayout.TextField("Item ID", item.itemID);
        item.itemName = EditorGUILayout.TextField("Item Name", item.itemName);
        item.type = (ItemType)EditorGUILayout.EnumPopup("Item Type", item.type);
        
        EditorGUILayout.Space();
        
        // SADECE SİLAH SEÇİLİYSE SİLAH ÖZELLİKLERİNİ GÖSTER
        if (item.type == ItemType.Weapon)
        {
            EditorGUILayout.LabelField("--- Silah Ayarları ---", EditorStyles.boldLabel);
            item.weaponClass = (WeaponClass)EditorGUILayout.EnumPopup("Weapon Class", item.weaponClass);
            item.weaponRange = EditorGUILayout.FloatField("Weapon Range", item.weaponRange);
            item.isRanged = EditorGUILayout.Toggle("Is Ranged", item.isRanged);
            EditorGUILayout.Space();
        }

        // Diğer tüm değişkenleri normal şekilde çizmeye devam et (Fiyat, ikon vs.)
        DrawPropertiesExcluding(serializedObject, "m_Script", "itemID", "itemName", "type", "weaponClass", "weaponRange", "isRanged");
        serializedObject.ApplyModifiedProperties();
    }
}