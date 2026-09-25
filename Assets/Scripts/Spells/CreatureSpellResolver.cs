using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resuelve efectos sobre criaturas, recompensas y contraataques en su orden original.
/// </summary>
internal sealed class CreatureSpellResolver
{
    private readonly PlayerStatusView playerStatusView;
    private readonly SelectedCreatureView selectedCreatureView;
    private readonly CreatureSelectionManager creatureSelectionManager;
    private readonly CreaturePanel creaturePanel;
    private readonly IngredientDeck ingredientDeck;

    internal CreatureSpellResolver(
        PlayerStatusView playerStatusView,
        SelectedCreatureView selectedCreatureView,
        CreatureSelectionManager creatureSelectionManager,
        CreaturePanel creaturePanel,
        IngredientDeck ingredientDeck
    )
    {
        this.playerStatusView = playerStatusView;
        this.selectedCreatureView = selectedCreatureView;
        this.creatureSelectionManager = creatureSelectionManager;
        this.creaturePanel = creaturePanel;
        this.ingredientDeck = ingredientDeck;
    }

    internal void ResolveDamage(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        if (targetCreature == null)
        {
            Debug.Log("Este hechizo necesita una criatura objetivo.");

            return;
        }

        CreatureInstance creature = targetCreature.GetCreatureInstance();

        if (creature == null || creature.IsDead)
            return;

        int baseDamage = spellInstance.definition.GetEffectValue(spellInstance.level);

        int damage = SpellDamageCalculator.CalculateCreatureDamage(creature, baseDamage);

        creature.TakeDamage(damage);

        targetCreature.Refresh();

        if (selectedCreatureView != null)
        {
            selectedCreatureView.Refresh();
        }

        Debug.Log(
            $"{spellInstance.definition.spellName} "
                + $"hace {damage} de daño a "
                + $"{creature.definition.creatureName}."
        );

        // Si muere, damos recompensas y no contraataca
        if (creature.IsDead)
        {
            ResolveCreatureDeath(targetCreature, creature, caster);

            return;
        }

        // Si sobrevive, contraataca
        ResolveCounterAttack(creature, caster);
    }

