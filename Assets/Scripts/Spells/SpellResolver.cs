using System.Collections.Generic;
using UnityEngine;

public class SpellResolver : MonoBehaviour
{
    [SerializeField]
    private PlayerState player;

    [SerializeField]
    private PlayerStatusView playerStatusView;

    [SerializeField]
    private SelectedCreatureView selectedCreatureView;

    [SerializeField]
    private CreatureSelectionManager creatureSelectionManager;

    [SerializeField]
    private CreaturePanel creaturePanel;

    [SerializeField]
    private IngredientDeck ingredientDeck;

    [SerializeField]
    private IllusionChoicePanel illusionChoicePanel;

    [SerializeField]
    private PlayerTargetView botTargetView;

    public void Resolve(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        if (caster == null)
        {
            Debug.LogError("SpellResolver: caster es NULL.");
            return;
        }

        if (spellInstance == null)
        {
            Debug.LogError("SpellResolver: spellInstance es NULL");
            return;
        }

        SpellDefinition spell = spellInstance.definition;

        Debug.Log(
            $"SpellResolver ejecutando: {spell.spellName} | "
                + $"EffectType: {spell.effectType} | "
                + $"Nivel: {spellInstance.level} | "
                + $"Valor: {spell.GetEffectValue(spellInstance.level)}"
        );

        switch (spell.effectType)
        {
            case SpellEffectType.Damage:
                Debug.Log("Resolviendo DAMAGE");

                ResolveDamage(spellInstance, targetCreature, caster);
                break;

            case SpellEffectType.Heal:
                Debug.Log("Resolviendo HEAL");

                ResolveHeal(spellInstance, caster);
                break;

            case SpellEffectType.Shield:
                ResolveShield(spellInstance, caster);
                break;

            case SpellEffectType.Drain:
                ResolveDrain(spellInstance, targetCreature, caster);
                break;

            case SpellEffectType.WindWhip:
                ResolveWindWhip(spellInstance, targetCreature, caster);
                break;

            case SpellEffectType.Roots:
                ResolveRoots(spellInstance, targetCreature, caster);
                break;

            case SpellEffectType.AcidExplosion:
                ResolveAcidExplosion(spellInstance, targetCreature, caster);
                break;

            case SpellEffectType.Illusion:
                ResolveIllusion(spellInstance);
                break;

            default:
                Debug.LogWarning($"EffectType todavía no implementado: {spell.effectType}");
                break;
        }
    }

    public void ResolveAgainstPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        if (
            spellInstance == null
            || spellInstance.definition == null
            || targetPlayer == null
            || caster == null
        )
        {
            Debug.LogError("SpellResolver: datos inválidos al resolver hechizo contra jugador.");

            return;
        }

        SpellDefinition spell = spellInstance.definition;

