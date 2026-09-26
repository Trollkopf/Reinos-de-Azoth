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

    [SerializeField]
    private MarketPanelController marketPanelController;

    private readonly List<MarketCardDefinition> visibleCards = new List<MarketCardDefinition>();

    private readonly List<MarketCardView> cardViews = new List<MarketCardView>();

    private bool freeRewardActive = false;
    private PlayerState freeRewardPlayer = null;

    private const int VisibleSlots = 5;

    private void Awake()
    {
        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (visibleCards.Count > 0)
            return;

        if (marketDeck == null)
        {
            Debug.LogError("MarketView: MarketDeck no está asignado.");
            return;
        }

        marketDeck.Initialize();

        for (int i = 0; i < VisibleSlots; i++)
        {
            MarketCardDefinition card = marketDeck.Draw();

            if (card == null)
                continue;

            visibleCards.Add(card);

            MarketCardView view = Instantiate(marketCardPrefab, cardContainer);

            view.Setup(card, this, i);

            cardViews.Add(view);
        }

        Debug.Log($"Mercado inicializado con {visibleCards.Count} cartas.");
    }

    public void TryBuy(int slotIndex, PlayerState buyer)
    {
        EnsureInitialized();

        if (buyer == null)
            return;

        if (slotIndex < 0 || slotIndex >= visibleCards.Count)
        {
            return;
        }

        MarketCardDefinition card = visibleCards[slotIndex];

        if (card == null)
            return;

        bool isFreeReward = freeRewardActive && freeRewardPlayer == buyer;

        if (!isFreeReward)
        {
            if (buyer.coins < card.price)
            {
                Debug.Log($"No tienes monedas suficientes. " + $"Necesitas {card.price}.");

                return;
            }

            buyer.SpendCoins(card.price);
        }

        buyer.inventory.Add(card.ingredientType, card.amount);

        if (isFreeReward)
        {
            Debug.Log(
                $"{buyer.gameObject.name} obtiene gratis "
                    + $"{card.amount} x {card.ingredientType} "
                    + "como recompensa del Dragón de Azufre."
            );

            freeRewardActive = false;
            freeRewardPlayer = null;

            if (marketPanelController != null && buyer == player)
            {
                marketPanelController.CloseMarket();
            }
        }
        else
        {
            Debug.Log(
                $"Comprado {card.amount} x "
                    + $"{card.ingredientType} "
                    + $"por {card.price} monedas."
            );
        }

        marketDeck.Discard(card);

        ReplaceSlot(slotIndex);

        if (buyer == player)
        {
            if (inventoryView != null)
            {
                inventoryView.Refresh();
            }

            if (playerStatusView != null)
            {
                playerStatusView.Refresh();
            }
        }
    }

    private void ReplaceSlot(int slotIndex)
    {
        MarketCardDefinition newCard = marketDeck.Draw();

        if (newCard == null)
            return;

        visibleCards[slotIndex] = newCard;

        cardViews[slotIndex].Setup(newCard, this, slotIndex);
    }

    public PlayerState GetPlayer()
    {
        return player;
    }

    public int GetVisibleCardCount()
    {
        EnsureInitialized();

        return visibleCards.Count;
    }

    public MarketCardDefinition GetCardAt(int index)
    {
        EnsureInitialized();

        if (index < 0 || index >= visibleCards.Count)
        {
            return null;
        }

        return visibleCards[index];
    }

    public void BeginFreeReward(PlayerState rewardPlayer)
    {
        if (rewardPlayer == null)
        {
            return;
        }

        EnsureInitialized();

        freeRewardActive = true;
        freeRewardPlayer = rewardPlayer;

        Debug.Log(
            $"{rewardPlayer.gameObject.name} puede elegir " + "1 carta visible del Mercado gratis."
        );
    }

    public bool HasFreeRewardFor(PlayerState buyer)
    {
        return freeRewardActive && freeRewardPlayer == buyer;
    }
}
