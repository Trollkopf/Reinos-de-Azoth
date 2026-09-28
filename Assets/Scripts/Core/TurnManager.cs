using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Game")]
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private IngredientDeck ingredientDeck;

    [SerializeField]
    private CreaturePanel creaturePanel;

    [SerializeField]
    private MarketView marketView;

    [Header("Human UI")]
    [SerializeField]
    private SpellBookView spellBookView;

    [SerializeField]
    private IngredientInventoryView inventoryView;

    [SerializeField]
    private PlayerStatusView playerStatusView;

    [SerializeField]
    private PlayerTargetSelectionManager playerTargetSelectionManager;

    [Header("Bots")]
    [SerializeField]
    private List<BotController> botControllers = new List<BotController>();

    private int currentPlayerIndex = 0;

    private int roundNumber = 1;

    private bool turnStarted = false;

    public PlayerState CurrentPlayer
    {
        get
        {
            if (
                gameManager == null
                || gameManager.ActivePlayers == null
                || gameManager.ActivePlayers.Count == 0
            )
            {
                return null;
            }

            if (currentPlayerIndex < 0 || currentPlayerIndex >= gameManager.ActivePlayers.Count)
            {
                return null;
            }

            return gameManager.ActivePlayers[currentPlayerIndex];
        }
    }

    private IEnumerator Start()
    {
        /*
         * Esperamos un frame porque GameManager.Start()
         * tiene que configurar primero los jugadores
         * seleccionados desde GameSetup.
         */
        yield return null;

        if (gameManager == null || gameManager.ActivePlayers.Count < 2)
        {
            Debug.LogError("TurnManager: no hay suficientes " + "jugadores activos.");

            yield break;
        }

        currentPlayerIndex = 0;
        roundNumber = 1;

        /*
         * El primer jugador ya recibió sus
         * 3 ingredientes iniciales desde GameManager,
         * así que NO roba uno adicional.
         */
        StartCurrentTurn(true);
    }

    public void EndTurn()
    {
        if (gameManager == null || gameManager.IsGameOver)
        {
            return;
        }

        PlayerState currentPlayer = CurrentPlayer;

        if (currentPlayer == null || !currentPlayer.IsAlive)
        {
            return;
        }

        /*
         * El botón de terminar turno solamente
         * puede controlar jugadores humanos.
         */
        if (!gameManager.IsHumanPlayer(currentPlayer))
        {
            return;
        }

        if (currentPlayer.inventory.GetTotalCount() > PlayerState.MaxHandSize)
        {
            Debug.Log(
                $"Debes descartar ingredientes hasta tener "
                    + $"{PlayerState.MaxHandSize} "
                    + "antes de terminar el turno."
            );

            return;
        }

        FinishCurrentTurn();
    }

    private void StartCurrentTurn(bool initialTurn = false)
    {
        if (gameManager == null || gameManager.IsGameOver)
        {
            return;
        }

        PlayerState currentPlayer = CurrentPlayer;

        if (currentPlayer == null)
        {
            return;
        }

        /*
         * Por seguridad, si por algún motivo
         * hemos llegado a un jugador muerto,
         * saltamos al siguiente.
         */
        if (!currentPlayer.IsAlive)
        {
            AdvanceToNextPlayer();
            return;
        }

        turnStarted = true;

        Debug.Log(
            $"===== RONDA {roundNumber} | "
                + $"TURNO DE "
                + $"{currentPlayer.gameObject.name} ====="
        );

        currentPlayer.BeginTurn();

        /*
         * BeginTurn puede matar al jugador por
         * Quemadura, Veneno, etc.
         */
        if (gameManager.IsGameOver)
        {
            return;
        }

        if (!currentPlayer.IsAlive)
        {
            AdvanceToNextPlayer();
            return;
        }

        /*
         * En el primer turno de la partida
         * mantenemos el comportamiento anterior:
         * empieza con los 3 ingredientes iniciales
         * y no roba un cuarto.
         */
        if (!initialTurn)
        {
            ingredientDeck.DrawToPlayer(currentPlayer, 1);
        }

        if (gameManager.IsHumanPlayer(currentPlayer))
        {
            StartHumanTurn(currentPlayer);
        }
        else if (gameManager.IsBotPlayer(currentPlayer))
        {
            StartBotTurn(currentPlayer);
        }
    }

    private void StartHumanTurn(PlayerState currentPlayer)
    {
        Debug.Log($"{currentPlayer.gameObject.name} " + "es controlado por un humano.");

        if (playerTargetSelectionManager != null)
        {
            playerTargetSelectionManager.SetCurrentPlayer(currentPlayer);
        }

        if (inventoryView != null)
        {
            inventoryView.SetPlayer(currentPlayer);
        }

        if (playerStatusView != null)
        {
            playerStatusView.SetPlayer(currentPlayer);
        }

        if (spellBookView != null)
        {
            spellBookView.SetPlayer(currentPlayer);
        }

        if (marketView != null)
        {
            marketView.SetPlayer(currentPlayer);
        }

        Debug.Log($"La interfaz pertenece ahora a " + $"{currentPlayer.gameObject.name}.");

        /*
         * Aquí termina la inicialización.
         * Ahora esperamos a que el humano juegue
         * y pulse Finalizar turno.
         */
    }

    private void StartBotTurn(PlayerState botPlayer)
    {
        BotController controller = GetBotController(botPlayer);

        if (controller == null)
        {
            Debug.LogError(
                $"No se encuentra BotController para " + $"{botPlayer.gameObject.name}."
            );

            /*
             * Evitamos bloquear la partida
             * si falta una referencia.
             */
            FinishCurrentTurn();

            return;
        }

        StartCoroutine(RunBotTurn(controller));
    }

    private IEnumerator RunBotTurn(BotController controller)
    {
        /*
         * Dejamos un frame entre jugadores.
         * También evita encadenar recursivamente
         * varios bots en el mismo call stack.
         */
        yield return null;

        if (gameManager == null || gameManager.IsGameOver)
        {
            yield break;
        }

        controller.StartBotTurn();

        if (gameManager.IsGameOver)
        {
            yield break;
        }

        controller.DiscardDownToHandLimit();

        if (gameManager.IsGameOver)
        {
            yield break;
        }

        FinishCurrentTurn();
    }

    private void FinishCurrentTurn()
    {
        if (gameManager == null || gameManager.IsGameOver || !turnStarted)
        {
            return;
        }

        PlayerState currentPlayer = CurrentPlayer;

        if (currentPlayer == null)
        {
            return;
        }

        turnStarted = false;

        Debug.Log($"Termina el turno de " + $"{currentPlayer.gameObject.name}.");

        currentPlayer.EndTurn();

        if (creaturePanel != null)
        {
            creaturePanel.ClearEndOfTurnEffects();
        }

        if (gameManager.IsGameOver)
        {
            return;
        }

        /*
         * El aspirante puede haber muerto
         * durante la ronda de Coronación.
         *
         * GameManager.ResolveCoronation()
         * cancelará la Coronación en ese caso.
         */
        if (
            gameManager.IsCoronationActive
            && (gameManager.CoronationPlayer == null || !gameManager.CoronationPlayer.IsAlive)
        )
        {
            gameManager.ResolveCoronation();

            if (gameManager.IsGameOver)
            {
                return;
            }
        }

        AdvanceToNextPlayer();
    }

    private void AdvanceToNextPlayer()
    {
        if (gameManager == null || gameManager.IsGameOver)
        {
            return;
        }

        int playerCount = gameManager.ActivePlayers.Count;

        if (playerCount == 0)
        {
            return;
        }

        int previousIndex = currentPlayerIndex;

        int nextIndex = FindNextAlivePlayerIndex();

        if (nextIndex < 0)
        {
            return;
        }

        bool wrappedRound = nextIndex <= previousIndex;

        currentPlayerIndex = nextIndex;

        PlayerState nextPlayer = CurrentPlayer;

        /*
         * Si hemos dado toda la vuelta y el
         * siguiente jugador vivo vuelve a ser
         * quien reclamó la Coronación,
         * todos sus rivales ya tuvieron su
         * oportunidad de responder.
         */
        if (gameManager.IsCoronationActive && nextPlayer == gameManager.CoronationPlayer)
        {
            gameManager.ResolveCoronation();

            return;
        }

        if (wrappedRound)
        {
            roundNumber++;

            Debug.Log($"===== COMIENZA RONDA " + $"{roundNumber} =====");

            /*
             * Regeneración del Gólem Óseo y
             * futuros efectos de inicio de ronda.
             */
            if (creaturePanel != null)
            {
                creaturePanel.ResolveStartOfRoundEffects();
            }
        }

        StartCurrentTurn();
    }

    private int FindNextAlivePlayerIndex()
    {
        int playerCount = gameManager.ActivePlayers.Count;

        for (int offset = 1; offset <= playerCount; offset++)
        {
            int index = (currentPlayerIndex + offset) % playerCount;

            PlayerState candidate = gameManager.ActivePlayers[index];

            if (candidate != null && candidate.IsAlive)
            {
                return index;
            }
        }

        return -1;
    }

    private BotController GetBotController(PlayerState botPlayer)
    {
        foreach (BotController controller in botControllers)
        {
            if (controller == null)
            {
                continue;
            }

            if (controller.IsControlledPlayer(botPlayer))
            {
                return controller;
            }
        }

        return null;
    }
}