        switch (spell.effectType)
        {
            case SpellEffectType.Damage:
                ResolveDamageToPlayer(spellInstance, targetPlayer, caster);
                break;
            case SpellEffectType.Drain:
                ResolveDrainAgainstPlayer(spellInstance, targetPlayer, caster);
                break;

            default:
                Debug.LogWarning($"{spell.spellName} todavía no está preparado para PvP.");
                break;
        }
    }

    private void ResolveDamage(
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

        int damage = CalculateCreatureDamage(creature, baseDamage);

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

    private void ResolveHeal(SpellInstance spellInstance, PlayerState caster)
    {
        int heal = spellInstance.definition.GetEffectValue(spellInstance.level);

        caster.currentHP += heal;

        if (caster.currentHP > caster.maxHP)
        {
            caster.currentHP = caster.maxHP;
        }

        playerStatusView.Refresh();

        Debug.Log($"{spellInstance.definition.spellName} " + $"cura {heal} PV.");
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

    private void ResolveShield(SpellInstance spellInstance, PlayerState caster)
    {
        int shieldAmount = spellInstance.definition.GetEffectValue(spellInstance.level);

        int previousShield = caster.shield;

        caster.shield = Mathf.Min(caster.shield + shieldAmount, PlayerState.MaxShield);

        int gainedShield = caster.shield - previousShield;

        Debug.Log(
            $"{spellInstance.definition.spellName} "
                + $"otorga {gainedShield} puntos de escudo. "
                + $"Escudo actual: {caster.shield}/{PlayerState.MaxShield}."
        );

        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }
    }

    private void ResolveDrain(
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

        int damage = CalculateCreatureDamage(creature, baseDamage);

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

    private void ResolveWindWhip(
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

        int damage = CalculateCreatureDamage(creature, baseDamage);

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

    private void ResolveRoots(
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

        int damage = baseDamage > 0 ? CalculateCreatureDamage(creature, baseDamage) : 0;

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

    private void ResolveAcidExplosion(
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

    private int CalculateCreatureDamage(CreatureInstance creature, int baseDamage)
    {
        int finalDamage = baseDamage;

        if (creature != null && creature.isCorroded)
        {
            finalDamage += 1;

            Debug.Log($"{creature.definition.creatureName} está corroído: +1 daño.");
        }

        return finalDamage;
    }

    private void ResolveIllusion(SpellInstance spellInstance)
    {
        if (ingredientDeck == null)
        {
            Debug.LogError("SpellResolver: IngredientDeck no está asignado.");

            return;
        }

        if (illusionChoicePanel == null)
        {
            Debug.LogError("SpellResolver: IllusionChoicePanel no está asignado.");

            return;
        }

        int drawAmount = spellInstance.definition.GetEffectValue(spellInstance.level);

        int keepAmount = spellInstance.definition.GetSecondaryEffectValue(spellInstance.level);

        List<IngredientType> revealedIngredients = new List<IngredientType>();

        for (int i = 0; i < drawAmount; i++)
        {
            IngredientType ingredient = ingredientDeck.Draw();

            revealedIngredients.Add(ingredient);

            Debug.Log($"Ilusión ha revelado: {ingredient}");
        }

        illusionChoicePanel.ShowChoices(revealedIngredients, keepAmount);
    }

    private void ResolveDamageToPlayer(
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

                targetPlayer.shield -= absorbed;
                damageToShield = absorbed;

                damageToHealth = damage - absorbed;

                break;
            }

            case ShieldPiercingType.IgnoreOne:
            {
                // 1 punto atraviesa directamente el escudo.
                int piercingDamage = Mathf.Min(1, damage);

                damageToHealth += piercingDamage;

                int remainingDamage = damage - piercingDamage;

                int absorbed = Mathf.Min(targetPlayer.shield, remainingDamage);

                targetPlayer.shield -= absorbed;
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

        targetPlayer.currentHP -= damageToHealth;

        targetPlayer.currentHP = Mathf.Max(0, targetPlayer.currentHP);

        Debug.Log(
            $"{caster.gameObject.name} lanza {spell.spellName} "
                + $"contra {targetPlayer.gameObject.name}. "
                + $"Escudo perdido: {damageToShield}. "
                + $"Vida perdida: {damageToHealth}. "
                + $"Vida restante: {targetPlayer.currentHP}/{targetPlayer.maxHP}. "
                + $"Escudo restante: {targetPlayer.shield}."
        );

        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }

        if (botTargetView != null)
        {
            botTargetView.RefreshView();
        }
    }

    private void ResolveDrainAgainstPlayer(
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

                targetPlayer.shield -= absorbed;
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

                targetPlayer.shield -= absorbed;
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

        targetPlayer.currentHP -= damageToHealth;

        targetPlayer.currentHP = Mathf.Max(0, targetPlayer.currentHP);

        caster.currentHP = Mathf.Min(caster.currentHP + healing, caster.maxHP);

        Debug.Log(
            $"{caster.gameObject.name} lanza {spell.spellName} "
                + $"contra {targetPlayer.gameObject.name}. "
                + $"Daño a vida: {damageToHealth}. "
                + $"Curación: {healing}."
        );

        if (botTargetView != null)
        {
            botTargetView.RefreshView();
        }

        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }
    }
}
