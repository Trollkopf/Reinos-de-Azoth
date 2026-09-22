using System.Collections.Generic;

[System.Serializable]
public class IngredientInventory
{
    private Dictionary<IngredientType, int> ingredients =
        new Dictionary<IngredientType, int>();

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
        foreach (IngredientCost cost in spell.cost)
        {
            if (!Has(cost.type, cost.amount))
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

        foreach (IngredientCost cost in spell.cost)
        {
            ingredients[cost.type] -= cost.amount;
        }

        return true;
    }
}