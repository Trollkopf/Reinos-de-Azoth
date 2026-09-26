using UnityEngine;

/// <summary>
/// Calcula el daño final que recibe una criatura
/// teniendo en cuenta resistencias y estados.
/// </summary>
internal static class SpellDamageCalculator
{
    internal static int CalculateCreatureDamage(
        CreatureInstance creature,
        SpellDefinition spell,
        int baseDamage
    )
    {
        int finalDamage = baseDamage;

        if (creature == null || creature.definition == null)
        {
            return Mathf.Max(0, finalDamage);
        }

        // Corrosión:
        // la criatura recibe +1 de daño
        // de cada hechizo ofensivo.
        if (creature.statusEffects != null && creature.statusEffects.corroded)
        {
            finalDamage += 1;

            Debug.Log($"{creature.definition.creatureName} " + "está corroído: +1 daño.");
        }

        // Enemigos resistentes al fuego
        bool resistsFire =
            creature.definition.ability == CreatureAbility.FireResistance
            || creature.definition.ability == CreatureAbility.SulfurDragonFireResistance;

        if (resistsFire && spell != null && spell.appliesBurnAtLevel3)
        {
            finalDamage -= 1;

            Debug.Log($"{creature.definition.creatureName} " + "resiste el fuego: -1 daño.");
        }

        if (creature.definition.ability == CreatureAbility.DamageReduction)
        {
            finalDamage -= 1;

            Debug.Log($"{creature.definition.creatureName} " + "reduce el daño recibido en 1.");
        }

        return Mathf.Max(0, finalDamage);
    }
}
