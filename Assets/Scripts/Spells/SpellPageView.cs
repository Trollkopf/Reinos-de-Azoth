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

    [SerializeField]
    private IngredientInventoryView inventoryView;

    [Header("Combat")]
    [SerializeField]
    private CreatureSelectionManager creatureSelectionManager;

    [SerializeField]
    private PlayerStatusView playerStatusView;

    [SerializeField]
    private SelectedCreatureView selectedCreatureView;

    private PlayerTargetSelectionManager playerTargetSelectionManager;

    private SpellInstance spellInstance;

    private CreaturePanel creaturePanel;

    private SpellResolver spellResolver;

    public void Setup(SpellInstance instance)
    {
        spellInstance = instance;
        Refresh();
    }

    public void SetInventoryView(IngredientInventoryView newInventoryView)
    {
        inventoryView = newInventoryView;
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

        bool available = canAfford;

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
            unavailableText.text = "Ingredientes insuficientes";
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

        SpellDefinition definition = spellInstance.definition;

        // Comprobar ingredientes
        if (!player.inventory.CanAfford(definition))
        {
            Debug.Log($"No tienes ingredientes suficientes para lanzar {definition.spellName}.");

            return;
        }

        // Si el hechizo necesita objetivo criatura, comprobamos que exista.
        bool needsCreatureTarget =
            definition.effectType == SpellEffectType.Drain
            || definition.effectType == SpellEffectType.WindWhip
            || definition.effectType == SpellEffectType.Roots;

        bool canTargetPlayer =
            definition.effectType == SpellEffectType.Damage
            || definition.effectType == SpellEffectType.Drain;

        bool hasCreatureTarget =
            creatureSelectionManager != null && creatureSelectionManager.SelectedCreature != null;

        bool hasPlayerTarget =
            playerTargetSelectionManager != null
            && playerTargetSelectionManager.SelectedPlayer != null;

        if (needsCreatureTarget && !hasCreatureTarget)
        {
            Debug.Log($"Selecciona una criatura antes de lanzar {definition.spellName}.");

            return;
        }

        if (canTargetPlayer && !hasCreatureTarget && !hasPlayerTarget)
        {
            Debug.Log(
                $"Selecciona una criatura o un jugador antes de lanzar {definition.spellName}."
            );

            return;
        }

        // Gastar ingredientes
        bool spent = player.inventory.Spend(definition);

        if (!spent)
            return;

        // Resolver el hechizo
        CreatureView targetCreature =
            creatureSelectionManager != null ? creatureSelectionManager.SelectedCreature : null;

        PlayerState targetPlayer =
            playerTargetSelectionManager != null
                ? playerTargetSelectionManager.SelectedPlayer
                : null;

        canTargetPlayer =
            definition.effectType == SpellEffectType.Damage
            || definition.effectType == SpellEffectType.Drain;

        if (canTargetPlayer && targetPlayer != null)
        {
            spellResolver.ResolveAgainstPlayer(spellInstance, targetPlayer, player);
        }
        else
        {
            spellResolver.Resolve(spellInstance, targetCreature, player);
        }

        // Aumentar maestría
        spellInstance.AddMastery();

        Debug.Log(
            $"Has lanzado {definition.spellName}. "
                + $"Maestría actual: {spellInstance.mastery}. "
                + $"Nivel: {spellInstance.level}."
        );

        DebugInventory();

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

    public void SetCreatureSelectionManager(CreatureSelectionManager manager)
    {
        creatureSelectionManager = manager;
    }

    public void SetPlayerStatusView(PlayerStatusView statusView)
    {
        playerStatusView = statusView;
    }

    public void SetSelectedCreatureView(SelectedCreatureView creatureView)
    {
        selectedCreatureView = creatureView;
    }

    private void AttackSelectedCreature(SpellDefinition spell)
    {
        CreatureView creatureView = creatureSelectionManager.SelectedCreature;

        if (creatureView == null)
            return;

        CreatureInstance creature = creatureView.GetCreatureInstance();

        if (creature == null || creature.IsDead)
            return;

        int damage = spell.GetEffectValue(spellInstance.level);

        creature.TakeDamage(damage);

        Debug.Log(
            $"{spell.spellName} hace {damage} de daño a " + $"{creature.definition.creatureName}."
        );

        creatureView.Refresh();

        if (selectedCreatureView != null)
        {
            selectedCreatureView.Refresh();
        }

        // Si ha muerto, no contraataca.
        if (creature.IsDead)
        {
            ResolveCreatureDeath(creatureView, creature);

            return;
        }

        CreatureCounterAttack(creature);
    }

    private void CreatureCounterAttack(CreatureInstance creature)
    {
        int damage = creature.definition.attack;

        player.currentHP -= damage;

        if (player.currentHP < 0)
        {
            player.currentHP = 0;
        }

        Debug.Log(
            $"{creature.definition.creatureName} contraataca " + $"y causa {damage} de daño."
        );

        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }
    }

    public void SetCreaturePanel(CreaturePanel newCreaturePanel)
    {
        creaturePanel = newCreaturePanel;
    }

    private void ResolveCreatureDeath(CreatureView creatureView, CreatureInstance creature)
    {
        CreatureDefinition definition = creature.definition;

        Debug.Log($"{definition.creatureName} ha sido derrotado.");

        // Recompensas
        player.coins += definition.coinReward;
        player.arcanePower += definition.arcanePowerReward;

        Debug.Log(
            $"Recompensa: +{definition.coinReward} monedas, "
                + $"+{definition.arcanePowerReward} Poder Arcano."
        );

        // Refrescar estado del jugador
        if (playerStatusView != null)
        {
            playerStatusView.Refresh();
        }

        // Limpiar selección
        if (creatureSelectionManager != null)
        {
            creatureSelectionManager.ClearSelection();
        }

        // Reemplazar la criatura por una nueva
        if (creaturePanel != null)
        {
            creaturePanel.ReplaceCreature(creatureView);
        }
    }

    public void SetSpellResolver(SpellResolver resolver)
    {
        spellResolver = resolver;
    }

    public void SetPlayerTargetSelectionManager(PlayerTargetSelectionManager manager)
    {
        playerTargetSelectionManager = manager;
    }
}
