using System;

[Serializable]
public class SpellInstance
{
    public SpellDefinition definition;

    public int mastery = 0;
    public int level = 1;

    public SpellInstance(SpellDefinition definition)
    {
        this.definition = definition;
    }

    public void AddMastery(int amount = 1)
    {
        mastery += amount;

        if (mastery >= 6)
        {
            level = 3;
        }
        else if (mastery >= 3)
        {
            level = 2;
        }
    }

    public int GetNextMasteryRequirement()
    {
        return level switch
        {
            1 => 3,
            2 => 6,
            _ => 6
        };
    }
}