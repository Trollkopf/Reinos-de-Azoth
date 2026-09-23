using System.Collections.Generic;
using UnityEngine;

public class IngredientDeck : MonoBehaviour
{
    private List<IngredientType> deck = new List<IngredientType>();

    private List<IngredientType> discardPile = new List<IngredientType>();

    private void Awake()
    {
        BuildDeck();
        ShuffleDeck();
    }

    private void BuildDeck()
    {
        deck.Clear();

        AddIngredients(IngredientType.RedHerb, 25);
        AddIngredients(IngredientType.PureWater, 25);
        AddIngredients(IngredientType.SulfurMineral, 22);
        AddIngredients(IngredientType.AirCrystal, 20);
        AddIngredients(IngredientType.BoneDust, 16);

        Debug.Log($"Mazo creado con {deck.Count} ingredientes.");
    }

    private void AddIngredients(IngredientType type, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            deck.Add(type);
        }
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);

            IngredientType temp = deck[i];

            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }

        Debug.Log("Mazo de ingredientes barajado.");
    }

    public IngredientType Draw()
    {
        if (deck.Count == 0)
        {
            RebuildDeckFromDiscard();
        }

        IngredientType ingredient = deck[0];

        deck.RemoveAt(0);

        return ingredient;
    }

    public void Discard(IngredientType ingredient)
    {
        discardPile.Add(ingredient);
    }

    private void RebuildDeckFromDiscard()
    {
        if (discardPile.Count == 0)
        {
            Debug.LogWarning("No quedan ingredientes en el mazo ni en descarte.");

            return;
        }

        deck.AddRange(discardPile);
        discardPile.Clear();

        ShuffleDeck();

        Debug.Log("Mazo reconstruido desde la pila de descarte.");
    }

    public int GetDeckCount()
    {
        return deck.Count;
    }

    public int GetDiscardCount()
    {
        return discardPile.Count;
    }

    public void DrawToPlayer(PlayerState player, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            IngredientType ingredient = Draw();

            player.inventory.Add(ingredient);

            Debug.Log($"{player.name} roba {ingredient}");
        }
    }
}
