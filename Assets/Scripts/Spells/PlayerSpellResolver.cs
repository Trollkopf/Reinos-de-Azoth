using UnityEngine;

/// <summary>
/// Resuelve curación, escudo y efectos contra jugadores.
/// </summary>
internal sealed class PlayerSpellResolver
{
    private readonly PlayerStatusView playerStatusView;
    private readonly PlayerTargetView botTargetView;

    internal PlayerSpellResolver(PlayerStatusView playerStatusView, PlayerTargetView botTargetView)
    {
        this.playerStatusView = playerStatusView;
        this.botTargetView = botTargetView;
    }

    internal void ResolveHeal(SpellInstance spellInstance, PlayerState caster)
    {
        int heal = spellInstance.definition.GetEffectValue(spellInstance.level);

        caster.Heal(heal);

        Debug.Log($"{spellInstance.definition.spellName} " + $"cura {heal} PV.");

        RefreshViews();
    }

    internal void ResolveShield(SpellInstance spellInstance, PlayerState caster)
    {
        int shieldAmount = spellInstance.definition.GetEffectValue(spellInstance.level);

        int previousShield = caster.shield;

        caster.AddShield(shieldAmount);

        int gainedShield = caster.shield - previousShield;

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
            targetPlayer.TakeDamage(damageToHealth);
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
            targetPlayer.TakeDamage(damageToHealth);
        }

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
}
