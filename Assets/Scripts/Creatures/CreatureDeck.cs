using System.Collections.Generic;
using UnityEngine;

public class CreatureDeck : MonoBehaviour
{
    [Header("Creature Definitions")]
    [SerializeField]
    private List<CreatureDefinition> creatureDefinitions;

    private readonly List<CreatureDefinition> deck = new List<CreatureDefinition>();

    private readonly List<CreatureDefinition> discardPile = new List<CreatureDefinition>();

    private bool initialized = false;

    public void Initialize()
{
    if (initialized)
        return;

    BuildDeck();
    ShuffleDeck();

    initialized = true;

    Debug.Log(
        $"Mazo de criaturas preparado con {deck.Count} cartas."
    );
}

    private void Awake()
    {
        Initialize();
    }

    private void BuildDeck()
    {
        deck.Clear();

        foreach (CreatureDefinition creature in creatureDefinitions)
        {
            deck.Add(creature);
        }

        Debug.Log($"Mazo de criaturas creado con {deck.Count} cartas.");
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);

            CreatureDefinition temp = deck[i];

            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public CreatureDefinition Draw()
    {
        Initialize();
        
        if (deck.Count == 0)
        {
            RebuildFromDiscard();
        }

        if (deck.Count == 0)
        {
            Debug.LogWarning("No quedan criaturas disponibles.");

            return null;
        }

        CreatureDefinition creature = deck[0];

        deck.RemoveAt(0);

        return creature;
    }

    public void Discard(CreatureDefinition creature)
    {
        if (creature != null)
        {
            discardPile.Add(creature);
        }
    }

    private void RebuildFromDiscard()
    {
        deck.AddRange(discardPile);
        discardPile.Clear();

        ShuffleDeck();
    }
}
