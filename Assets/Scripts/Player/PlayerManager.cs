using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private List<PlayerState> players = new List<PlayerState>();

    public List<PlayerState> Players => players;

    public List<PlayerState> GetOpponents(PlayerState player)
    {
        List<PlayerState> opponents = new List<PlayerState>();

        foreach (PlayerState other in players)
        {
            if (other != null && other != player && other.currentHP > 0)
            {
                opponents.Add(other);
            }
        }

        return opponents;
    }
}
