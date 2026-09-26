using System.Collections.Generic;
using UnityEngine;

public class SpellResolver : MonoBehaviour
{
    [SerializeField]
    private PlayerState player;

    [SerializeField]
    private PlayerStatusView playerStatusView;

    [SerializeField]
    private SelectedCreatureView selectedCreatureView;

    [SerializeField]
    private CreatureSelectionManager creatureSelectionManager;

    [SerializeField]
    private CreaturePanel creaturePanel;

    [SerializeField]
    private IngredientDeck ingredientDeck;

    [SerializeField]
    private IllusionChoicePanel illusionChoicePanel;

    [SerializeField]
    private PlayerTargetView botTargetView;

    [SerializeField]
    private PlayerManager playerManager;

    [SerializeField]
    private MarketView marketView;

    [SerializeField]
    private MarketPanelController marketPanelController;

    [SerializeField]
    private SpellBookView spellBookView;

    [SerializeField]
    private BotController botController;

    public void Resolve(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        if (caster == null)
        {
            Debug.LogError("SpellResolver: caster es NULL.");

            return;
        }

        if (spellInstance == null)
        {
            Debug.LogError("SpellResolver: spellInstance es NULL.");

            return;
        }

        SpellDefinition spell = spellInstance.definition;

        Debug.Log(
            $"SpellResolver ejecutando: {spell.spellName} | "
                + $"EffectType: {spell.effectType} | "
                + $"Nivel: {spellInstance.level} | "
                + $"Valor: {spell.GetEffectValue(spellInstance.level)}"
        );

        switch (spell.effectType)
        {
            case SpellEffectType.Damage:
                Debug.Log("Resolviendo DAMAGE");

                ResolveDamage(spellInstance, targetCreature, caster);

                break;

            case SpellEffectType.Heal:
                Debug.Log("Resolviendo HEAL");

                ResolveHeal(spellInstance, caster);

                break;

            case SpellEffectType.Shield:
                ResolveShield(spellInstance, caster);

                break;

            case SpellEffectType.Drain:
                ResolveDrain(spellInstance, targetCreature, caster);

                break;

            case SpellEffectType.WindWhip:
                ResolveWindWhip(spellInstance, targetCreature, caster);

                break;

            case SpellEffectType.Roots:
                ResolveRoots(spellInstance, targetCreature, caster);

                break;

            case SpellEffectType.AcidExplosion:
                ResolveAcidExplosion(spellInstance, targetCreature, caster);

                break;

            case SpellEffectType.Illusion:
                ResolveIllusion(spellInstance);

                break;

            default:
                Debug.LogWarning($"EffectType todavía no implementado: " + $"{spell.effectType}");

                break;
        }
    }

    public void ResolveAgainstPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        if (
            spellInstance == null
            || spellInstance.definition == null
            || targetPlayer == null
            || caster == null
        )
        {
            Debug.LogError(
                "SpellResolver: datos inválidos al resolver " + "hechizo contra jugador."
            );

            return;
        }

        SpellDefinition spell = spellInstance.definition;

        switch (spell.effectType)
        {
            case SpellEffectType.Damage:
                ResolveDamageToPlayer(spellInstance, targetPlayer, caster);

                break;

            case SpellEffectType.Drain:
                ResolveDrainAgainstPlayer(spellInstance, targetPlayer, caster);

                break;

            case SpellEffectType.WindWhip:
                ResolveWindWhipAgainstPlayer(spellInstance, targetPlayer, caster);

                break;

            case SpellEffectType.Roots:
                ResolveRootsAgainstPlayer(spellInstance, targetPlayer, caster);

                break;

            case SpellEffectType.AcidExplosion:
                ResolveAcidExplosionAgainstPlayers(spellInstance, caster);
                break;

            default:
                Debug.LogWarning($"{spell.spellName} todavía no está preparado para PvP.");

                break;
        }
    }

    private void ResolveDamage(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        CreateCreatureResolver().ResolveDamage(spellInstance, targetCreature, caster);
    }

    private void ResolveHeal(SpellInstance spellInstance, PlayerState caster)
    {
        CreatePlayerResolver().ResolveHeal(spellInstance, caster);
    }

    private void ResolveShield(SpellInstance spellInstance, PlayerState caster)
    {
        CreatePlayerResolver().ResolveShield(spellInstance, caster);
    }

    private void ResolveDrain(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        CreateCreatureResolver().ResolveDrain(spellInstance, targetCreature, caster);
    }

    private void ResolveWindWhip(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        CreateCreatureResolver().ResolveWindWhip(spellInstance, targetCreature, caster);
    }

    private void ResolveRoots(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        CreateCreatureResolver().ResolveRoots(spellInstance, targetCreature, caster);
    }

    private void ResolveAcidExplosion(
        SpellInstance spellInstance,
        CreatureView targetCreature,
        PlayerState caster
    )
    {
        CreateCreatureResolver().ResolveAcidExplosion(spellInstance, targetCreature, caster);
    }

    private void ResolveIllusion(SpellInstance spellInstance)
    {
        if (ingredientDeck == null)
        {
            Debug.LogError("SpellResolver: IngredientDeck no está asignado.");

            return;
        }

        if (illusionChoicePanel == null)
        {
            Debug.LogError("SpellResolver: IllusionChoicePanel no está asignado.");

            return;
        }

        int drawAmount = spellInstance.definition.GetEffectValue(spellInstance.level);

        int keepAmount = spellInstance.definition.GetSecondaryEffectValue(spellInstance.level);

        List<IngredientType> revealedIngredients = new List<IngredientType>();

        for (int i = 0; i < drawAmount; i++)
        {
            IngredientType ingredient = ingredientDeck.Draw();

            revealedIngredients.Add(ingredient);

            Debug.Log($"Ilusión ha revelado: {ingredient}");
        }

        illusionChoicePanel.ShowChoices(revealedIngredients, keepAmount);
    }

    private void ResolveDamageToPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        CreatePlayerResolver().ResolveDamageToPlayer(spellInstance, targetPlayer, caster);
    }

    private void ResolveDrainAgainstPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        CreatePlayerResolver().ResolveDrainAgainstPlayer(spellInstance, targetPlayer, caster);
    }

    private void ResolveWindWhipAgainstPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        CreatePlayerResolver().ResolveWindWhipAgainstPlayer(spellInstance, targetPlayer, caster);
    }

    private void ResolveRootsAgainstPlayer(
        SpellInstance spellInstance,
        PlayerState targetPlayer,
        PlayerState caster
    )
    {
        CreatePlayerResolver().ResolveRootsAgainstPlayer(spellInstance, targetPlayer, caster);
    }

    private CreatureSpellResolver CreateCreatureResolver()
    {
        return new CreatureSpellResolver(
            playerStatusView,
            selectedCreatureView,
            creatureSelectionManager,
            creaturePanel,
            ingredientDeck,
            marketView,
            marketPanelController,
            spellBookView,
            botController
        );
    }

    private PlayerSpellResolver CreatePlayerResolver()
    {
        return new PlayerSpellResolver(playerStatusView, botTargetView, ingredientDeck);
    }

    private void ResolveAcidExplosionAgainstPlayers(SpellInstance spellInstance, PlayerState caster)
    {
        if (playerManager == null)
        {
            Debug.LogError("SpellResolver: PlayerManager no está asignado.");

            return;
        }

        List<PlayerState> opponents = playerManager.GetOpponents(caster);

        foreach (PlayerState opponent in opponents)
        {
            if (opponent == null)
                continue;

            CreatePlayerResolver()
                .ResolveAcidExplosionAgainstPlayer(spellInstance, opponent, caster);
        }
    }
}
