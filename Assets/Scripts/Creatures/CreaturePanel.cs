using System.Collections.Generic;
using UnityEngine;

public class CreaturePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CreatureDeck creatureDeck;

    [SerializeField]
    private CreatureView creaturePrefab;

    [SerializeField]
    private Transform creatureContainer;

    [SerializeField]
    private CreatureSelectionManager selectionManager;

    private readonly List<CreatureInstance> activeCreatures = new List<CreatureInstance>();

    private readonly List<CreatureView> creatureViews = new List<CreatureView>();

    private const int VisibleCreatureCount = 3;

    private void Awake()
    {
        InitializeCreatures();
    }

    private void InitializeCreatures()
    {
        for (int i = 0; i < VisibleCreatureCount; i++)
        {
            CreatureDefinition definition = creatureDeck.Draw();

            if (definition == null)
                continue;

            CreatureInstance instance = new CreatureInstance(definition);

            activeCreatures.Add(instance);

            CreatureView view = Instantiate(creaturePrefab, creatureContainer);

            view.Setup(instance);

            view.SetSelectionManager(selectionManager);

            creatureViews.Add(view);
        }
    }

    public void ReplaceCreature(CreatureView creatureView)
    {
        int index = creatureViews.IndexOf(creatureView);

        if (index < 0)
            return;

        CreatureInstance oldCreature = activeCreatures[index];

        creatureDeck.Discard(oldCreature.definition);

        CreatureDefinition newDefinition = creatureDeck.Draw();

        if (newDefinition == null)
            return;

        CreatureInstance newInstance = new CreatureInstance(newDefinition);

        activeCreatures[index] = newInstance;

        creatureView.Setup(newInstance);
        creatureView.SetSelectionManager(selectionManager);
    }

    public List<CreatureView> GetActiveCreatureViews()
    {
        return creatureViews;
    }

    public void ClearCorrosion()
    {
        foreach (CreatureInstance creature in activeCreatures)
        {
            if (creature != null)
            {
                creature.isCorroded = false;
            }
        }

        foreach (CreatureView view in creatureViews)
        {
            if (view != null)
            {
                view.Refresh();
            }
        }

        Debug.Log("La corrosión de las criaturas ha desaparecido.");
    }
}
