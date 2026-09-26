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

    [SerializeField]
    private GameManager gameManager;

    private int turnNumber = 1;

    private bool isPlayerTurn = true;

    public void EndTurn()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        if (!isPlayerTurn)
        {
            return;
        }

        if (player.inventory.GetTotalCount() > PlayerState.MaxHandSize)
        {
            Debug.Log(
                $"Debes descartar ingredientes hasta tener "
                    + $"{PlayerState.MaxHandSize} antes de terminar el turno."
            );

            return;
        }

        Debug.Log("Termina el turno del jugador.");

        player.EndTurn();

        if (creaturePanel != null)
        {
            creaturePanel.ClearEndOfTurnEffects();
        }

        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        // Si hay Coronación y el jugador humano NO es
        // quien se coronó, este era su último turno
        // para intentar detener al coronado.
        if (
            gameManager != null
            && gameManager.IsCoronationActive
            && gameManager.CoronationPlayer != player
        )
        {
            Debug.Log("El jugador termina su último turno " + "de la ronda de Coronación.");

            gameManager.ResolveCoronation();

            return;
        }

        isPlayerTurn = false;

        StartBotTurn();
    }

    private void StartTurn()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        Debug.Log($"===== TURNO {turnNumber} DEL JUGADOR =====");

        if (creaturePanel != null)
        {
            creaturePanel.ResolveStartOfRoundEffects();
        }

        player.BeginTurn();

        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        ingredientDeck.DrawToPlayer(player, 1);

        if (spellBookView != null)
        {
            spellBookView.RefreshBook();
        }

        DebugInventory();
    }

    private void StartBotTurn()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        Debug.Log("===== TURNO DEL BOT =====");

        botPlayer.BeginTurn();

        // El bot puede morir aquí por Quemadura.
        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        ingredientDeck.DrawToPlayer(botPlayer, 1);

        botController.StartBotTurn();

        // El bot puede matar al jugador durante sus acciones.
        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        EndBotTurn();
    }

    private void EndBotTurn()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        botController.DiscardDownToHandLimit();

        Debug.Log("Termina el turno del bot.");

        botPlayer.EndTurn();

        if (creaturePanel != null)
        {
            creaturePanel.ClearEndOfTurnEffects();
        }

        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }

        // Si hay Coronación y el bot NO es
        // quien se coronó, este era su último turno
        // para intentar detener al coronado.
        if (
            gameManager != null
            && gameManager.IsCoronationActive
            && gameManager.CoronationPlayer != botPlayer
        )
        {
            Debug.Log("El bot termina su último turno " + "de la ronda de Coronación.");

            gameManager.ResolveCoronation();

            return;
        }

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
