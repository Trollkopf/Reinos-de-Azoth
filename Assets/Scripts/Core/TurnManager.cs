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

    [SerializeField]
    private PlayerState botPlayer;

    [SerializeField]
    private BotController botController;

    private int turnNumber = 1;
    private bool isPlayerTurn = true;

    public void EndTurn()
    {
        // Solo el jugador humano usa este botón
        if (!isPlayerTurn)
            return;

        // No puede acabar con más de 7 ingredientes
        if (player.inventory.GetTotalCount() > PlayerState.MaxHandSize)
        {
            Debug.Log("Debes descartar ingredientes antes de terminar el turno.");

            return;
        }

        Debug.Log("Termina el turno del jugador.");

        isPlayerTurn = false;

        StartBotTurn();
    }

    private void StartTurn()
    {
        Debug.Log($"===== TURNO {turnNumber} DEL JUGADOR =====");

        ingredientDeck.DrawToPlayer(player, 1);

        if (spellBookView != null)
        {
            spellBookView.RefreshBook();
        }

        DebugInventory();
    }

    private void StartBotTurn()
    {
        Debug.Log("===== TURNO DEL BOT =====");

        ingredientDeck.DrawToPlayer(botPlayer, 1);

        botController.StartBotTurn();

        // De momento el bot termina automáticamente
        EndBotTurn();
    }

    private void EndBotTurn()
    {
        botController.DiscardDownToHandLimit();

        Debug.Log("Termina el turno del bot.");

        isPlayerTurn = true;

        turnNumber++;

        StartTurn();
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
