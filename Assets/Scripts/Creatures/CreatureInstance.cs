[System.Serializable]
public class CreatureInstance
{
    public CreatureDefinition definition;

    public int currentHP;

    // Sistema nuevo de estados temporales.
    public CreatureStatusEffects statusEffects;

    // Temporal:
    // mantenemos esto mientras migramos Corrosión
    // completamente al nuevo sistema.
    public bool isCorroded = false;

    public CreatureInstance(
        CreatureDefinition definition
    )
    {
        this.definition = definition;

        currentHP = definition.maxHP;

        statusEffects =
            new CreatureStatusEffects();
    }

    public bool IsDead =>
        currentHP <= 0;

    public void TakeDamage(
        int amount
    )
    {
        currentHP -= amount;

        if (currentHP < 0)
        {
            currentHP = 0;
        }
    }
}