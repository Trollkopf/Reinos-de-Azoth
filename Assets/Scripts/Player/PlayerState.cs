using System;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public const int MaxHandSize = 10;
    public const int MaxShield = 6;

    [Header("Player Stats")]
    public int maxHP = 15;
    public int currentHP = 15;
    public int shield = 0;
    public int coins = 3;
    public int arcanePower = 0;

    public IngredientInventory inventory = new IngredientInventory();

    public event Action OnStatsChanged;

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        currentHP = Mathf.Max(0, currentHP - amount);

        NotifyStatsChanged();
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
        arcanePower += amount;

        NotifyStatsChanged();
    }

    public void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    public void DiscardIngredient(IngredientType type)
    {
        if (inventory.Remove(type))
        {
            Debug.Log($"Descartado: {type}");
        }
    }
}
