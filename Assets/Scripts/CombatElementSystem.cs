using UnityEngine;

public enum ElementType { Normal, Inanc, Ates, Zehir }

public static class CombatElementSystem
{
    public static float GetDamageMultiplier(ElementType attackerType, ElementType defenderType)
    {
        if (attackerType == ElementType.Inanc && defenderType == ElementType.Zehir) return 2.0f; 
        if (attackerType == ElementType.Zehir && defenderType == ElementType.Ates) return 2.0f;
        if (attackerType == ElementType.Ates && defenderType == ElementType.Inanc) return 2.0f;

        // Dirençler (Çok Etkili Değil) - İsteğe bağlı
        if (attackerType == ElementType.Zehir && defenderType == ElementType.Inanc) return 0.5f; 
        if (attackerType == ElementType.Ates && defenderType == ElementType.Zehir) return 0.5f;
        if (attackerType == ElementType.Inanc && defenderType == ElementType.Ates) return 0.5f;

        // Aynı türler birbirine az vurur
        if (attackerType == defenderType && attackerType != ElementType.Normal) return 0.5f;

        return 1.0f; // Normal hasar
    }
}