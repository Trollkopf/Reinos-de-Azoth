using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerState player;

    [SerializeField]
    private PlayerSpellBook spellBook;

    [SerializeField]
    private SpellBookView spellBookView;

    [SerializeField]
    private IngredientDeck ingredientDeck;

    [SerializeField]
    private IngredientInventoryView inventoryView;

    [SerializeField]
    private CreaturePanel creaturePanel;

    private int turnNumber = 1;

    public void EndTurn()
    {
        int ingredientCount = player.inventory.GetTotalCount();

        if (ingredientCount > PlayerState.MaxHandSize)
        {
            Debug.Log(
                $"No puedes terminar el turno. "
                    + $"Tienes {ingredientCount} ingredientes y el máximo es {PlayerState.MaxHandSize}."
            );

            return;
        }

        if (creaturePanel != null)
        {
            creaturePanel.ClearCorrosion();
        }

        Debug.Log($"Fin del turno {turnNumber}");

        turnNumber++;

        StartTurn();
    }

    private void StartTurn()
    {
        Debug.Log($"Comienza el turno {turnNumber}");

        // Robar 1 ingrediente al inicio del turno
        ingredientDeck.DrawToPlayer(player, 1);

        // Refrescar el grimorio
        spellBookView.RefreshBook();

        // Refrescar la vista del inventario
        inventoryView.Refresh();

        DebugInventory();
    }

    private void DebugInventory()
    {
        Debug.Log(
            $"Inventario → "
                + $"Hierba: {player.inventory.GetAmount(IngredientType.RedHerb)} | "
                + $"Agua: {player.inventory.GetAmount(IngredientType.PureWater)} | "
                + $"Azufre: {player.inventory.GetAmount(IngredientType.SulfurMineral)} | "
                + $"Cristal: {player.inventory.GetAmount(IngredientType.AirCrystal)} | "
                + $"Hueso: {player.inventory.GetAmount(IngredientType.BoneDust)}"
        );
    }
}
