using System.Collections.Generic;
using UnityEngine;

public class CreaturePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CreatureDeck creatureDeck;

    [SerializeField]
    private CreatureView creaturePrefab;

    [SerializeField]
    private Transform creatureContainer;

    [SerializeField]
    private CreatureSelectionManager selectionManager;

    private readonly List<CreatureInstance> activeCreatures = new List<CreatureInstance>();

    private readonly List<CreatureView> creatureViews = new List<CreatureView>();

    private const int VisibleCreatureCount = 3;

    private void Awake()
    {
        InitializeCreatures();
    }

    private void InitializeCreatures()
    {
        for (int i = 0; i < VisibleCreatureCount; i++)
        {
            CreatureDefinition definition = creatureDeck.Draw();

            if (definition == null)
                continue;

            CreatureInstance instance = new CreatureInstance(definition);

            activeCreatures.Add(instance);

            CreatureView view = Instantiate(creaturePrefab, creatureContainer);

            view.Setup(instance);

            view.SetSelectionManager(selectionManager);

            creatureViews.Add(view);
        }
    }

    public void ReplaceCreature(CreatureView creatureView)
    {
        int index = creatureViews.IndexOf(creatureView);

        if (index < 0)
            return;

        CreatureInstance oldCreature = activeCreatures[index];

        creatureDeck.Discard(oldCreature.definition);

        CreatureDefinition newDefinition = creatureDeck.Draw();

        if (newDefinition == null)
            return;

        CreatureInstance newInstance = new CreatureInstance(newDefinition);

        activeCreatures[index] = newInstance;

        creatureView.Setup(newInstance);

        creatureView.SetSelectionManager(selectionManager);
    }

    public List<CreatureView> GetActiveCreatureViews()
    {
        return creatureViews;
    }

    public void ClearEndOfTurnEffects()
    {
        for (int i = 0; i < activeCreatures.Count; i++)
        {
            CreatureInstance creature = activeCreatures[i];

            CreatureView creatureView = creatureViews[i];

            if (creature == null)
                continue;

            bool diedFromBurn = ResolveBurns(creature, creatureView);

            /*
             * Si ha muerto por quemadura,
             * ReplaceCreature ya ha colocado
             * una criatura nueva en este slot.
             */
            if (diedFromBurn)
            {
                continue;
            }

            if (creature.statusEffects != null)
            {
                creature.statusEffects.ClearEndOfTurnEffects();
            }

            /*
             * Temporal:
             * seguimos limpiando el booleano antiguo
             * hasta migrar completamente Corrosión
             * al nuevo sistema.
             */
            creature.isCorroded = false;

            if (creatureView != null)
            {
                creatureView.Refresh();
            }
        }

        Debug.Log("Los estados temporales de las criaturas han terminado.");
    }

    private bool ResolveBurns(CreatureInstance creature, CreatureView creatureView)
    {
        if (
            creature.statusEffects == null
            || creature.statusEffects.burns == null
            || creature.statusEffects.burns.Count == 0
        )
        {
            return false;
        }

        List<BurnStack> burns = creature.statusEffects.burns;

        List<BurnStack> expiredBurns = new List<BurnStack>();

        /*
         * Recorremos en orden de aplicación:
         * la primera quemadura aplicada
         * se resuelve primero.
         */
        for (int i = 0; i < burns.Count; i++)
        {
            BurnStack burn = burns[i];

            if (burn == null)
            {
                expiredBurns.Add(burn);

                continue;
            }

            creature.TakeDamage(1);

            burn.turnsRemaining--;

            string sourceName =
                burn.source != null ? burn.source.gameObject.name : "origen desconocido";

            Debug.Log(
                $"{creature.definition.creatureName} "
                    + $"recibe 1 de daño por Quemadura de "
                    + $"{sourceName}. "
                    + $"PV restantes: {creature.currentHP}. "
                    + $"Duración restante: {burn.turnsRemaining}."
            );

            if (creatureView != null)
            {
                creatureView.Refresh();
            }

            /*
             * Si esta quemadura mata a la criatura,
             * su source se lleva la recompensa.
             *
             * No procesamos las quemaduras posteriores.
             */
            if (creature.IsDead)
            {
                Debug.Log(
                    $"{creature.definition.creatureName} "
                        + $"ha muerto por la Quemadura de {sourceName}."
                );

                ResolveBurnDeath(creature, creatureView, burn.source);

                return true;
            }

            if (burn.turnsRemaining <= 0)
            {
                expiredBurns.Add(burn);
            }
        }

        /*
         * Eliminamos después de resolverlas todas
         * para no alterar el orden durante el recorrido.
         */
        foreach (BurnStack expiredBurn in expiredBurns)
        {
            burns.Remove(expiredBurn);
        }

        if (burns.Count > 0)
        {
            Debug.Log(
                $"{creature.definition.creatureName} mantiene "
                    + $"{burns.Count} acumulación(es) de Quemadura."
            );
        }

        return false;
    }

    private void ResolveBurnDeath(
        CreatureInstance creature,
        CreatureView creatureView,
        PlayerState killer
    )
    {
        CreatureDefinition definition = creature.definition;

        if (killer != null)
        {
            killer.AddCoins(definition.coinReward);

            killer.AddArcanePower(definition.arcanePowerReward);

            Debug.Log(
                $"{killer.gameObject.name} recibe "
                    + $"+{definition.coinReward} monedas y "
                    + $"+{definition.arcanePowerReward} Poder Arcano "
                    + $"por derrotar a {definition.creatureName} "
                    + "con Quemadura."
            );
        }

        if (selectionManager != null)
        {
            selectionManager.ClearSelection();
        }

        ReplaceCreature(creatureView);
    }

    public void ResolveStartOfRoundEffects()
    {
        for (int i = 0; i < activeCreatures.Count; i++)
        {
            CreatureInstance creature = activeCreatures[i];

            CreatureView creatureView = creatureViews[i];

            if (creature == null || creature.IsDead || creature.definition == null)
            {
                continue;
            }

            if (creature.definition.ability != CreatureAbility.Regeneration)
            {
                continue;
            }

            int previousHP = creature.currentHP;

            creature.currentHP = Mathf.Min(creature.definition.maxHP, creature.currentHP + 1);

            int healed = creature.currentHP - previousHP;

            if (healed > 0)
            {
                Debug.Log(
                    $"{creature.definition.creatureName} "
                        + $"regenera {healed} PV. "
                        + $"PV actuales: {creature.currentHP}/"
                        + $"{creature.definition.maxHP}."
                );

                if (creatureView != null)
                {
                    creatureView.Refresh();
                }
            }
        }
    }
}
