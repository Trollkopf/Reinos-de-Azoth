using System.Collections.Generic;
using UnityEngine;

internal sealed class PlayerSpellResolver
{
    private readonly PlayerStatusView playerStatusView;
    private readonly PlayerTargetView botTargetView;
    private readonly IngredientDeck ingredientDeck;

    internal PlayerSpellResolver(
        PlayerStatusView playerStatusView,
        PlayerTargetView botTargetView,
        IngredientDeck ingredientDeck
    )
    {
        this.playerStatusView = playerStatusView;
        this.botTargetView = botTargetView;
        this.ingredientDeck = ingredientDeck;
    }

    internal void ResolveHeal(SpellInstance spellInstance, PlayerState caster)
    {
        int heal = spellInstance.definition.GetEffectValue(spellInstance.level);

        caster.Heal(heal);

        Debug.Log($"{spellInstance.definition.spellName} cura {heal} PV.");

        if (spellInstance.level >= 3 && caster.statusEffects != null)
        {
            int burnCount = caster.statusEffects.GetBurnCount();

            bool hadSpellLimit =
                caster.statusEffects.spellLimitThisTurn > 0
                || caster.statusEffects.spellLimitNextTurn > 0;

            caster.statusEffects.ClearNegativeEffects();

            if (burnCount > 0 || hadSpellLimit)
            {
                Debug.Log(
                    $"{caster.gameObject.name} elimina sus estados negativos "
                        + "gracias a Curación Nv.3."
                );
            }
        }

        RefreshViews();
    }

    internal void ResolveShield(SpellInstance spellInstance, PlayerState caster)
    {
        int shieldAmount = spellInstance.definition.GetEffectValue(spellInstance.level);

        int previousShield = caster.shield;

        caster.AddShield(shieldAmount);

        int gainedShield = caster.shield - previousShield;

        if (spellInstance.level >= 3 && caster.statusEffects != null)
        {
            caster.statusEffects.reflectNextDamage = true;

            Debug.Log(
                $"{caster.gameObject.name} activa Reflejo. "
                    + "El próximo hechizo ofensivo directo devolverá 1 de daño."
            );
        }

        Debug.Log(
            $"{spellInstance.definition.spellName} "
                + $"otorga {gainedShield} puntos de escudo. "
                + $"Escudo actual: {caster.shield}/{PlayerState.MaxShield}."
        );

        RefreshViews();
    }

    internal void ResolveDamageToPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        SpellDefinition spell = spellInstance.definition;

        int damage = spell.GetEffectValue(spellInstance.level);

        damage = ApplyCorrosionBonus(targetPlayer, damage);

        int damageToHealth = 0;
        int damageToShield = 0;

        ShieldPiercingType piercing = spell.GetShieldPiercing(spellInstance.level);

        switch (piercing)
        {
            case ShieldPiercingType.None:
            {
                int absorbed = Mathf.Min(targetPlayer.shield, damage);

                if (absorbed > 0)
                {
                    targetPlayer.RemoveShield(absorbed);
                }

                damageToShield = absorbed;

                damageToHealth = damage - absorbed;

                break;
            }

            case ShieldPiercingType.IgnoreOne:
            {
                int piercingDamage = Mathf.Min(1, damage);

                damageToHealth += piercingDamage;

                int remainingDamage = damage - piercingDamage;

                int absorbed = Mathf.Min(targetPlayer.shield, remainingDamage);

                if (absorbed > 0)
                {
                    targetPlayer.RemoveShield(absorbed);
                }

                damageToShield = absorbed;

                damageToHealth += remainingDamage - absorbed;

                break;
            }

            case ShieldPiercingType.IgnoreAll:
            {
                damageToHealth = damage;

                break;
            }
        }

        if (damageToHealth > 0)
        {
            targetPlayer.TakeDamage(damageToHealth, caster);
        }

        TryReflectDamage(targetPlayer, caster);

