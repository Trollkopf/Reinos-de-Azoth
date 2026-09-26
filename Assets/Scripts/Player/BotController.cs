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

    [SerializeField]
    private GameManager gameManager;

    private class OffensiveDecision
    {
        public SpellInstance spell;
        public CreatureView creatureTarget;
        public PlayerState playerTarget;
        public int score;
    }

    public void StartBotTurn()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        Debug.Log(
            $"Empieza turno del bot. "
                + $"Vida: {player.currentHP}, "
                + $"Escudo: {player.shield}, "
                + $"Monedas: {player.coins}, "
                + $"Poder Arcano: {player.arcanePower}, "
                + $"Ingredientes: {player.inventory.GetTotalCount()}"
        );

        EvaluateOpponents();

        TryClaimFreeMarketReward();

        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        EvaluateAvailableSpells();

        /*
         * MODO CORONACIÓN:
         * si otro jugador está coronado,
         * el bot deja de preocuparse por criaturas,
         * curarse, escudarse o hacer otras historias.
         *
         * Compra si puede mejorar su mano
         * y después va A POR EL CORONADO.
         */
        if (GetEnemyCoronationTarget() != null)
        {
            Debug.Log(
                $"¡CORONACIÓN ACTIVA! "
                    + $"{player.gameObject.name} concentra todos sus ataques "
                    + $"en {GetEnemyCoronationTarget().gameObject.name}."
            );

            TryMultipleMarketPurchases();

            if (gameManager != null && gameManager.IsGameOver)
            {
                return;
            }

            EvaluateAvailableSpells();

            TryMultipleOffensiveSpells();

            return;
        }

        TryMultipleMarketPurchases();

        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        EvaluateAvailableSpells();

        TryIllusion();

        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        TryHeal();

        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        TryShield();

        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

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

        if (spellResolver == null || playerManager == null)
        {
            return false;
        }

        List<CreatureView> creatures =
            creaturePanel != null
                ? creaturePanel.GetActiveCreatureViews()
                : new List<CreatureView>();

        PlayerState coronationTarget = GetEnemyCoronationTarget();

        bool huntingCoronationTarget = coronationTarget != null;

        OffensiveDecision bestDecision = new OffensiveDecision { score = int.MinValue };

        if (!huntingCoronationTarget)
        {
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
        }

        List<PlayerState> opponents;

        if (huntingCoronationTarget)
        {
            opponents = new List<PlayerState> { coronationTarget };
        }
        else
        {
            opponents = playerManager.GetOpponents(player);
        }

        foreach (SpellInstance spell in spellBook.Spells)
        {
            if (spell == null || spell.definition == null)
                continue;

            if (!player.inventory.CanAfford(spell.definition))
                continue;

            bool canAttackPlayer =
                spell.definition.effectType == SpellEffectType.Damage
                || spell.definition.effectType == SpellEffectType.Drain
                || spell.definition.effectType == SpellEffectType.WindWhip
                || spell.definition.effectType == SpellEffectType.Roots
                || spell.definition.effectType == SpellEffectType.AcidExplosion;

            if (!canAttackPlayer)
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

        TryClaimFreeMarketReward();

        if (gameManager != null && gameManager.IsGameOver)
        {
            return true;
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

        // Si estamos cerca de Coronación y esta criatura
        // puede darnos el Poder Arcano que falta,
        // se convierte en objetivo prioritario.
        int arcanePowerNeeded = Mathf.Max(0, 10 - player.arcanePower);

        bool creatureCanTriggerCoronation =
            arcanePowerNeeded > 0 && creature.definition.arcanePowerReward >= arcanePowerNeeded;

        if (creatureCanTriggerCoronation)
        {
            // Prioridad estratégica enorme:
            // matar esta criatura puede iniciar la Coronación.
            score += 5000;

            // Cuanto más cerca esté de morir,
            // más interesante es seguir golpeándola.
            score += Mathf.Max(0, 100 - creature.currentHP * 10);

            // Si ESTE hechizo ya la mata,
            // prioridad prácticamente absoluta.
            if (damage >= creature.currentHP)
            {
                score += 20000;
            }

            Debug.Log(
                $"Bot detecta oportunidad de Coronación: "
                    + $"{creature.definition.creatureName} puede darle "
                    + $"+{creature.definition.arcanePowerReward} Poder Arcano. "
                    + $"Necesita {arcanePowerNeeded}."
            );
        }

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
            if (gameManager != null && gameManager.IsGameOver)
            {
                return;
            }

            bool castSomething = TryCastOffensiveSpell();

            if (!castSomething)
            {
                break;
            }

            if (gameManager != null && gameManager.IsGameOver)
            {
                return;
            }
        }
    }

    public void DiscardDownToHandLimit()
    {
        while (player.inventory.GetTotalCount() > PlayerState.MaxHandSize)
        {
            IngredientType? ingredientToDiscard = FindLeastUsefulIngredient();

            if (ingredientToDiscard == null)
                break;

            player.DiscardIngredient(ingredientToDiscard.Value);

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

            bool spent = player.SpendIngredientsForSpell(spell.definition);

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

            bool spent = player.SpendIngredientsForSpell(spell.definition);

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
        SpellEffectType effectType = spell.definition.effectType;

        int damage;

        if (effectType == SpellEffectType.WindWhip)
        {
            damage = spell.definition.GetSecondaryEffectValue(spell.level);
        }
        else
        {
            damage = spell.definition.GetEffectValue(spell.level);
        }

        if (target.statusEffects != null && target.statusEffects.corroded && damage > 0)
        {
            damage += 1;
        }

        int effectiveDamage = damage;

        /*
         * Solo los hechizos Damage usan actualmente
         * nuestro sistema de perforación de escudo.
         */
        if (effectType == SpellEffectType.Damage)
        {
            ShieldPiercingType piercing = spell.definition.GetShieldPiercing(spell.level);

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
        }
        else
        {
            effectiveDamage = Mathf.Max(0, damage - target.shield);
        }

        int score = damage;

        score += effectiveDamage * 3;

        // Kill directa.
        if (effectiveDamage >= target.currentHP)
        {
            score += 200;
        }

        // Drenaje además cura al bot.
        if (effectType == SpellEffectType.Drain)
        {
            int missingHP = player.maxHP - player.currentHP;

            int healing = spell.definition.GetSecondaryEffectValue(spell.level);

            score += Mathf.Min(missingHP, healing) * 3;
        }

        // Látigo fuerza descarte.
        if (effectType == SpellEffectType.WindWhip)
        {
            if (target.inventory.GetTotalCount() > 0)
            {
                score += 6;
            }

            if (spell.level >= 3)
            {
                score += 4;
            }
        }

        // Raíces destroza el siguiente turno.
        if (effectType == SpellEffectType.Roots)
        {
            score += 8;
        }

        // Ácido Nv.3 prepara los siguientes ataques.
        if (effectType == SpellEffectType.AcidExplosion && spell.level >= 3)
        {
            score += 8;
        }

        // Durante Coronación, atacar al coronado
        // domina cualquier otra decisión.
        if (
            gameManager != null
            && gameManager.IsCoronationActive
            && gameManager.CoronationPlayer == target
        )
        {
            score += 10000;
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

            bool spent = player.SpendIngredientsForSpell(spell.definition);

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

    private PlayerState GetEnemyCoronationTarget()
    {
        if (
            gameManager == null
            || !gameManager.IsCoronationActive
            || gameManager.CoronationPlayer == null
            || !gameManager.CoronationPlayer.IsAlive
            || gameManager.CoronationPlayer == player
        )
        {
            return null;
        }

        return gameManager.CoronationPlayer;
    }

    private void TryClaimFreeMarketReward()
    {
        if (marketView == null || !marketView.HasFreeRewardFor(player))
        {
            return;
        }

        int cardCount = marketView.GetVisibleCardCount();

        int bestSlot = -1;
        int bestScore = int.MinValue;

        for (int i = 0; i < cardCount; i++)
        {
            MarketCardDefinition card = marketView.GetCardAt(i);

            if (card == null)
            {
                continue;
            }

            int score = 0;

            foreach (SpellInstance spell in spellBook.Spells)
            {
                if (spell == null || spell.definition == null)
                {
                    continue;
                }

                // Si esta carta permite completar
                // inmediatamente un hechizo, interesa mucho.
                if (
                    !player.inventory.CanAfford(spell.definition)
                    && WouldEnableSpell(spell.definition, card)
                )
                {
                    score += 20;
                }

                // Además valoramos ingredientes útiles
                // para varios hechizos.
                foreach (IngredientCost cost in spell.definition.cost)
                {
                    if (cost.type == card.ingredientType)
                    {
                        score += card.amount * 3;
                    }
                }
            }

            // Polvo de Hueso es más escaso,
            // así que una carta gratuita tiene más valor.
            if (card.ingredientType == IngredientType.BoneDust)
            {
                score += 5;
            }

            Debug.Log(
                $"Bot evalúa recompensa del Dragón: "
                    + $"{card.amount}x {card.ingredientType} "
                    + $"→ puntuación {score}"
            );

            if (score > bestScore)
            {
                bestScore = score;
                bestSlot = i;
            }
        }

        if (bestSlot < 0)
        {
            return;
        }

        MarketCardDefinition chosenCard = marketView.GetCardAt(bestSlot);

        Debug.Log(
            $"Bot elige como recompensa del Dragón "
                + $"{chosenCard.amount}x "
                + $"{chosenCard.ingredientType}."
        );

        marketView.TryBuy(bestSlot, player);
    }

    public void GrantSpectralLordMasteryReward()
    {
        if (spellBook == null || spellBook.Spells == null || spellBook.Spells.Count == 0)
        {
            return;
        }

        List<SpellInstance> candidates = new List<SpellInstance>();

        foreach (SpellInstance spell in spellBook.Spells)
        {
            if (spell == null || spell.definition == null || spell.level >= 3)
            {
                continue;
            }

            candidates.Add(spell);
        }

        if (candidates.Count == 0)
        {
            Debug.Log("El bot no tiene hechizos que puedan recibir más Maestría.");

            return;
        }

        int bestDistance = int.MaxValue;

        int bestTargetLevel = int.MinValue;

        int bestUsage = int.MinValue;

        List<SpellInstance> bestCandidates = new List<SpellInstance>();

        foreach (SpellInstance spell in candidates)
        {
            int requirement = spell.GetNextMasteryRequirement();

            int distance = requirement - spell.mastery;

            int targetLevel = spell.level + 1;

            int usage = spell.mastery;

            Debug.Log(
                $"Bot evalúa Maestría: "
                    + $"{spell.definition.spellName} | "
                    + $"Nv.{spell.level} → Nv.{targetLevel} | "
                    + $"Distancia: {distance} | "
                    + $"Usos: {usage}"
            );

            bool isBetter = false;

            if (distance < bestDistance)
            {
                isBetter = true;
            }
            else if (distance == bestDistance && targetLevel > bestTargetLevel)
            {
                isBetter = true;
            }
            else if (
                distance == bestDistance
                && targetLevel == bestTargetLevel
                && usage > bestUsage
            )
            {
                isBetter = true;
            }

            if (isBetter)
            {
                bestDistance = distance;

                bestTargetLevel = targetLevel;

                bestUsage = usage;

                bestCandidates.Clear();

                bestCandidates.Add(spell);
            }
            else if (
                distance == bestDistance
                && targetLevel == bestTargetLevel
                && usage == bestUsage
            )
            {
                bestCandidates.Add(spell);
            }
        }

        if (bestCandidates.Count == 0)
        {
            return;
        }

        SpellInstance selectedSpell = bestCandidates[Random.Range(0, bestCandidates.Count)];

        int previousLevel = selectedSpell.level;

        int previousMastery = selectedSpell.mastery;

        selectedSpell.AddMastery();

        Debug.Log(
            $"El bot concede la recompensa del Señor Espectral a "
                + $"{selectedSpell.definition.spellName}: "
                + $"Maestría {previousMastery} → "
                + $"{selectedSpell.mastery}."
        );

        if (selectedSpell.level > previousLevel)
        {
            Debug.Log(
                $"¡{selectedSpell.definition.spellName} "
                    + $"del bot sube a Nivel "
                    + $"{selectedSpell.level}!"
            );
        }
    }

    public bool IsControlledPlayer(PlayerState target)
    {
        return player == target;
    }
}
