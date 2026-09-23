using UnityEngine;

public class CreatureSelectionManager : MonoBehaviour
{
    public CreatureView SelectedCreature { get; private set; }

    [SerializeField]
    private SelectedCreatureView selectedCreatureView;

    public void SelectCreature(CreatureView creature)
    {
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
