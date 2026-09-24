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

    List<CreatureView> defeatedCreatures = new List<CreatureView>();

    public void Resolve(SpellInstance spellInstance, CreatureView targetCreature)
    {
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

                ResolveDamage(spellInstance, targetCreature);
                break;

            case SpellEffectType.Heal:
                Debug.Log("Resolviendo HEAL");

                ResolveHeal(spellInstance);
                break;

            case SpellEffectType.Shield:
                ResolveShield(spellInstance);
                break;

            case SpellEffectType.Drain:
                ResolveDrain(spellInstance, targetCreature);
                break;

            case SpellEffectType.WindWhip:
                ResolveWindWhip(spellInstance, targetCreature);
                break;

            case SpellEffectType.Roots:
                ResolveRoots(spellInstance, targetCreature);
                break;

            case SpellEffectType.AcidExplosion:
                ResolveAcidExplosion(spellInstance, targetCreature);
                break;

            case SpellEffectType.Illusion:
                ResolveIllusion(spellInstance);
                break;

            default:
                Debug.LogWarning($"EffectType todavía no implementado: {spell.effectType}");
                break;
        }
    }

    private void ResolveDamage(SpellInstance spellInstance, CreatureView targetCreature)
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
            ResolveCreatureDeath(targetCreature, creature);

            return;
        }

        // Si sobrevive, contraataca
        ResolveCounterAttack(creature);
    }

    private void ResolveHeal(SpellInstance spellInstance)
    {
        int heal = spellInstance.definition.GetEffectValue(spellInstance.level);

        player.currentHP += heal;

        if (player.currentHP > player.maxHP)
        {
            player.currentHP = player.maxHP;
        }

        playerStatusView.Refresh();

        Debug.Log($"{spellInstance.definition.spellName} " + $"cura {heal} PV.");
    }

    private void ResolveCounterAttack(CreatureInstance creature)
    {
        int damage = creature.definition.attack;

        int remainingDamage = damage;

        if (player.shield > 0)
        {
            int absorbed = Mathf.Min(player.shield, remainingDamage);

            player.shield -= absorbed;
            remainingDamage -= absorbed;

            Debug.Log($"El Escudo Arcano absorbe {absorbed} de daño.");
        }

        if (remainingDamage > 0)
        {
            player.currentHP -= remainingDamage;

            if (player.currentHP < 0)
            {
                player.currentHP = 0;
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

    private void ResolveCreatureDeath(CreatureView creatureView, CreatureInstance creature)
    {
        CreatureDefinition definition = creature.definition;

        Debug.Log($"{definition.creatureName} ha sido derrotado.");

        player.coins += definition.coinReward;

        player.arcanePower += definition.arcanePowerReward;

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

    private void ResolveShield(SpellInstance spellInstance)
    {
        int shieldAmount = spellInstance.definition.GetEffectValue(spellInstance.level);

        int previousShield = player.shield;

        player.shield = Mathf.Min(player.shield + shieldAmount, PlayerState.MaxShield);

        int gainedShield = player.shield - previousShield;

        Debug.Log(
            $"{spellInstance.definition.spellName} "
                + $"otorga {gainedShield} puntos de escudo. "
                + $"Escudo actual: {player.shield}/{PlayerState.MaxShield}."
        );

        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }
    }

    private void ResolveDrain(SpellInstance spellInstance, CreatureView targetCreature)
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

        player.currentHP = Mathf.Min(player.currentHP + healing, player.maxHP);

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
            ResolveCreatureDeath(targetCreature, creature);

            return;
        }

        ResolveCounterAttack(creature);
    }

    private void ResolveWindWhip(SpellInstance spellInstance, CreatureView targetCreature)
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
            ingredientDeck.DrawToPlayer(player, 1);

            Debug.Log("Látigo de Viento Nv.3: robas 1 ingrediente.");
        }

        if (creature.IsDead)
        {
            ResolveCreatureDeath(targetCreature, creature);

            return;
        }

        ResolveCounterAttack(creature);
    }

    private void ResolveRoots(SpellInstance spellInstance, CreatureView targetCreature)
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
            ResolveCreatureDeath(targetCreature, creature);

            return;
        }

        // Raíces impide el contraataque
        Debug.Log(
            $"{creature.definition.creatureName} "
                + $"no puede contraatacar por efecto de Raíces de Tierra."
        );
    }

    private void ResolveAcidExplosion(SpellInstance spellInstance, CreatureView targetCreature)
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

            ResolveCreatureDeath(defeatedCreature, creature);
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
}
