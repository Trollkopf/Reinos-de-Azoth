using UnityEngine;

/// <summary>Calcula el da?o a criaturas con la regla actual de corrosi?n.</summary>
internal static class SpellDamageCalculator
{
    internal static int CalculateCreatureDamage(CreatureInstance creature, int baseDamage)
    {
        int finalDamage = baseDamage;

        if (creature != null && creature.isCorroded)
        {
            finalDamage += 1;

            Debug.Log($"{creature.definition.creatureName} está corroído: +1 daño.");
        }

        return finalDamage;
    }
}
