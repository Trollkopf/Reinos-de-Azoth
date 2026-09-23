[System.Serializable]
public class CreatureInstance
{
    public CreatureDefinition definition;
    public int currentHP;

    public bool isCorroded = false;

    public CreatureInstance(CreatureDefinition definition)
    {
        this.definition = definition;
        currentHP = definition.maxHP;
    }

    public bool IsDead => currentHP <= 0;

    public void TakeDamage(int amount)
    {
        currentHP -= amount;

        if (currentHP < 0)
        {
            currentHP = 0;
        }
    }
}
