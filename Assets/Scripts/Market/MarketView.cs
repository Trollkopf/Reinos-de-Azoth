using System.Collections.Generic;
using UnityEngine;

public class MarketView : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private MarketDeck marketDeck;

    [SerializeField]
    private PlayerState player;

    [SerializeField]
    private IngredientInventoryView inventoryView;

    [SerializeField]
    private MarketCardView marketCardPrefab;

    [SerializeField]
    private Transform cardContainer;

    [SerializeField]
    private PlayerStatusView playerStatusView;

    private readonly List<MarketCardDefinition> visibleCards = new List<MarketCardDefinition>();

    private readonly List<MarketCardView> cardViews = new List<MarketCardView>();

    private const int VisibleSlots = 5;

    private void Start()
    {
        InitializeMarket();
    }

    private void InitializeMarket()
    {
        for (int i = 0; i < VisibleSlots; i++)
        {
            MarketCardDefinition card = marketDeck.Draw();

            visibleCards.Add(card);

            MarketCardView view = Instantiate(marketCardPrefab, cardContainer);

            view.Setup(card, this, i);

            cardViews.Add(view);
        }
    }

    public void TryBuy(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= visibleCards.Count)
        {
            return;
        }

        MarketCardDefinition card = visibleCards[slotIndex];

        if (card == null)
            return;

        if (player.coins < card.price)
        {
            Debug.Log($"No tienes monedas suficientes. Necesitas {card.price}.");

            return;
        }

        player.coins -= card.price;

        player.inventory.Add(card.ingredientType, card.amount);

        Debug.Log(
            $"Comprado {card.amount} x {card.ingredientType} " + $"por {card.price} monedas."
        );

        marketDeck.Discard(card);

        ReplaceSlot(slotIndex);

        inventoryView.Refresh();

        playerStatusView.Refresh();
    }

    private void ReplaceSlot(int slotIndex)
    {
        MarketCardDefinition newCard = marketDeck.Draw();

        visibleCards[slotIndex] = newCard;

        cardViews[slotIndex].Setup(newCard, this, slotIndex);
    }
}
