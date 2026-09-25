using System;

[Serializable]
public class BurnStack
{
    public int turnsRemaining = 2;

    public PlayerState source;

    public BurnStack(PlayerState source)
    {
        this.source = source;
        turnsRemaining = 2;
    }
}
