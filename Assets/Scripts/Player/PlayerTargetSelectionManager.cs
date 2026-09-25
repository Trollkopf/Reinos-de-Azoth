using UnityEngine;

public class PlayerTargetSelectionManager : MonoBehaviour
{
    [SerializeField]
    private CreatureSelectionManager creatureSelectionManager;

    public PlayerState SelectedPlayer { get; private set; }

    public void SelectPlayer(PlayerState player)
    {
        // Si seleccionamos un jugador,
        // dejamos de tener criatura objetivo.
        if (creatureSelectionManager != null)
        {
            creatureSelectionManager.ClearSelection();
        }

        SelectedPlayer = player;

        Debug.Log($"Jugador objetivo seleccionado: {player.gameObject.name}");
    }

    public void ClearSelection()
    {
        SelectedPlayer = null;
    }
}
