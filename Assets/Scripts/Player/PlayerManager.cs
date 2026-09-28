using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public const int PlayerKillArcanePowerReward = 2;

    [SerializeField]
    private List<PlayerState> players = new List<PlayerState>();

    public List<PlayerState> Players => players;

    public event Action<PlayerState> OnLastPlayerStanding;

    public event Action<PlayerState> OnCoronationClaimed;

    private void OnEnable()
    {
        SubscribeToPlayers();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayers();
    }

    public void ConfigurePlayers(List<PlayerState> newPlayers)
    {
        UnsubscribeFromPlayers();

        players.Clear();

        if (newPlayers != null)
        {
            foreach (PlayerState newPlayer in newPlayers)
            {
                if (newPlayer == null || players.Contains(newPlayer))
                {
                    continue;
                }

                players.Add(newPlayer);
            }
        }

        SubscribeToPlayers();

        Debug.Log($"PlayerManager configurado con " + $"{players.Count} jugadores.");
    }

    private void SubscribeToPlayers()
    {
        foreach (PlayerState player in players)
        {
            if (player == null)
            {
                continue;
            }

            player.OnPlayerDied -= HandlePlayerDeath;

            player.OnPlayerDied += HandlePlayerDeath;

            player.OnCoronationThresholdReached -= HandleCoronationThresholdReached;

            player.OnCoronationThresholdReached += HandleCoronationThresholdReached;
        }
    }

    private void UnsubscribeFromPlayers()
    {
        foreach (PlayerState player in players)
        {
            if (player == null)
            {
                continue;
            }

            player.OnPlayerDied -= HandlePlayerDeath;

            player.OnCoronationThresholdReached -= HandleCoronationThresholdReached;
        }
    }

    private void HandlePlayerDeath(PlayerState deadPlayer, PlayerState killer)
    {
        Debug.Log($"{deadPlayer.gameObject.name} " + "ha sido eliminado.");

        if (killer != null && killer != deadPlayer)
        {
            killer.AddArcanePower(PlayerKillArcanePowerReward);

            Debug.Log(
                $"{killer.gameObject.name} obtiene "
                    + $"+{PlayerKillArcanePowerReward} "
                    + "Poder Arcano por eliminar a "
                    + $"{deadPlayer.gameObject.name}."
            );
        }
        else
        {
            Debug.Log("La eliminación no tiene " + "un jugador responsable.");
        }

        CheckLastPlayerStanding();
    }

    private void CheckLastPlayerStanding()
    {
        List<PlayerState> alivePlayers = GetAlivePlayers();

        Debug.Log($"Jugadores vivos: " + $"{alivePlayers.Count}.");

        if (alivePlayers.Count == 1)
        {
            PlayerState winner = alivePlayers[0];

            Debug.Log($"¡{winner.gameObject.name} " + "es el último jugador vivo!");

            OnLastPlayerStanding?.Invoke(winner);
        }
    }

    public List<PlayerState> GetAlivePlayers()
    {
        List<PlayerState> alivePlayers = new List<PlayerState>();

        foreach (PlayerState player in players)
        {
            if (player != null && player.IsAlive)
            {
                alivePlayers.Add(player);
            }
        }

        return alivePlayers;
    }

    public List<PlayerState> GetOpponents(PlayerState player)
    {
        List<PlayerState> opponents = new List<PlayerState>();

        foreach (PlayerState other in players)
        {
            if (other != null && other != player && other.IsAlive)
            {
                opponents.Add(other);
            }
        }

        return opponents;
    }

    public bool IsRegisteredPlayer(PlayerState player)
    {
        return player != null && players.Contains(player);
    }

    public int GetPlayerCount()
    {
        return players.Count;
    }

    private void HandleCoronationThresholdReached(PlayerState player)
    {
        if (player == null || !player.IsAlive)
        {
            return;
        }

        Debug.Log(
            $"{player.gameObject.name} "
                + "reclama la Coronación "
                + $"con {player.arcanePower} "
                + "de Poder Arcano."
        );

        OnCoronationClaimed?.Invoke(player);
    }
}
