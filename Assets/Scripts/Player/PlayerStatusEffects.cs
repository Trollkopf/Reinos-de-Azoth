using System;
using System.Collections.Generic;

[Serializable]
public class PlayerStatusEffects
{
    public int spellLimitNextTurn = -1;
    public int spellLimitThisTurn = -1;

    public List<BurnStack> burns = new List<BurnStack>();

    public bool corroded = false;

    public bool reflectNextDamage = false;

    public void BeginTurn()
    {
        spellLimitThisTurn = spellLimitNextTurn;

        spellLimitNextTurn = -1;
    }

    public void AddBurn(PlayerState source)
    {
        burns.Add(new BurnStack(source));
    }

    public bool HasBurns()
    {
        return burns != null && burns.Count > 0;
    }

    public int GetBurnCount()
    {
        return burns?.Count ?? 0;
    }

    public bool ConsumeReflection()
    {
        if (!reflectNextDamage)
        {
            return false;
        }

        reflectNextDamage = false;

        return true;
    }

    public void ClearNegativeEffects()
    {
        burns.Clear();

        spellLimitNextTurn = -1;
        spellLimitThisTurn = -1;

        corroded = false;
    }

    public void ClearEndOfTurnEffects()
    {
        spellLimitThisTurn = -1;

        corroded = false;

        reflectNextDamage = false;
    }
}