    private void ResolveCounterAttack(CreatureInstance creature, PlayerState caster)
    {
        int damage = creature.definition.attack;

        int remainingDamage = damage;

        if (caster.shield > 0)
        {
            int absorbed = Mathf.Min(caster.shield, remainingDamage);

            caster.shield -= absorbed;
            remainingDamage -= absorbed;

            Debug.Log($"El Escudo Arcano absorbe {absorbed} de daño.");
        }

        if (remainingDamage > 0)
        {
            caster.currentHP -= remainingDamage;

            if (caster.currentHP < 0)
            {
                caster.currentHP = 0;
            }
        }

        Debug.Log(
            $"{creature.definition.creatureName} ataca por {damage}. "
                + $"Daño recibido en vida: {remainingDamage}."
        );

        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }
    }

    private void ResolveCreatureDeath(
        CreatureView creatureView,
        CreatureInstance creature,
        PlayerState caster
    )
    {
        CreatureDefinition definition = creature.definition;

        Debug.Log($"{definition.creatureName} ha sido derrotado.");

        caster.coins += definition.coinReward;

        caster.arcanePower += definition.arcanePowerReward;

        Debug.Log(
            $"Recompensa: +{definition.coinReward} monedas, "
                + $"+{definition.arcanePowerReward} Poder Arcano."
        );

        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }

        if (creatureSelectionManager != null)
        {
            creatureSelectionManager.ClearSelection();
        }

        if (creaturePanel != null)
        {
            creaturePanel.ReplaceCreature(creatureView);
        }
    }

    internal void ResolveDrain(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        if (targetCreature == null)
        {
            Debug.Log("Drenaje Vital necesita una criatura objetivo.");

            return;
        }

        CreatureInstance creature = targetCreature.GetCreatureInstance();

        if (creature == null || creature.IsDead)
            return;

        int baseDamage = spellInstance.definition.GetEffectValue(spellInstance.level);

        int damage = SpellDamageCalculator.CalculateCreatureDamage(creature, baseDamage);

        int healing = spellInstance.definition.GetSecondaryEffectValue(spellInstance.level);

        creature.TakeDamage(damage);

        caster.currentHP = Mathf.Min(caster.currentHP + healing, caster.maxHP);

        targetCreature.Refresh();

        if (selectedCreatureView != null)
        {
            selectedCreatureView.Refresh();
        }

        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }

        Debug.Log(
            $"{spellInstance.definition.spellName} " + $"hace {damage} de daño y cura {healing} PV."
        );

        if (creature.IsDead)
        {
            ResolveCreatureDeath(targetCreature, creature, caster);

            return;
        }

        ResolveCounterAttack(creature, caster);
    }

    internal void ResolveWindWhip(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        if (targetCreature == null)
        {
            Debug.Log("Látigo de Viento necesita una criatura objetivo.");

            return;
        }

        CreatureInstance creature = targetCreature.GetCreatureInstance();

        if (creature == null || creature.IsDead)
            return;

        int baseDamage = spellInstance.definition.GetEffectValue(spellInstance.level);

        int damage = SpellDamageCalculator.CalculateCreatureDamage(creature, baseDamage);

        creature.TakeDamage(damage);

        targetCreature.Refresh();

        if (selectedCreatureView != null)
        {
            selectedCreatureView.Refresh();
        }

        Debug.Log($"Látigo de Viento hace {damage} de daño.");

        // Nivel 3: roba 1 ingrediente
        if (spellInstance.level >= 3 && ingredientDeck != null)
        {
            ingredientDeck.DrawToPlayer(caster, 1);

            Debug.Log("Látigo de Viento Nv.3: robas 1 ingrediente.");
        }

        if (creature.IsDead)
        {
            ResolveCreatureDeath(targetCreature, creature, caster);

            return;
        }

        ResolveCounterAttack(creature, caster);
    }

    internal void ResolveRoots(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        if (targetCreature == null)
        {
            Debug.Log("Raíces de Tierra necesita una criatura objetivo.");

            return;
        }

        CreatureInstance creature = targetCreature.GetCreatureInstance();

        if (creature == null || creature.IsDead)
            return;

        int baseDamage = spellInstance.definition.GetEffectValue(spellInstance.level);

        int damage = baseDamage > 0 ? SpellDamageCalculator.CalculateCreatureDamage(creature, baseDamage) : 0;

        if (damage > 0)
        {
            creature.TakeDamage(damage);
        }

        targetCreature.Refresh();

        if (selectedCreatureView != null)
        {
            selectedCreatureView.Refresh();
        }

        Debug.Log(
            $"{spellInstance.definition.spellName} "
                + $"inmoviliza a {creature.definition.creatureName} "
                + $"y hace {damage} de daño."
        );

        if (creature.IsDead)
        {
            ResolveCreatureDeath(targetCreature, creature, caster);

            return;
        }

        // Raíces impide el contraataque
        Debug.Log(
            $"{creature.definition.creatureName} "
                + $"no puede contraatacar por efecto de Raíces de Tierra."
        );
    }

    internal void ResolveAcidExplosion(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        if (creaturePanel == null)
        {
            Debug.LogError("SpellResolver: CreaturePanel no está asignado.");

            return;
        }

        int damage = spellInstance.definition.GetEffectValue(spellInstance.level);

        List<CreatureView> creatures = creaturePanel.GetActiveCreatureViews();

        List<CreatureView> defeatedCreatures = new List<CreatureView>();

        foreach (CreatureView creatureView in creatures)
        {
            if (creatureView == null)
                continue;

            CreatureInstance creature = creatureView.GetCreatureInstance();

            if (creature == null || creature.IsDead)
                continue;

            creature.TakeDamage(damage);

            if (spellInstance.level >= 3 && !creature.IsDead)
            {
                creature.isCorroded = true;
            }

            creatureView.Refresh();

            Debug.Log(
                $"Explosión Ácida hace {damage} de daño a " + $"{creature.definition.creatureName}."
            );

            if (creature.IsDead)
            {
                defeatedCreatures.Add(creatureView);
            }
        }

        // Resolver las muertes después de recorrer todas
        foreach (CreatureView defeatedCreature in defeatedCreatures)
        {
            CreatureInstance creature = defeatedCreature.GetCreatureInstance();

            ResolveCreatureDeath(defeatedCreature, creature, caster);
        }

        if (selectedCreatureView != null)
        {
            selectedCreatureView.Refresh();
        }
    }
}
