using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHP = 15;
    public int currentHP = 15;

    public int coins = 3;
    public int arcanePower = 0;

    public IngredientInventory inventory =
        new IngredientInventory();

    private void Awake()
    {
        // Ingredientes de prueba
        inventory.Add(IngredientType.RedHerb, 3);
        inventory.Add(IngredientType.PureWater, 1);
        inventory.Add(IngredientType.SulfurMineral, 3);

        Debug.Log(
            $"Hierba: {inventory.GetAmount(IngredientType.RedHerb)} | " +
            $"Agua: {inventory.GetAmount(IngredientType.PureWater)} | " +
            $"Azufre: {inventory.GetAmount(IngredientType.SulfurMineral)}"
        );
    }
}