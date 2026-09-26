using System;
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

    private PlayerTargetSelectionManager playerTargetSelectionManager;

    private SpellInstance spellInstance;

    private SpellResolver spellResolver;

    private SpellDetailsPanel spellDetailsPanel;

    private bool masteryRewardMode = false;

    private Action<SpellInstance> masteryRewardCallback;

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

        RefreshMastery();

        if (definition.artwork != null)
        {
            artwork.sprite = definition.artwork;
        }

        RefreshCost();

        UpdateAvailability();
    }

    private void RefreshMastery()
    {
        int requirement = spellInstance.GetNextMasteryRequirement();

        masteryText.text =
            spellInstance.level >= 3
                ? "Maestría máxima"
                : $"Maestría {spellInstance.mastery} / {requirement}";

        if (spellInstance.level >= 3)
        {
            masteryFill.fillAmount = 1f;

            return;
        }

        masteryFill.fillAmount = (float)spellInstance.mastery / requirement;
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
        {
            return;
        }

        bool canAfford = player.inventory.CanAfford(spellInstance.definition);

        bool canCast = player.CanCastSpell();

        bool available = canAfford && canCast;

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;

        if (artwork != null)
        {
            Color artworkColor = artwork.color;

            artworkColor.a = available ? 1f : 0.35f;

            artwork.color = artworkColor;
        }

        if (unavailableOverlay != null)
        {
            unavailableOverlay.SetActive(!available);
        }

        if (unavailableText != null && !available)
        {
            if (!canCast)
            {
                unavailableText.text = "Límite de hechizos alcanzado";
            }
            else
            {
                unavailableText.text = "Ingredientes insuficientes";
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (masteryRewardMode)
        {
            masteryRewardCallback?.Invoke(spellInstance);

            return;
        }

        TryCastSpell();
    }

    private void TryCastSpell()
    {
        if (spellInstance == null || player == null || spellResolver == null)
        {
            return;
        }

        SpellDefinition definition = spellInstance.definition;

        if (!player.CanCastSpell())
        {
            Debug.Log($"{player.gameObject.name} no puede lanzar más hechizos este turno.");

            Refresh();

            return;
        }

        if (!player.inventory.CanAfford(definition))
        {
            Debug.Log(
                $"No tienes ingredientes suficientes para lanzar " + $"{definition.spellName}."
            );

            return;
        }

        CreatureView targetCreature = GetSelectedCreature();

        PlayerState targetPlayer = GetSelectedPlayer();

        if (!HasValidTarget(definition, targetCreature, targetPlayer))
        {
            return;
        }

        bool spent = player.SpendIngredientsForSpell(definition);

        if (!spent)
            return;

        ResolveSpell(definition, targetCreature, targetPlayer);

        player.RegisterSpellCast();

        spellInstance.AddMastery();

        Debug.Log(
            $"Has lanzado {definition.spellName}. "
                + $"Maestría actual: {spellInstance.mastery}. "
                + $"Nivel: {spellInstance.level}."
        );

        DebugInventory();

        Refresh();
    }

    private CreatureView GetSelectedCreature()
    {
        if (creatureSelectionManager == null)
        {
            return null;
        }

        return creatureSelectionManager.SelectedCreature;
    }

    private PlayerState GetSelectedPlayer()
    {
        if (playerTargetSelectionManager == null)
        {
            return null;
        }

        return playerTargetSelectionManager.SelectedPlayer;
    }

    private bool HasValidTarget(
        SpellDefinition definition,
        CreatureView targetCreature,
        PlayerState targetPlayer
    )
    {
        // Explosión Ácida es especial:
        // sin jugador seleccionado afecta a todas las criaturas;
        // con jugador seleccionado entra en modo PvP
        // y afecta a todos los rivales.
        if (definition.effectType == SpellEffectType.AcidExplosion)
        {
            return true;
        }

        bool canTargetCreature = CanTargetCreature(definition.effectType);

        bool canTargetPlayer = CanTargetPlayer(definition.effectType);

        bool hasCreatureTarget = targetCreature != null;

        bool hasPlayerTarget = targetPlayer != null;

        if (canTargetCreature && canTargetPlayer && !hasCreatureTarget && !hasPlayerTarget)
        {
            Debug.Log(
                $"Selecciona una criatura o un jugador antes de lanzar "
                    + $"{definition.spellName}."
            );

            return false;
        }

        if (canTargetCreature && !canTargetPlayer && !hasCreatureTarget)
        {
            Debug.Log($"Selecciona una criatura antes de lanzar " + $"{definition.spellName}.");

            return false;
        }

        return true;
    }

    private bool CanTargetCreature(SpellEffectType effectType)
    {
        return effectType == SpellEffectType.Damage
            || effectType == SpellEffectType.Drain
            || effectType == SpellEffectType.WindWhip
            || effectType == SpellEffectType.Roots
            || effectType == SpellEffectType.AcidExplosion;
    }

    private bool CanTargetPlayer(SpellEffectType effectType)
    {
        return effectType == SpellEffectType.Damage
            || effectType == SpellEffectType.Drain
            || effectType == SpellEffectType.WindWhip
            || effectType == SpellEffectType.Roots
            || effectType == SpellEffectType.AcidExplosion;
    }

    private void ResolveSpell(
        SpellDefinition definition,
        CreatureView targetCreature,
        PlayerState targetPlayer
    )
    {
        bool usePlayerTarget = CanTargetPlayer(definition.effectType) && targetPlayer != null;

        if (usePlayerTarget)
        {
            spellResolver.ResolveAgainstPlayer(spellInstance, targetPlayer, player);

            return;
        }

        spellResolver.Resolve(spellInstance, targetCreature, player);
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

    public void SetSpellResolver(SpellResolver resolver)
    {
        spellResolver = resolver;
    }

    public void SetPlayerTargetSelectionManager(PlayerTargetSelectionManager manager)
    {
        playerTargetSelectionManager = manager;
    }

    public void ShowSpellDetails()
    {
        if (spellInstance == null || spellInstance.definition == null || spellDetailsPanel == null)
        {
            return;
        }

        spellDetailsPanel.Show(spellInstance.definition);
    }

    public void SetSpellDetailsPanel(SpellDetailsPanel panel)
    {
        spellDetailsPanel = panel;
    }

    public void SetMasteryRewardMode(bool active, Action<SpellInstance> callback)
    {
        masteryRewardMode = active;
        masteryRewardCallback = callback;
    }
}
