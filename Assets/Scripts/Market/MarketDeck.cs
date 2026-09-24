using System.Collections.Generic;
using UnityEngine;

public class MarketDeck : MonoBehaviour
{
    [Header("Definitions")]
    [SerializeField]
    private MarketCardDefinition redHerbCard;

    [SerializeField]
    private MarketCardDefinition pureWaterCard;

    [SerializeField]
    private MarketCardDefinition sulfurCard;

    [SerializeField]
    private MarketCardDefinition airCrystalCard;

    [SerializeField]
    private MarketCardDefinition boneDustCard;

    private readonly List<MarketCardDefinition> deck = new List<MarketCardDefinition>();

    private readonly List<MarketCardDefinition> discardPile = new List<MarketCardDefinition>();

    private bool initialized = false;

    public void Initialize()
    {
        if (initialized)
            return;

        BuildDeck();
        Shuffle();

        initialized = true;

        Debug.Log($"Mazo de mercado preparado con {deck.Count} cartas.");
    }

    private void Awake()
    {
        Initialize();
    }

    private void BuildDeck()
    {
        deck.Clear();

        AddCopies(redHerbCard, 12);
        AddCopies(pureWaterCard, 12);
        AddCopies(sulfurCard, 8);
        AddCopies(airCrystalCard, 8);
        AddCopies(boneDustCard, 8);

        Debug.Log($"Mazo de mercado creado: {deck.Count} cartas.");
    }

    private void AddCopies(MarketCardDefinition definition, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            deck.Add(definition);
        }
    }

    private void Shuffle()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);

            MarketCardDefinition temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public MarketCardDefinition Draw()
    {
        Initialize();

        if (deck.Count == 0)
        {
            RebuildFromDiscard();
        }

        if (deck.Count == 0)
            return null;

        MarketCardDefinition card = deck[0];

        deck.RemoveAt(0);

        return card;
    }

    public void Discard(MarketCardDefinition card)
    {
        if (card != null)
        {
            discardPile.Add(card);
        }
    }

    private void RebuildFromDiscard()
    {
        deck.AddRange(discardPile);
        discardPile.Clear();

        Shuffle();
    }
}
