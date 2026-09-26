using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private PlayerState player;

    [SerializeField]
    private IngredientDeck ingredientDeck;

    [SerializeField]
    private SpellBookView spellBookView;

    [SerializeField]
    private IngredientInventoryView inventoryView;

    [SerializeField]
    private PlayerStatusView playerStatusView;

    [SerializeField]
    private PlayerState botPlayer;

    [SerializeField]
    private PlayerManager playerManager;

    [SerializeField]
    private EndGamePanel endGamePanel;

    public bool IsGameOver { get; private set; }

    public PlayerState Winner { get; private set; }

    public bool IsCoronationActive { get; private set; }

    public PlayerState CoronationPlayer { get; private set; }

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
        if (player == null || botPlayer == null || ingredientDeck == null)
        {
            Debug.LogWarning(
                "GameManager: no se inicia la partida porque " + "faltan referencias principales."
            );

            return;
        }

        StartGame();
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

        ingredientDeck.DrawToPlayer(player, 3);

        ingredientDeck.DrawToPlayer(botPlayer, 3);

        if (inventoryView != null)
        {
            inventoryView.Refresh();
        }

        Debug.Log("Partida iniciada. " + "El jugador y el bot roban 3 ingredientes.");

        if (spellBookView != null)
        {
            spellBookView.RefreshBook();
        }

        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }
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

        Debug.Log($"PARTIDA TERMINADA. " + $"Ganador: {winner.gameObject.name}.");

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
                + $"ha alcanzado {coronationPlayer.arcanePower} "
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
            Debug.Log("La Coronación fracasa porque " + "el jugador coronado ha sido eliminado.");

            IsCoronationActive = false;
            CoronationPlayer = null;

            return;
        }

        Debug.Log($"{CoronationPlayer.gameObject.name} " + "ha sobrevivido a la última ronda.");

        EndGame(CoronationPlayer);
    }
}
