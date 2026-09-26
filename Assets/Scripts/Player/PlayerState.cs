using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField]
    private IngredientDeck ingredientDeck;

    public const int MaxHandSize = 10;
    public const int MaxShield = 6;

    [Header("Player Stats")]
    public int maxHP = 15;
    public int currentHP = 15;
    public int shield = 0;
    public int coins = 3;
    public int arcanePower = 0;

    public IngredientInventory inventory = new IngredientInventory();

    public PlayerStatusEffects statusEffects = new PlayerStatusEffects();

    public event Action OnStatsChanged;
    public event Action<PlayerState, PlayerState> OnPlayerDied;
    public event Action<PlayerState> OnCoronationThresholdReached;
    public bool IsAlive => currentHP > 0;

    private int spellsCastThisTurn = 0;

    public int SpellsCastThisTurn => spellsCastThisTurn;

    public void BeginTurn()
    {
        spellsCastThisTurn = 0;

        statusEffects.BeginTurn();

        ResolveBurns();

        if (currentHP > 0)
        {
            ResolvePoison();
        }

        if (statusEffects.spellLimitThisTurn > 0)
        {
            Debug.Log(
                $"{gameObject.name} está limitado a "
                    + $"{statusEffects.spellLimitThisTurn} hechizo(s) este turno."
            );
        }
    }

    public void EndTurn()
    {
        statusEffects.ClearEndOfTurnEffects();
    }

    private void ResolveBurns()
    {
        if (statusEffects == null || statusEffects.burns == null || statusEffects.burns.Count == 0)
        {
            return;
        }

        Debug.Log(
            $"{gameObject.name} sufre daño por "
                + $"{statusEffects.burns.Count} acumulación(es) de Quemadura."
        );

        List<BurnStack> expiredBurns = new List<BurnStack>();

        foreach (BurnStack burn in statusEffects.burns)
        {
            if (currentHP <= 0)
            {
                break;
            }

            if (burn == null)
            {
                continue;
            }

            PlayerState source = burn.source;

            TakeDamageWithShield(1, source);

            burn.turnsRemaining--;

            if (burn.turnsRemaining <= 0)
            {
                expiredBurns.Add(burn);
            }
        }

        foreach (BurnStack expiredBurn in expiredBurns)
        {
            statusEffects.burns.Remove(expiredBurn);
        }

        Debug.Log(
            $"{gameObject.name} mantiene "
                + $"{statusEffects.burns.Count} acumulación(es) de Quemadura."
        );
    }

    public void TakeDamageWithShield(int amount, PlayerState source = null)
    {
        if (amount <= 0 || currentHP <= 0)
        {
            return;
        }

        int remainingDamage = amount;

        if (shield > 0)
        {
            int absorbed = Mathf.Min(shield, remainingDamage);

            RemoveShield(absorbed);

            remainingDamage -= absorbed;

            Debug.Log($"{gameObject.name}: el escudo absorbe " + $"{absorbed} de daño.");
        }

        if (remainingDamage > 0)
        {
            TakeDamage(remainingDamage, source);
        }
    }

    public bool CanCastSpell()
    {
        int limit = statusEffects.spellLimitThisTurn;

        if (limit < 0)
        {
            return true;
        }

        return spellsCastThisTurn < limit;
    }

    public void RegisterSpellCast()
    {
        spellsCastThisTurn++;

        Debug.Log(
            $"{gameObject.name} ha lanzado " + $"{spellsCastThisTurn} hechizo(s) este turno."
        );
    }

    public int GetRemainingSpellCasts()
    {
        int limit = statusEffects.spellLimitThisTurn;

        if (limit < 0)
        {
            return -1;
        }

        return Mathf.Max(0, limit - spellsCastThisTurn);
    }

    public void TakeDamage(int amount, PlayerState source = null)
    {
        if (amount <= 0 || currentHP <= 0)
        {
            return;
        }

        currentHP = Mathf.Max(0, currentHP - amount);

        NotifyStatsChanged();

        if (currentHP <= 0)
        {
            Debug.Log($"{gameObject.name} ha sido eliminado.");

            OnPlayerDied?.Invoke(this, source);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        currentHP = Mathf.Min(maxHP, currentHP + amount);

        NotifyStatsChanged();
    }

    public void AddShield(int amount)
    {
        if (amount <= 0)
            return;

        shield = Mathf.Min(MaxShield, shield + amount);

        NotifyStatsChanged();
    }

    public void RemoveShield(int amount)
    {
        if (amount <= 0)
            return;

        shield = Mathf.Max(0, shield - amount);

        NotifyStatsChanged();
    }

    public void AddCoins(int amount)
    {
        coins += amount;

        NotifyStatsChanged();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
            return false;

        coins -= amount;

        NotifyStatsChanged();

        return true;
    }

    public void AddArcanePower(int amount)
    {
        if (amount <= 0)
            return;

        int previousArcanePower = arcanePower;

        arcanePower += amount;

        NotifyStatsChanged();

        if (previousArcanePower < 10 && arcanePower >= 10)
        {
            Debug.Log(
                $"{gameObject.name} ha alcanzado "
                    + $"{arcanePower} de Poder Arcano "
                    + "y reclama la Coronación."
            );

            OnCoronationThresholdReached?.Invoke(this);
        }
    }

    public void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    public void DiscardIngredient(IngredientType type)
    {
        if (!inventory.Remove(type))
        {
            return;
        }

        if (ingredientDeck != null)
        {
            ingredientDeck.Discard(type);

            Debug.Log(
                $"Descartado: {type}. " + $"Pila de descartes: {ingredientDeck.GetDiscardCount()}."
            );
        }
        else
        {
            Debug.LogWarning($"Se ha descartado {type}, pero IngredientDeck no está asignado.");
        }
    }

    public bool SpendIngredientsForSpell(SpellDefinition spell)
    {
        if (spell == null || !inventory.CanAfford(spell))
        {
            return false;
        }

        Dictionary<IngredientType, int> required = new Dictionary<IngredientType, int>();

        foreach (IngredientCost cost in spell.cost)
        {
            if (!required.ContainsKey(cost.type))
            {
                required[cost.type] = 0;
            }

            required[cost.type] += cost.amount;
        }

        if (!inventory.Spend(spell))
        {
            return false;
        }

        if (ingredientDeck != null)
        {
            foreach (KeyValuePair<IngredientType, int> requirement in required)
            {
                for (int i = 0; i < requirement.Value; i++)
                {
                    ingredientDeck.Discard(requirement.Key);
                }
            }

            Debug.Log(
                $"Ingredientes gastados enviados al descarte. "
                    + $"Pila actual: {ingredientDeck.GetDiscardCount()}."
            );
        }
        else
        {
            Debug.LogWarning(
                "PlayerState: IngredientDeck no está asignado. "
                    + "Los ingredientes gastados no han podido ir al descarte."
            );
        }

        return true;
    }

    private void ResolvePoison()
    {
        if (statusEffects == null || !statusEffects.poisoned)
        {
            return;
        }

        Debug.Log($"{gameObject.name} sufre 1 de daño por Veneno.");

        statusEffects.poisoned = false;

        TakeDamageWithShield(1);
    }
}
