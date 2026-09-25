using UnityEngine;

public class CreatureSelectionManager : MonoBehaviour
{
    public CreatureView SelectedCreature { get; private set; }

    [SerializeField]
    private SelectedCreatureView selectedCreatureView;

    [SerializeField]
    private PlayerTargetSelectionManager playerTargetSelectionManager;

    public void SelectCreature(CreatureView creature)
    {
        // Si seleccionamos una criatura,
        // dejamos de tener jugador objetivo.
        if (playerTargetSelectionManager != null)
        {
            playerTargetSelectionManager.ClearSelection();
        }

        if (SelectedCreature != null)
        {
            SelectedCreature.SetSelected(false);
        }

        SelectedCreature = creature;

        if (SelectedCreature != null)
        {
            SelectedCreature.SetSelected(true);
        }

        if (selectedCreatureView != null)
        {
            selectedCreatureView.ShowCreature(SelectedCreature);
        }

        Debug.Log($"Criatura seleccionada: {SelectedCreature?.GetCreatureName()}");
    }

    public void ClearSelection()
    {
        if (SelectedCreature != null)
        {
            SelectedCreature.SetSelected(false);
        }

        if (selectedCreatureView != null)
        {
            selectedCreatureView.Clear();
        }

        SelectedCreature = null;
    }
}
