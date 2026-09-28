using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetSelectionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CreatureSelectionManager creatureSelectionManager;

    [SerializeField]
    private PlayerManager playerManager;

    public PlayerState SelectedPlayer { get; private set; }

    public PlayerState CurrentPlayer { get; private set; }

    public void SetCurrentPlayer(PlayerState currentPlayer)
    {
        CurrentPlayer = currentPlayer;

        ClearSelection();

        string playerName =
            currentPlayer != null ? currentPlayer.gameObject.name : "ningún jugador";

        Debug.Log($"Selector PvP preparado para {playerName}.");
    }

    public void SelectPlayer(PlayerState targetPlayer)
    {
        if (targetPlayer == null)
        {
            ClearSelection();
            return;
        }

        if (CurrentPlayer == null)
        {
            Debug.LogWarning("No hay jugador activo para " + "seleccionar un objetivo PvP.");

            return;
        }

        if (targetPlayer == CurrentPlayer)
        {
            Debug.Log("Un jugador no puede seleccionarse " + "a sí mismo como objetivo.");

            return;
        }

        if (!targetPlayer.IsAlive)
        {
            Debug.Log(
                $"{targetPlayer.gameObject.name} " + "está eliminado y no puede " + "ser objetivo."
            );

            return;
        }

        if (
            playerManager != null
            && !playerManager.GetOpponents(CurrentPlayer).Contains(targetPlayer)
        )
        {
            Debug.LogWarning($"{targetPlayer.gameObject.name} " + "no es un objetivo PvP válido.");

            return;
        }

        /*
         * Si seleccionamos un jugador,
         * dejamos de tener criatura objetivo.
         */
        if (creatureSelectionManager != null)
        {
            creatureSelectionManager.ClearSelection();
        }

        SelectedPlayer = targetPlayer;

        Debug.Log($"Jugador objetivo seleccionado: " + $"{targetPlayer.gameObject.name}");
    }

    public List<PlayerState> GetAvailableTargets()
    {
        if (CurrentPlayer == null || playerManager == null)
        {
            return new List<PlayerState>();
        }

        return playerManager.GetOpponents(CurrentPlayer);
    }

    public bool IsValidTarget(PlayerState targetPlayer)
    {
        if (
            CurrentPlayer == null
            || targetPlayer == null
            || targetPlayer == CurrentPlayer
            || !targetPlayer.IsAlive
        )
        {
            return false;
        }

        if (playerManager == null)
        {
            return true;
        }

        return playerManager.GetOpponents(CurrentPlayer).Contains(targetPlayer);
    }

    public void ClearSelection()
    {
        SelectedPlayer = null;
    }

    
}
