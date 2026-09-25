using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerState player;

    [SerializeField]
    private PlayerSpellBook spellBook;

    [SerializeField]
    private SpellResolver spellResolver;

    [SerializeField]
    private CreaturePanel creaturePanel;

    [SerializeField]
    private MarketView marketView;

    [SerializeField]
    private PlayerManager playerManager;

    [SerializeField]
    private IngredientDeck ingredientDeck;

    private class OffensiveDecision
    {
        public SpellInstance spell;
        public CreatureView creatureTarget;
        public PlayerState playerTarget;
        public int score;
    }

    public void StartBotTurn()
    {
        Debug.Log(
            $"Empieza turno del bot. "
                + $"Vida: {player.currentHP}, "
                + $"Escudo: {player.shield}, "
                + $"Monedas: {player.coins}, "
                + $"Poder Arcano: {player.arcanePower}, "
                + $"Ingredientes: {player.inventory.GetTotalCount()}"
        );

        EvaluateOpponents();

        EvaluateAvailableSpells();

        TryMultipleMarketPurchases();

        EvaluateAvailableSpells();

        TryIllusion();

        TryHeal();

        TryShield();

        TryMultipleOffensiveSpells();
    }

    private void EvaluateAvailableSpells()
    {
        foreach (SpellInstance spell in spellBook.Spells)
        {
            if (
                spell != null
                && spell.definition != null
                && player.inventory.CanAfford(spell.definition)
            )
            {
                Debug.Log($"Bot puede lanzar: {spell.definition.spellName}");
            }
        }
    }

    private bool TryCastOffensiveSpell()
    {
        if (!player.CanCastSpell())
        {
            Debug.Log("Bot no puede lanzar más hechizos este turno.");

            return false;
        }

        if (spellResolver == null || creaturePanel == null)
            return false;
        if (spellResolver == null || creaturePanel == null)
            return false;

        List<CreatureView> creatures = creaturePanel.GetActiveCreatureViews();

        if (creatures == null || creatures.Count == 0)
            return false;

        OffensiveDecision bestDecision = new OffensiveDecision { score = int.MinValue };

        foreach (SpellInstance spell in spellBook.Spells)
        {
            if (spell == null || spell.definition == null)
                continue;

            if (!player.inventory.CanAfford(spell.definition))
                continue;

            bool isOffensive =
                spell.definition.effectType == SpellEffectType.Damage
                || spell.definition.effectType == SpellEffectType.Drain
                || spell.definition.effectType == SpellEffectType.WindWhip
                || spell.definition.effectType == SpellEffectType.Roots
                || spell.definition.effectType == SpellEffectType.AcidExplosion;

            if (!isOffensive)
                continue;

            // Explosión Ácida se evalúa una sola vez,
            // porque afecta a todas las criaturas.
            if (spell.definition.effectType == SpellEffectType.AcidExplosion)
            {
                int score = EvaluateAcidExplosion(spell);

                Debug.Log(
                    $"Bot evalúa {spell.definition.spellName} "
                        + $"contra todas las criaturas → puntuación {score}"
                );

                if (score > bestDecision.score)
                {
                    bestDecision.spell = spell;
                    bestDecision.creatureTarget = null;
                    bestDecision.playerTarget = null;
                    bestDecision.score = score;
                }
                continue;
            }

            // El resto se evalúa criatura por criatura.
            foreach (CreatureView creatureView in creatures)
            {
                if (creatureView == null)
                    continue;

                CreatureInstance creature = creatureView.GetCreatureInstance();

                if (creature == null || creature.IsDead)
                {
                    continue;
                }

                int score = EvaluateOffensiveMove(spell, creature);

                Debug.Log(
                    $"Bot evalúa {spell.definition.spellName} "
                        + $"contra {creature.definition.creatureName} "
                        + $"→ puntuación {score}"
                );

                if (score > bestDecision.score)
                {
                    bestDecision.spell = spell;
                    bestDecision.creatureTarget = creatureView;
                    bestDecision.playerTarget = null;
                    bestDecision.score = score;
                }
            }
        }

        List<PlayerState> opponents = playerManager.GetOpponents(player);

        foreach (SpellInstance spell in spellBook.Spells)
        {
            if (spell == null || spell.definition == null)
                continue;

            if (!player.inventory.CanAfford(spell.definition))
                continue;

            if (spell.definition.effectType != SpellEffectType.Damage)
            {
                continue;
            }

            foreach (PlayerState opponent in opponents)
            {
                int score = EvaluatePlayerAttack(spell, opponent);

                Debug.Log(
                    $"Bot evalúa {spell.definition.spellName} "
                        + $"contra {opponent.gameObject.name} "
                        + $"→ puntuación PvP {score}"
                );

                if (score > bestDecision.score)
                {
                    bestDecision.spell = spell;
                    bestDecision.creatureTarget = null;
                    bestDecision.playerTarget = opponent;
                    bestDecision.score = score;
                }
            }
        }

        if (bestDecision.spell == null)
        {
            Debug.Log("Bot no encuentra ningún ataque posible.");

            return false;
        }

        bool spent = player.SpendIngredientsForSpell(bestDecision.spell.definition);

        if (!spent)
            return false;

        if (!spent)
            return false;

        if (bestDecision.playerTarget != null)
        {
            Debug.Log(
                $"Bot decide lanzar "
                    + $"{bestDecision.spell.definition.spellName} "
                    + $"contra {bestDecision.playerTarget.gameObject.name}."
            );

            spellResolver.ResolveAgainstPlayer(
                bestDecision.spell,
                bestDecision.playerTarget,
                player
            );
        }
        else if (bestDecision.spell.definition.effectType == SpellEffectType.AcidExplosion)
        {
            Debug.Log(
                $"Bot decide lanzar "
                    + $"{bestDecision.spell.definition.spellName} "
                    + $"contra todas las criaturas."
            );

            spellResolver.Resolve(bestDecision.spell, null, player);
        }
        else
        {
            if (bestDecision.creatureTarget == null)
                return false;

            Debug.Log(
                $"Bot decide lanzar "
                    + $"{bestDecision.spell.definition.spellName} "
                    + $"contra "
                    + $"{bestDecision.creatureTarget.GetCreatureName()}."
            );

            spellResolver.Resolve(bestDecision.spell, bestDecision.creatureTarget, player);
        }

        player.RegisterSpellCast();

        bestDecision.spell.AddMastery();

        Debug.Log($"Bot ha lanzado {player.SpellsCastThisTurn} hechizo(s) este turno.");

        return true;
    }

    private int EvaluateAcidExplosion(SpellInstance spell)
    {
        int damage = spell.definition.GetEffectValue(spell.level);

        int score = 0;

        List<CreatureView> creatures = creaturePanel.GetActiveCreatureViews();

        foreach (CreatureView creatureView in creatures)
        {
            if (creatureView == null)
                continue;

            CreatureInstance creature = creatureView.GetCreatureInstance();

            if (creature == null || creature.IsDead)
            {
                continue;
            }

            // Valoramos todo el daño que hará.
            score += damage;

            // Si mata, muchísimo más interesante.
            if (damage >= creature.currentHP)
            {
                score += 100;

                score += creature.definition.coinReward * 5;

                score += creature.definition.arcanePowerReward * 10;
            }

            // Nivel 3 además deja corrosión.
            if (spell.level >= 3 && damage < creature.currentHP)
            {
                score += 4;
            }
        }

        return score;
    }

    private int EvaluateOffensiveMove(SpellInstance spell, CreatureInstance creature)
    {
        int damage = spell.definition.GetEffectValue(spell.level);

        SpellEffectType effectType = spell.definition.effectType;

        // La corrosión añade +1 de daño real.
        if (creature.isCorroded && damage > 0)
        {
            damage += 1;
        }

        int score = damage;

        // Si puede matar a la criatura,
        // prioridad alta.
        if (damage >= creature.currentHP)
        {
            score += 100;

            score += creature.definition.coinReward * 5;

            score += creature.definition.arcanePowerReward * 10;
        }

        // Drenaje Vital:
        // cuanto más herido esté el bot,
        // más interesante resulta.
        if (effectType == SpellEffectType.Drain)
        {
            int missingHP = player.maxHP - player.currentHP;

            int healing = spell.definition.GetSecondaryEffectValue(spell.level);

            int usefulHealing = Mathf.Min(missingHP, healing);

            score += usefulHealing * 4;
        }

        // Raíces:
        // evita contraataque,
        // así que vale más contra
        // criaturas que pegan fuerte.
        if (effectType == SpellEffectType.Roots)
        {
            score += creature.definition.attack * 4;
        }

        // Látigo de Viento Nv.3:
        // roba un ingrediente.
        if (effectType == SpellEffectType.WindWhip && spell.level >= 3)
        {
            score += 5;
        }

        return score;
    }

    private void TryMultipleOffensiveSpells()
    {
        const int maxCasts = 10;

        for (int i = 0; i < maxCasts; i++)
        {
            bool castSomething = TryCastOffensiveSpell();

            if (!castSomething)
                break;
        }
    }

    public void DiscardDownToHandLimit()
    {
        while (player.inventory.GetTotalCount() > PlayerState.MaxHandSize)
        {
            IngredientType? ingredientToDiscard = FindLeastUsefulIngredient();

            if (ingredientToDiscard == null)
                break;

            player.inventory.Remove(ingredientToDiscard.Value, 1);

            Debug.Log(
                $"Bot descarta {ingredientToDiscard.Value}. "
                    + $"Ingredientes restantes: "
                    + $"{player.inventory.GetTotalCount()}"
            );
        }
    }

    private IngredientType? FindLeastUsefulIngredient()
    {
        IngredientType? worstIngredient = null;

        int worstScore = int.MaxValue;

        foreach (IngredientType type in System.Enum.GetValues(typeof(IngredientType)))
        {
            if (player.inventory.GetAmount(type) <= 0)
            {
                continue;
            }

            int score = GetIngredientUsefulness(type);

            Debug.Log($"Bot evalúa descarte: {type} " + $"→ utilidad {score}");

            if (score < worstScore)
            {
                worstScore = score;
                worstIngredient = type;
            }
        }

        return worstIngredient;
    }

    private int GetIngredientUsefulness(IngredientType ingredient)
    {
        int score = 0;

        foreach (SpellInstance spell in spellBook.Spells)
        {
            if (spell == null || spell.definition == null)
            {
                continue;
            }

            foreach (IngredientCost cost in spell.definition.cost)
            {
                if (cost.type == ingredient)
                {
                    score++;
                }
            }
        }

        return score;
    }

    private void TryHeal()
    {
        if (!player.CanCastSpell())
        {
            return;
        }

        // Si solo le falta 0 o 1 PV,
        // no gastamos recursos.
        if (player.currentHP > player.maxHP - 2)
        {
            return;
        }

        foreach (SpellInstance spell in spellBook.Spells)
        {
            if (spell == null || spell.definition == null)
            {
                continue;
            }

            if (spell.definition.effectType != SpellEffectType.Heal)
            {
                continue;
            }

            if (!player.inventory.CanAfford(spell.definition))
            {
                continue;
            }

            bool spent = player.inventory.Spend(spell.definition);

            if (!spent)
                return;

            Debug.Log($"Bot decide curarse con " + $"{spell.definition.spellName}.");

            spellResolver.Resolve(spell, null, player);

            player.RegisterSpellCast();

            spell.AddMastery();

            return;
        }
    }

    private void TryShield()
    {
        if (!player.CanCastSpell())
        {
            return;
        }

        // Si ya tiene bastante escudo,
        // no gastamos recursos.
        if (player.shield >= 3)
        {
            return;
        }

        foreach (SpellInstance spell in spellBook.Spells)
        {
            if (spell == null || spell.definition == null)
            {
                continue;
            }

            if (spell.definition.effectType != SpellEffectType.Shield)
            {
                continue;
            }

            if (!player.inventory.CanAfford(spell.definition))
            {
                continue;
            }

            bool spent = player.inventory.Spend(spell.definition);

            if (!spent)
                return;

            Debug.Log($"Bot decide protegerse con " + $"{spell.definition.spellName}.");

            spellResolver.Resolve(spell, null, player);

            player.RegisterSpellCast();

            spell.AddMastery();

            return;
        }
    }

    private bool TryBuyFromMarket()
    {
        if (marketView == null)
            return false;

        List<int> affordableSlots = new List<int>();

        int bestSlot = -1;
        int bestScore = 0;

        int cardCount = marketView.GetVisibleCardCount();

        for (int i = 0; i < cardCount; i++)
        {
            MarketCardDefinition card = marketView.GetCardAt(i);

            if (card == null)
                continue;

            if (player.coins < card.price)
                continue;

            affordableSlots.Add(i);

            int score = 0;

            foreach (SpellInstance spell in spellBook.Spells)
            {
                if (spell == null || spell.definition == null)
                {
                    continue;
                }

                // Si ya podemos lanzar el hechizo,
                // comprar para él no nos aporta
                // nada inmediato.
                if (player.inventory.CanAfford(spell.definition))
                {
                    continue;
                }

                if (WouldEnableSpell(spell.definition, card))
                {
                    score++;
                }
            }

            Debug.Log(
                $"Bot evalúa mercado: " + $"{card.ingredientType} " + $"→ puntuación {score}"
            );

            if (score > bestScore)
            {
                bestScore = score;
                bestSlot = i;
            }
        }

        if (affordableSlots.Count == 0)
        {
            Debug.Log("Bot no puede permitirse ninguna carta del mercado.");

            return false;
        }

        // Si ninguna carta completa hechizos,
        // compramos una asequible al azar.
        if (bestSlot == -1)
        {
            bestSlot = affordableSlots[Random.Range(0, affordableSlots.Count)];

            Debug.Log(
                "Bot no encuentra una compra estratégica. " + "Compra una carta asequible al azar."
            );
        }

        MarketCardDefinition chosenCard = marketView.GetCardAt(bestSlot);

        Debug.Log(
            $"Bot compra {chosenCard.amount}x "
                + $"{chosenCard.ingredientType} "
                + $"por {chosenCard.price} monedas. "
                + $"Puntuación: {bestScore}"
        );

        marketView.TryBuy(bestSlot, player);

        return true;
    }

    private void TryMultipleMarketPurchases()
    {
        const int maxPurchases = 5;

        for (int i = 0; i < maxPurchases; i++)
        {
            bool boughtSomething = TryBuyFromMarket();

            if (!boughtSomething)
                break;
        }
    }

    private bool WouldEnableSpell(SpellDefinition spell, MarketCardDefinition card)
    {
        Dictionary<IngredientType, int> required = new Dictionary<IngredientType, int>();

        foreach (IngredientCost cost in spell.cost)
        {
            if (!required.ContainsKey(cost.type))
            {
                required[cost.type] = 0;
            }

            required[cost.type] += cost.amount;
        }

        foreach (KeyValuePair<IngredientType, int> requirement in required)
        {
            int currentAmount = player.inventory.GetAmount(requirement.Key);

            // Simulamos la compra.
            if (requirement.Key == card.ingredientType)
            {
                currentAmount += card.amount;
            }

            if (currentAmount < requirement.Value)
            {
                return false;
            }
        }

        return true;
    }

    private void EvaluateOpponents()
    {
        if (playerManager == null)
            return;

        List<PlayerState> opponents = playerManager.GetOpponents(player);

        foreach (PlayerState opponent in opponents)
        {
            Debug.Log(
                $"Bot detecta rival: "
                    + $"{opponent.gameObject.name} | "
                    + $"Vida: {opponent.currentHP}/{opponent.maxHP} | "
                    + $"Escudo: {opponent.shield}"
            );
        }
    }

    private int EvaluatePlayerAttack(SpellInstance spell, PlayerState target)
    {
        int damage = spell.definition.GetEffectValue(spell.level);

        int score = damage;

        ShieldPiercingType piercing = spell.definition.GetShieldPiercing(spell.level);

        int effectiveDamage = damage;

        if (piercing == ShieldPiercingType.None && target.shield > 0)
        {
            effectiveDamage = Mathf.Max(0, damage - target.shield);
        }

        if (piercing == ShieldPiercingType.IgnoreOne && target.shield > 0)
        {
            int directDamage = Mathf.Min(1, damage);

            int remaining = damage - directDamage;

            int afterShield = Mathf.Max(0, remaining - target.shield);

            effectiveDamage = directDamage + afterShield;
        }

        if (piercing == ShieldPiercingType.IgnoreAll)
        {
            effectiveDamage = damage;
        }

        score += effectiveDamage * 3;

        // Si puede matar al rival, prioridad enorme.
        if (effectiveDamage >= target.currentHP)
        {
            score += 200;
        }

        return score;
    }

    private void TryIllusion()
    {
        if (!player.CanCastSpell())
        {
            return;
        }

        if (ingredientDeck == null)
        {
            Debug.LogError("BotController: IngredientDeck no está asignado.");

            return;
        }

        foreach (SpellInstance spell in spellBook.Spells)
        {
            if (spell == null || spell.definition == null)
            {
                continue;
            }

            if (spell.definition.effectType != SpellEffectType.Illusion)
            {
                continue;
            }

            if (!player.inventory.CanAfford(spell.definition))
            {
                continue;
            }

            int drawAmount = spell.definition.GetEffectValue(spell.level);

            int keepAmount = spell.definition.GetSecondaryEffectValue(spell.level);

            bool spent = player.inventory.Spend(spell.definition);

            if (!spent)
            {
                return;
            }

            List<IngredientType> revealed = new List<IngredientType>();

            for (int i = 0; i < drawAmount; i++)
            {
                IngredientType ingredient = ingredientDeck.Draw();

                revealed.Add(ingredient);

                Debug.Log($"Bot revela con Ilusión: {ingredient}");
            }

            revealed.Sort(
                (a, b) => GetIngredientUsefulness(b).CompareTo(GetIngredientUsefulness(a))
            );

            int amountToKeep = Mathf.Min(keepAmount, revealed.Count);

            for (int i = 0; i < revealed.Count; i++)
            {
                IngredientType ingredient = revealed[i];

                if (i < amountToKeep)
                {
                    player.inventory.Add(ingredient, 1);

                    Debug.Log($"Bot conserva {ingredient} gracias a Ilusión.");
                }
                else
                {
                    ingredientDeck.Discard(ingredient);

                    Debug.Log($"Bot descarta {ingredient} revelado por Ilusión.");
                }
            }

            player.RegisterSpellCast();

            spell.AddMastery();

            Debug.Log(
                $"Bot lanza {spell.definition.spellName}. "
                    + $"Revela {drawAmount}, conserva {amountToKeep}. "
                    + $"Nivel: {spell.level}, Maestría: {spell.mastery}."
            );

            return;
        }
    }
}
