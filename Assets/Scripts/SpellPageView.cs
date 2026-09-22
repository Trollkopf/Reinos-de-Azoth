using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellPageView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField]
    private Image artwork;

    [SerializeField]
    private TMP_Text spellNameText;

    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text masteryText;

    [SerializeField]
    private Image masteryFill;

    [Header("Cost")]
    [SerializeField]
    private Transform costPanel;

    [SerializeField]
    private IngredientIconView ingredientIconPrefab;

    [SerializeField]
    private IngredientIconDatabase ingredientIconDatabase;

    [Header("Player")]
    [SerializeField]
    private PlayerState player;

    [SerializeField]
    private GameObject unavailableOverlay;

    [SerializeField]
    private TMP_Text unavailableText;

    private SpellInstance spellInstance;

    public void Setup(SpellInstance instance)
    {
        spellInstance = instance;
        Refresh();
    }

    public void Refresh()
    {
        if (spellInstance == null)
            return;

        SpellDefinition definition = spellInstance.definition;

        spellNameText.text = definition.spellName;
        levelText.text = $"Nv. {spellInstance.level}";
        descriptionText.text = definition.GetDescription(spellInstance.level);

        int requirement = spellInstance.GetNextMasteryRequirement();

        masteryText.text =
            spellInstance.level >= 3
                ? "Maestría máxima"
                : $"Maestría {spellInstance.mastery} / {requirement}";

        if (spellInstance.level >= 3)
        {
            masteryFill.fillAmount = 1f;
        }
        else
        {
            masteryFill.fillAmount = (float)spellInstance.mastery / requirement;
        }

        if (definition.artwork != null)
        {
            artwork.sprite = definition.artwork;
        }

        RefreshCost();
        UpdateAvailability();
    }

    private void RefreshCost()
    {
        foreach (Transform child in costPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (IngredientCost ingredientCost in spellInstance.definition.cost)
        {
            for (int i = 0; i < ingredientCost.amount; i++)
            {
                IngredientIconView icon = Instantiate(ingredientIconPrefab, costPanel);

                Sprite sprite = ingredientIconDatabase.GetSprite(ingredientCost.type);

                icon.Setup(sprite);
            }
        }
    }

    private void UpdateAvailability()
    {
        if (player == null || spellInstance == null)
            return;

        bool canAfford = player.inventory.CanAfford(spellInstance.definition);

        bool alreadyUsed = spellInstance.usedThisTurn;

        bool available = canAfford && !alreadyUsed;

        // La página siempre permanece opaca
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;

        // Atenuamos solo el artwork
        if (artwork != null)
        {
            Color artworkColor = artwork.color;

            artworkColor.a = available ? 1f : 0.35f;

            artwork.color = artworkColor;
        }

        // Mostramos u ocultamos el overlay
        if (unavailableOverlay != null)
        {
            unavailableOverlay.SetActive(!available);
        }

        // Texto según el motivo
        if (unavailableText != null && !available)
        {
            unavailableText.text = alreadyUsed ? "Usado este turno" : "Ingredientes insuficientes";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TryCastSpell();
    }

    private void TryCastSpell()
    {
        if (spellInstance == null || player == null)
            return;

        // Ya se ha utilizado este turno
        if (spellInstance.usedThisTurn)
        {
            Debug.Log($"{spellInstance.definition.spellName} ya ha sido usado este turno.");

            return;
        }

        // No tenemos ingredientes suficientes
        if (!player.inventory.CanAfford(spellInstance.definition))
        {
            Debug.Log(
                $"No tienes ingredientes suficientes para lanzar {spellInstance.definition.spellName}."
            );

            return;
        }

        // Gastamos los ingredientes
        bool spent = player.inventory.Spend(spellInstance.definition);

        if (!spent)
            return;

        // Marcamos el hechizo como usado
        spellInstance.usedThisTurn = true;

        // Aumentamos la maestría
        spellInstance.AddMastery();

        Debug.Log(
            $"Has lanzado {spellInstance.definition.spellName}. "
                + $"Maestría actual: {spellInstance.mastery}. Nivel: {spellInstance.level}."
        );

        DebugInventory();

        // Refrescamos la carta
        Refresh();
    }

    private void DebugInventory()
    {
        Debug.Log(
            $"Ingredientes restantes → "
                + $"Hierba: {player.inventory.GetAmount(IngredientType.RedHerb)} | "
                + $"Agua: {player.inventory.GetAmount(IngredientType.PureWater)} | "
                + $"Azufre: {player.inventory.GetAmount(IngredientType.SulfurMineral)} | "
                + $"Cristal: {player.inventory.GetAmount(IngredientType.AirCrystal)} | "
                + $"Hueso: {player.inventory.GetAmount(IngredientType.BoneDust)}"
        );
    }

    public SpellInstance GetSpellInstance()
    {
        return spellInstance;
    }

    public void SetPlayer(PlayerState newPlayer)
    {
        player = newPlayer;
    }
}
