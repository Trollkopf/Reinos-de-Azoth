using UnityEngine;

[CreateAssetMenu(fileName = "NewMarketCard", menuName = "Reinos de Azoth/Market Card")]
public class MarketCardDefinition : ScriptableObject
{
    [Header("Ingredient")]
    public IngredientType ingredientType;

    [Min(1)]
    public int amount = 1;

    [Header("Price")]
    [Min(0)]
    public int price = 1;
}
