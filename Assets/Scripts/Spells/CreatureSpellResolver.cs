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
    private readonly MarketView marketView;
    private readonly MarketPanelController marketPanelController;
    private readonly SpellBookView spellBookView;
    private readonly BotController botController;

    internal CreatureSpellResolver(
        PlayerStatusView playerStatusView,
        SelectedCreatureView selectedCreatureView,
        CreatureSelectionManager creatureSelectionManager,
        CreaturePanel creaturePanel,
        IngredientDeck ingredientDeck,
        MarketView marketView,
        MarketPanelController marketPanelController,
        SpellBookView spellBookView,
        BotController botController
    )
    {
        this.playerStatusView = playerStatusView;
        this.selectedCreatureView = selectedCreatureView;
        this.creatureSelectionManager = creatureSelectionManager;
        this.creaturePanel = creaturePanel;
        this.ingredientDeck = ingredientDeck;
        this.marketView = marketView;
        this.marketPanelController = marketPanelController;
        this.spellBookView = spellBookView;
        this.botController = botController;
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

        int damage = SpellDamageCalculator.CalculateCreatureDamage(
            creature,
            spellInstance.definition,
            baseDamage
        );

        creature.TakeDamage(damage);

        ResolveOnAttackedAbility(creature, caster);

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

        bool ignoresShield = creature.definition.ability == CreatureAbility.IgnoreShield;

        if (!ignoresShield && caster.shield > 0)
        {
            int absorbed = Mathf.Min(caster.shield, remainingDamage);

            caster.RemoveShield(absorbed);

            remainingDamage -= absorbed;

            Debug.Log($"El Escudo Arcano absorbe {absorbed} de daño.");
        }
        else if (ignoresShield)
        {
            Debug.Log($"{creature.definition.creatureName} " + "ignora el Escudo Arcano.");
        }

        if (remainingDamage > 0)
        {
            caster.TakeDamage(remainingDamage);

            if (
                creature.definition.ability == CreatureAbility.Poison
                && caster.IsAlive
                && caster.statusEffects != null
            )
            {
                caster.statusEffects.poisoned = true;

                Debug.Log(
                    $"{creature.definition.creatureName} " + $"envenena a {caster.gameObject.name}."
                );
            }
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

        ResolveMudGremlinDeathEffect(definition, caster);

        if (definition.ability == CreatureAbility.SulfurDragonFireResistance && marketView != null)
        {
            marketView.BeginFreeReward(caster);

            if (marketPanelController != null && marketView.GetPlayer() == caster)
            {
                marketPanelController.OpenMarket();
            }
        }

        if (definition.ability == CreatureAbility.IgnoreShield)
        {
            if (spellBookView != null && spellBookView.IsPlayer(caster))
            {
                spellBookView.BeginMasteryReward(caster);
            }
            else if (botController != null && botController.IsControlledPlayer(caster))
            {
                botController.GrantSpectralLordMasteryReward();
            }
        }

        if (
            definition.ability == CreatureAbility.SwampWitchDiscardOnAttack
            && ingredientDeck != null
        )
        {
            ingredientDeck.DrawToPlayer(caster, 1);

            Debug.Log(
                $"{caster.gameObject.name} roba 1 ingrediente "
                    + $"por derrotar a {definition.creatureName}."
            );
        }

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

        int damage = SpellDamageCalculator.CalculateCreatureDamage(
            creature,
            spellInstance.definition,
            baseDamage
        );

        int healing = spellInstance.definition.GetSecondaryEffectValue(spellInstance.level);

        creature.TakeDamage(damage);

        ResolveOnAttackedAbility(creature, caster);

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

        int damage = SpellDamageCalculator.CalculateCreatureDamage(
            creature,
            spellInstance.definition,
            baseDamage
        );

        creature.TakeDamage(damage);

        ResolveOnAttackedAbility(creature, caster);

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

        if (creature.definition.ability == CreatureAbility.Flying)
        {
            Debug.Log(
                $"{creature.definition.creatureName} vuela " + "y es inmune a Raíces de Tierra."
            );

            return;
        }

        int baseDamage = spellInstance.definition.GetEffectValue(spellInstance.level);

        int damage =
            baseDamage > 0
                ? SpellDamageCalculator.CalculateCreatureDamage(
                    creature,
                    spellInstance.definition,
                    baseDamage
                )
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

        Debug.Log(
            $"EXPLOSIÓN ÁCIDA DEBUG → "
                + $"CreaturePanel: {creaturePanel.gameObject.name} | "
                + $"Criaturas encontradas: {creatures.Count}"
        );

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

            ResolveOnAttackedAbility(creature, caster);

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

    private void ResolveOnAttackedAbility(CreatureInstance creature, PlayerState caster)
    {
        if (creature == null || creature.definition == null || caster == null)
        {
            return;
        }

        if (creature.definition.ability != CreatureAbility.SwampWitchDiscardOnAttack)
        {
            return;
        }

        List<IngredientType> availableIngredients = new List<IngredientType>();

        foreach (IngredientType type in System.Enum.GetValues(typeof(IngredientType)))
        {
            if (caster.inventory.GetAmount(type) > 0)
            {
                availableIngredients.Add(type);
            }
        }

        if (availableIngredients.Count == 0)
        {
            Debug.Log(
                $"{caster.gameObject.name} no tiene ingredientes "
                    + "adicionales que descartar por atacar "
                    + $"{creature.definition.creatureName}."
            );

            return;
        }

        IngredientType discardedIngredient = availableIngredients[
            Random.Range(0, availableIngredients.Count)
        ];

        caster.DiscardIngredient(discardedIngredient);

        Debug.Log(
            $"{creature.definition.creatureName} obliga a "
                + $"{caster.gameObject.name} a descartar "
                + $"{discardedIngredient}."
        );
    }

    private void ResolveMudGremlinDeathEffect(CreatureDefinition definition, PlayerState killer)
    {
        if (
            definition == null
            || killer == null
            || definition.ability != CreatureAbility.MudGremlinDeathDiscard
        )
        {
            return;
        }

        List<IngredientType> availableIngredients = new List<IngredientType>();

        foreach (IngredientType ingredientType in System.Enum.GetValues(typeof(IngredientType)))
        {
            if (killer.inventory.GetAmount(ingredientType) > 0)
            {
                availableIngredients.Add(ingredientType);
            }
        }

        if (availableIngredients.Count == 0)
        {
            Debug.Log(
                $"{killer.gameObject.name} no tiene ingredientes "
                    + "que descartar por la muerte del Gremlin de Barro."
            );

            return;
        }

        IngredientType discardedIngredient = availableIngredients[
            Random.Range(0, availableIngredients.Count)
        ];

        killer.DiscardIngredient(discardedIngredient);

        Debug.Log(
            $"El Gremlin de Barro cae y "
                + $"{killer.gameObject.name} descarta "
                + $"{discardedIngredient}."
        );
    }
}
