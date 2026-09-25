using System;
using System.Collections.Generic;

[Serializable]
public class CreatureStatusEffects
{
    public bool rootedUntilEndOfTurn = false;

    public bool corroded = false;

    public List<BurnStack> burns = new List<BurnStack>();

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

    public void ClearEndOfTurnEffects()
    {
        rootedUntilEndOfTurn = false;
        corroded = false;
    }
}
