using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketCardView : MonoBehaviour
{
    [SerializeField]
    private Image artworkImage;

    [SerializeField]
    private TMP_Text amountText;

    [SerializeField]
    private TMP_Text priceText;

    [SerializeField]
    private Button buyButton;

    private MarketCardDefinition definition;
    private MarketView marketView;
    private int slotIndex;

    [SerializeField]
    private IngredientIconDatabase ingredientIconDatabase;

    public void Setup(
        MarketCardDefinition newDefinition,
        MarketView newMarketView,
        int newSlotIndex
    )
    {
        definition = newDefinition;
        marketView = newMarketView;
        slotIndex = newSlotIndex;

        Refresh();
    }

    public void Refresh()
    {
        if (definition == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        Sprite sprite = ingredientIconDatabase.GetSprite(definition.ingredientType);

        artworkImage.sprite = sprite;
        artworkImage.enabled = sprite != null;

        amountText.text = $"x{definition.amount}";

        priceText.text = $"{definition.price} monedas";
    }

    public void Buy()
    {
        marketView.TryBuy(slotIndex, marketView.GetPlayer());
    }

    private string GetIngredientDisplayName(IngredientType type)
    {
        return type switch
        {
            IngredientType.RedHerb => "Hierba Roja",
            IngredientType.PureWater => "Agua Pura",
            IngredientType.SulfurMineral => "Mineral Sulfuroso",
            IngredientType.AirCrystal => "Cristal de Aire",
            IngredientType.BoneDust => "Polvo de Hueso",
            _ => type.ToString(),
        };
    }
}