        // Bola de Fuego Nv.3:
        // añade una nueva acumulación independiente
        // de Quemadura durante 2 turnos.
        if (spellInstance.level >= 3 && spell.appliesBurnAtLevel3 && targetPlayer.currentHP > 0)
        {
            targetPlayer.statusEffects.AddBurn(caster);

            Debug.Log(
                $"{targetPlayer.gameObject.name} recibe "
                    + $"1 acumulación de Quemadura. "
                    + $"Total: {targetPlayer.statusEffects.GetBurnCount()}."
            );
        }

        Debug.Log(
            $"{caster.gameObject.name} lanza {spell.spellName} "
                + $"contra {targetPlayer.gameObject.name}. "
                + $"Escudo perdido: {damageToShield}. "
                + $"Vida perdida: {damageToHealth}. "
                + $"Vida restante: {targetPlayer.currentHP}/{targetPlayer.maxHP}. "
                + $"Escudo restante: {targetPlayer.shield}."
        );

        RefreshViews();
    }

    internal void ResolveDrainAgainstPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        SpellDefinition spell = spellInstance.definition;

        int damage = spell.GetEffectValue(spellInstance.level);

        damage = ApplyCorrosionBonus(targetPlayer, damage);

        int healing = spell.GetSecondaryEffectValue(spellInstance.level);

        int damageToHealth = 0;
        int damageToShield = 0;

        ShieldPiercingType piercing = spell.GetShieldPiercing(spellInstance.level);

        switch (piercing)
        {
            case ShieldPiercingType.None:
            {
                int absorbed = Mathf.Min(targetPlayer.shield, damage);

                if (absorbed > 0)
                {
                    targetPlayer.RemoveShield(absorbed);
                }

                damageToShield = absorbed;

                damageToHealth = damage - absorbed;

                break;
            }

            case ShieldPiercingType.IgnoreOne:
            {
                int piercingDamage = Mathf.Min(1, damage);

                damageToHealth += piercingDamage;

                int remainingDamage = damage - piercingDamage;

                int absorbed = Mathf.Min(targetPlayer.shield, remainingDamage);

                if (absorbed > 0)
                {
                    targetPlayer.RemoveShield(absorbed);
                }

                damageToShield = absorbed;

                damageToHealth += remainingDamage - absorbed;

                break;
            }

            case ShieldPiercingType.IgnoreAll:
            {
                damageToHealth = damage;

                break;
            }
        }

        if (damageToHealth > 0)
        {
            targetPlayer.TakeDamage(damageToHealth, caster);
        }

        TryReflectDamage(targetPlayer, caster);

        caster.Heal(healing);

        Debug.Log(
            $"{caster.gameObject.name} lanza {spell.spellName} "
                + $"contra {targetPlayer.gameObject.name}. "
                + $"Escudo perdido: {damageToShield}. "
                + $"Daño a vida: {damageToHealth}. "
                + $"Curación: {healing}."
        );

        RefreshViews();
    }

    internal void ResolveWindWhipAgainstPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        int damage = spellInstance.definition.GetSecondaryEffectValue(spellInstance.level);

        damage = ApplyCorrosionBonus(targetPlayer, damage);

        int absorbed = Mathf.Min(targetPlayer.shield, damage);

        if (absorbed > 0)
        {
            targetPlayer.RemoveShield(absorbed);
        }

        int damageToHealth = damage - absorbed;

        if (damageToHealth > 0)
        {
            targetPlayer.TakeDamage(damageToHealth, caster);
        }

        TryReflectDamage(targetPlayer, caster);

        DiscardRandomIngredient(targetPlayer);

        if (spellInstance.level >= 3 && ingredientDeck != null)
        {
            ingredientDeck.DrawToPlayer(caster, 1);

            Debug.Log(
                $"{caster.gameObject.name} roba 1 ingrediente " + "por Látigo de Viento Nv.3."
            );
        }

        Debug.Log(
            $"{caster.gameObject.name} lanza "
                + $"{spellInstance.definition.spellName} contra "
                + $"{targetPlayer.gameObject.name}. "
                + $"Daño a escudo: {absorbed}. "
                + $"Daño a vida: {damageToHealth}."
        );

        RefreshViews();
    }

    internal void ResolveRootsAgainstPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        SpellDefinition spell = spellInstance.definition;

        int damage = spell.GetEffectValue(spellInstance.level);

        damage = ApplyCorrosionBonus(targetPlayer, damage);

        int absorbed = Mathf.Min(targetPlayer.shield, damage);

        if (absorbed > 0)
        {
            targetPlayer.RemoveShield(absorbed);
        }

        int damageToHealth = damage - absorbed;

        if (damageToHealth > 0)
        {
            targetPlayer.TakeDamage(damageToHealth, caster);
        }

        TryReflectDamage(targetPlayer, caster);

        // Raíces limita al rival a un solo
        // hechizo durante su siguiente turno.
        targetPlayer.statusEffects.spellLimitNextTurn = 1;

        Debug.Log(
            $"{caster.gameObject.name} lanza "
                + $"{spell.spellName} contra "
                + $"{targetPlayer.gameObject.name}. "
                + $"Daño a escudo: {absorbed}. "
                + $"Daño a vida: {damageToHealth}. "
                + $"{targetPlayer.gameObject.name} solo podrá lanzar "
                + "1 hechizo en su siguiente turno."
        );

        RefreshViews();
    }

    private void DiscardRandomIngredient(PlayerState targetPlayer)
    {
        List<IngredientType> availableIngredients = new List<IngredientType>();

        foreach (IngredientType type in System.Enum.GetValues(typeof(IngredientType)))
        {
            if (targetPlayer.inventory.GetAmount(type) > 0)
            {
                availableIngredients.Add(type);
            }
        }

        if (availableIngredients.Count == 0)
        {
            Debug.Log($"{targetPlayer.gameObject.name} " + "no tiene ingredientes que descartar.");

            return;
        }

        IngredientType discardedIngredient = availableIngredients[
            Random.Range(0, availableIngredients.Count)
        ];

        targetPlayer.DiscardIngredient(discardedIngredient);

        Debug.Log(
            $"{targetPlayer.gameObject.name} descarta "
                + $"{discardedIngredient} por Látigo de Viento."
        );
    }

    private void RefreshViews()
    {
        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }

        if (botTargetView != null)
        {
            botTargetView.RefreshView();
        }
    }

    private void TryReflectDamage(PlayerState targetPlayer, PlayerState caster)
    {
        if (targetPlayer.statusEffects == null || !targetPlayer.statusEffects.ConsumeReflection())
        {
            return;
        }

        caster.TakeDamage(1, targetPlayer);

        Debug.Log(
            $"{targetPlayer.gameObject.name} refleja 1 de daño " + $"a {caster.gameObject.name}."
        );
    }

    private int ApplyCorrosionBonus(PlayerState targetPlayer, int damage)
    {
        if (damage > 0 && targetPlayer.statusEffects != null && targetPlayer.statusEffects.corroded)
        {
            Debug.Log(
                $"{targetPlayer.gameObject.name} está corroído. " + "El hechizo recibe +1 de daño."
            );

            return damage + 1;
        }

        return damage;
    }

    internal void ResolveAcidExplosionAgainstPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        SpellDefinition spell = spellInstance.definition;

        int damage = spell.GetEffectValue(spellInstance.level);

        // Si ya estaba corroído de antes,
        // esta explosión también se beneficia.
        damage = ApplyCorrosionBonus(targetPlayer, damage);

        int absorbed = Mathf.Min(targetPlayer.shield, damage);

        if (absorbed > 0)
        {
            targetPlayer.RemoveShield(absorbed);
        }

        int damageToHealth = damage - absorbed;

        if (damageToHealth > 0)
        {
            targetPlayer.TakeDamage(damageToHealth, caster);
        }

        // Nv.3 aplica corrosión DESPUÉS
        // de resolver su propio daño.
        if (
            spellInstance.level >= 3
            && targetPlayer.currentHP > 0
            && targetPlayer.statusEffects != null
        )
        {
            targetPlayer.statusEffects.corroded = true;

            Debug.Log($"{targetPlayer.gameObject.name} queda corroído.");
        }

        Debug.Log(
            $"{caster.gameObject.name} alcanza a "
                + $"{targetPlayer.gameObject.name} con "
                + $"{spell.spellName}. "
                + $"Daño a escudo: {absorbed}. "
                + $"Daño a vida: {damageToHealth}."
        );

        RefreshViews();
    }
}
