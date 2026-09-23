using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public const int MaxHandSize = 10;

    [Header("Player Stats")]
    public int maxHP = 15;
    public int currentHP = 15;
    public int shield = 0;
    public const int MaxShield = 6;

    public int coins = 3;
    public int arcanePower = 0;

    public IngredientInventory inventory = new IngredientInventory();

    public void DiscardIngredient(IngredientType type)
    {
        if (inventory.Remove(type))
        {
            Debug.Log($"Descartado: {type}");
        }
    }
}
