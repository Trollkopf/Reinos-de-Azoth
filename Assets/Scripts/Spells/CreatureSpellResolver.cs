using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resuelve efectos sobre criaturas,
/// recompensas y contraataques.
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

        // Bola de Fuego Nv.3:
        // añade una acumulación independiente
        // de Quemadura durante 2 turnos.
        if (
            spellInstance.level >= 3
            && spellInstance.definition.appliesBurnAtLevel3
            && !creature.IsDead
            && creature.statusEffects != null
        )
        {
            creature.statusEffects.AddBurn(caster);

            Debug.Log(
                $"{creature.definition.creatureName} recibe "
                    + $"1 acumulación de Quemadura. "
                    + $"Total: {creature.statusEffects.GetBurnCount()}."
            );
        }

        RefreshCreatureViews(targetCreature);

        Debug.Log(
            $"{spellInstance.definition.spellName} "
                + $"hace {damage} de daño a "
                + $"{creature.definition.creatureName}."
        );

        if (creature.IsDead)
        {
            ResolveCreatureDeath(targetCreature, creature, caster);

            return;
        }

        ResolveCounterAttack(creature, caster);
    }

    private void ResolveCounterAttack(CreatureInstance creature, PlayerState caster)
    {
        if (creature.statusEffects != null && creature.statusEffects.rootedUntilEndOfTurn)
        {
            Debug.Log(
                $"{creature.definition.creatureName} " + "está enraizada y no puede contraatacar."
            );

            return;
        }

        int damage = creature.definition.attack;

        int remainingDamage = damage;

        if (caster.shield > 0)
        {
            int absorbed = Mathf.Min(caster.shield, remainingDamage);

            caster.RemoveShield(absorbed);

            remainingDamage -= absorbed;

            Debug.Log($"El Escudo Arcano absorbe {absorbed} de daño.");
        }

        if (remainingDamage > 0)
        {
            caster.TakeDamage(remainingDamage);
        }

        Debug.Log(
            $"{creature.definition.creatureName} "
                + $"ataca por {damage}. "
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

        caster.AddCoins(definition.coinReward);

        caster.AddArcanePower(definition.arcanePowerReward);

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

        caster.Heal(healing);

        RefreshCreatureViews(targetCreature);

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

        RefreshCreatureViews(targetCreature);

        Debug.Log($"Látigo de Viento hace {damage} de daño.");

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

        int damage =
            baseDamage > 0
                ? SpellDamageCalculator.CalculateCreatureDamage(creature, baseDamage)
                : 0;

        if (damage > 0)
        {
            creature.TakeDamage(damage);
        }

        if (creature.statusEffects != null)
        {
            creature.statusEffects.rootedUntilEndOfTurn = true;
        }

        RefreshCreatureViews(targetCreature);

        Debug.Log(
            $"{spellInstance.definition.spellName} "
                + $"enraíza a {creature.definition.creatureName} "
                + $"y hace {damage} de daño."
        );

        if (creature.IsDead)
        {
            ResolveCreatureDeath(targetCreature, creature, caster);

            return;
        }

        Debug.Log(
            $"{creature.definition.creatureName} "
                + "no podrá contraatacar durante el resto del turno."
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
            {
                continue;
            }

            creature.TakeDamage(damage);

            if (spellInstance.level >= 3 && !creature.IsDead)
            {
                creature.isCorroded = true;

                if (creature.statusEffects != null)
                {
                    creature.statusEffects.corroded = true;
                }
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

    private void RefreshCreatureViews(CreatureView creatureView)
    {
        if (creatureView != null)
        {
            creatureView.Refresh();
        }

        if (selectedCreatureView != null)
        {
            selectedCreatureView.Refresh();
        }
    }
}
