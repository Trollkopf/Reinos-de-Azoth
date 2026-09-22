using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField]
    private PlayerState player;

    [SerializeField]
    private SpellPageView spellCard;

    private int turnNumber = 1;

    public void EndTurn()
    {
        Debug.Log($"Fin del turno {turnNumber}");

        ResetPlayerSpells();

        turnNumber++;

        Debug.Log($"Comienza el turno {turnNumber}");

        spellCard.Refresh();
    }

    private void ResetPlayerSpells()
    {
        SpellInstance spell = spellCard.GetSpellInstance();

        if (spell != null)
        {
            spell.usedThisTurn = false;
        }
    }
}
