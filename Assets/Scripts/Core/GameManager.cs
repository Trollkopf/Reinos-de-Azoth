using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField]
    private List<PlayerState> playerSlots = new List<PlayerState>();

    [Header("Game References")]
    [SerializeField]
    private IngredientDeck ingredientDeck;

    [SerializeField]
    private SpellBookView spellBookView;

    [SerializeField]
    private IngredientInventoryView inventoryView;

    [SerializeField]
    private PlayerStatusView playerStatusView;

    [SerializeField]
    private PlayerManager playerManager;

    [SerializeField]
    private EndGamePanel endGamePanel;

    [SerializeField]
    private OpponentLayoutController opponentLayoutController;

    private readonly List<PlayerState> activePlayers = new List<PlayerState>();

    private readonly Dictionary<PlayerState, PlayerType> playerTypes =
        new Dictionary<PlayerState, PlayerType>();

    public bool IsGameOver { get; private set; }

    public PlayerState Winner { get; private set; }

    public bool IsCoronationActive { get; private set; }

    public PlayerState CoronationPlayer { get; private set; }

    public IReadOnlyList<PlayerState> ActivePlayers => activePlayers;

    private void OnEnable()
    {
        if (playerManager != null)
        {
            playerManager.OnLastPlayerStanding += HandleLastPlayerStanding;

            playerManager.OnCoronationClaimed += HandleCoronationClaimed;
        }
    }

    private void OnDisable()
    {
        if (playerManager != null)
        {
            playerManager.OnLastPlayerStanding -= HandleLastPlayerStanding;

            playerManager.OnCoronationClaimed -= HandleCoronationClaimed;
        }
    }

    private void Start()
    {
        if (ingredientDeck == null || playerManager == null)
        {
            Debug.LogWarning("GameManager: faltan referencias " + "principales.");

            return;
        }

        ConfigurePlayers();

        if (activePlayers.Count < 2)
        {
            Debug.LogError("GameManager: se necesitan al menos " + "2 jugadores activos.");

            return;
        }

        StartGame();
    }

    private void ConfigurePlayers()
    {
        activePlayers.Clear();
        playerTypes.Clear();

        for (int i = 0; i < playerSlots.Count; i++)
        {
            PlayerState slot = playerSlots[i];

            if (slot == null)
            {
                continue;
            }

            PlayerType playerType = GetConfiguredPlayerType(i);

            bool isActive = playerType != PlayerType.Empty;

            slot.gameObject.SetActive(isActive);

            if (!isActive)
            {
                continue;
            }

            activePlayers.Add(slot);

            playerTypes.Add(slot, playerType);

            Debug.Log($"Jugador {i + 1} activado: {playerType}.");
        }

        playerManager.ConfigurePlayers(activePlayers);

        if (opponentLayoutController != null && activePlayers.Count > 0)
        {
            List<PlayerState> opponents = playerManager.GetOpponents(activePlayers[0]);

            opponentLayoutController.ShowOpponents(opponents);
        }

        Debug.Log($"Partida configurada con " + $"{activePlayers.Count} jugadores.");
    }

    private PlayerType GetConfiguredPlayerType(int index)
    {
        if (GameSetup.Players == null || index < 0 || index >= GameSetup.Players.Length)
        {
            return PlayerType.Empty;
        }

        return GameSetup.Players[index];
    }

    private void StartGame()
    {
        IsGameOver = false;
        Winner = null;
        IsCoronationActive = false;
        CoronationPlayer = null;

        if (endGamePanel != null)
        {
            endGamePanel.Hide();
        }

        foreach (PlayerState activePlayer in activePlayers)
        {
            ingredientDeck.DrawToPlayer(activePlayer, 3);
        }

        if (inventoryView != null)
        {
            inventoryView.Refresh();
        }

        if (spellBookView != null)
        {
            spellBookView.RefreshBook();
        }

        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }

        Debug.Log(
            $"Partida iniciada. " + $"{activePlayers.Count} jugadores " + "roban 3 ingredientes."
        );
    }

    public PlayerType GetPlayerType(PlayerState target)
    {
        if (target != null && playerTypes.TryGetValue(target, out PlayerType type))
        {
            return type;
        }

        return PlayerType.Empty;
    }

    public bool IsHumanPlayer(PlayerState target)
    {
        return GetPlayerType(target) == PlayerType.Human;
    }

    public bool IsBotPlayer(PlayerState target)
    {
        return GetPlayerType(target) == PlayerType.Bot;
    }

    private void HandleLastPlayerStanding(PlayerState winner)
    {
        EndGame(winner);
    }

    private void EndGame(PlayerState winner)
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        IsCoronationActive = false;
        Winner = winner;

        Debug.Log($"PARTIDA TERMINADA. " + $"Ganador: " + $"{winner.gameObject.name}.");

        if (endGamePanel != null)
        {
            endGamePanel.Show(winner);
        }
    }

    private void HandleCoronationClaimed(PlayerState coronationPlayer)
    {
        if (
            IsGameOver
            || IsCoronationActive
            || coronationPlayer == null
            || !coronationPlayer.IsAlive
        )
        {
            return;
        }

        IsCoronationActive = true;
        CoronationPlayer = coronationPlayer;

        Debug.Log(
            $"¡CORONACIÓN! "
                + $"{coronationPlayer.gameObject.name} "
                + $"ha alcanzado "
                + $"{coronationPlayer.arcanePower} "
                + "de Poder Arcano."
        );

        Debug.Log("Comienza la última ronda.");
    }

    public void ResolveCoronation()
    {
        if (IsGameOver || !IsCoronationActive)
        {
            return;
        }

        if (CoronationPlayer == null || !CoronationPlayer.IsAlive)
        {
            Debug.Log(
                "La Coronación fracasa porque " + "el jugador coronado " + "ha sido eliminado."
            );

            IsCoronationActive = false;
            CoronationPlayer = null;

            return;
        }

        Debug.Log(
            $"{CoronationPlayer.gameObject.name} " + "ha sobrevivido " + "a la última ronda."
        );

        EndGame(CoronationPlayer);
    }
}
