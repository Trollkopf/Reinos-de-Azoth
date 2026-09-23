using System.Collections.Generic;
using System;

[System.Serializable]
public class IngredientInventory
{
    private Dictionary<IngredientType, int> ingredients = new Dictionary<IngredientType, int>();
    public event Action OnChanged;

    public IngredientInventory()
    {
        foreach (IngredientType type in System.Enum.GetValues(typeof(IngredientType)))
        {
            ingredients[type] = 0;
        }
    }

    public void Add(IngredientType type, int amount = 1)
    {
        ingredients[type] += amount;
        OnChanged?.Invoke();
    }

    public bool Has(IngredientType type, int amount)
    {
        return ingredients[type] >= amount;
    }

    public int GetAmount(IngredientType type)
    {
        return ingredients[type];
    }

    public bool CanAfford(SpellDefinition spell)
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

        foreach (var requirement in required)
        {
            if (!Has(requirement.Key, requirement.Value))
            {
                return false;
            }
        }

        return true;
    }

    public bool Spend(SpellDefinition spell)
    {
        if (!CanAfford(spell))
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

        foreach (var requirement in required)
        {
            ingredients[requirement.Key] -= requirement.Value;
        }
        OnChanged?.Invoke();

        return true;
    }

    public int GetTotalCount()
    {
        int total = 0;

        foreach (var entry in ingredients)
        {
            total += entry.Value;
        }

        return total;
    }

    public bool Remove(IngredientType type, int amount = 1)
    {
        if (!Has(type, amount))
        {
            return false;
        }

        ingredients[type] -= amount;
        OnChanged?.Invoke();
        return true;
    }
}
