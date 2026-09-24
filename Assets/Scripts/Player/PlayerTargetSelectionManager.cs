using UnityEngine;

public class PlayerTargetSelectionManager : MonoBehaviour
{
    public PlayerState SelectedPlayer { get; private set; }

    public void SelectPlayer(PlayerState player)
    {
        SelectedPlayer = player;

        Debug.Log($"Jugador objetivo seleccionado: {player.gameObject.name}");
    }

    public void ClearSelection()
    {
        SelectedPlayer = null;
    }
}
