using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientInventoryView : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    private PlayerState player;

    [Header("Ingredient Texts")]
    [SerializeField]
    private TMP_Text redHerbText;

    [SerializeField]
    private TMP_Text pureWaterText;

    [SerializeField]
    private TMP_Text sulfurText;

    [SerializeField]
    private TMP_Text airCrystalText;

    [SerializeField]
    private TMP_Text boneDustText;

    [Header("Total")]
    [SerializeField]
    private TMP_Text totalText;

    [Header("Discard Buttons")]
    [SerializeField]
    private Button discardRedHerbButton;

    [SerializeField]
    private Button discardPureWaterButton;

    [SerializeField]
    private Button discardSulfurButton;

    [SerializeField]
    private Button discardAirCrystalButton;

    [SerializeField]
    private Button discardBoneDustButton;

    public void Refresh()
    {
        if (player == null)
            return;

        IngredientInventory inventory = player.inventory;

        int redHerb = inventory.GetAmount(IngredientType.RedHerb);

        int pureWater = inventory.GetAmount(IngredientType.PureWater);

        int sulfur = inventory.GetAmount(IngredientType.SulfurMineral);

        int airCrystal = inventory.GetAmount(IngredientType.AirCrystal);

        int boneDust = inventory.GetAmount(IngredientType.BoneDust);

        redHerbText.text = $"{redHerb}";

        pureWaterText.text = $"{pureWater}";

        sulfurText.text = $"{sulfur}";

        airCrystalText.text = $"{airCrystal}";

        boneDustText.text = $"{boneDust}";

        int total = inventory.GetTotalCount();

        totalText.text = $"Total: {total} / {PlayerState.MaxHandSize}";

        if (total > PlayerState.MaxHandSize)
        {
            totalText.color = Color.red;
        }
        else
        {
            totalText.color = Color.black;
        }

        bool mustDiscard = total > PlayerState.MaxHandSize;

        discardRedHerbButton.gameObject.SetActive(mustDiscard);
        discardPureWaterButton.gameObject.SetActive(mustDiscard);
        discardSulfurButton.gameObject.SetActive(mustDiscard);
        discardAirCrystalButton.gameObject.SetActive(mustDiscard);
        discardBoneDustButton.gameObject.SetActive(mustDiscard);
    }

    public void DiscardRedHerb()
    {
        DiscardIngredient(IngredientType.RedHerb);
    }

    public void DiscardPureWater()
    {
        DiscardIngredient(IngredientType.PureWater);
    }

    public void DiscardSulfur()
    {
        DiscardIngredient(IngredientType.SulfurMineral);
    }

    public void DiscardAirCrystal()
    {
        DiscardIngredient(IngredientType.AirCrystal);
    }

    public void DiscardBoneDust()
    {
        DiscardIngredient(IngredientType.BoneDust);
    }

    private void DiscardIngredient(IngredientType type)
    {
        if (player == null)
            return;

        bool removed = player.inventory.Remove(type);

        if (!removed)
        {
            Debug.Log($"No tienes {type} para descartar.");

            return;
        }

        Debug.Log($"Descartado: {type}");

        Refresh();
    }

    private void Start()
    {
        SubscribeToPlayer();
        Refresh();
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayer();
    }

    public void SetPlayer(PlayerState newPlayer)
    {
        if (player == newPlayer)
        {
            Refresh();
            return;
        }

        UnsubscribeFromPlayer();

        player = newPlayer;

        SubscribeToPlayer();

        Refresh();
    }

    private void SubscribeToPlayer()
    {
        if (player != null && player.inventory != null)
        {
            player.inventory.OnChanged -= Refresh;
            player.inventory.OnChanged += Refresh;
        }
    }

    private void UnsubscribeFromPlayer()
    {
        if (player != null && player.inventory != null)
        {
            player.inventory.OnChanged -= Refresh;
        }
    }
}
