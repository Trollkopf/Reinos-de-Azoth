using UnityEngine;

[CreateAssetMenu(fileName = "NewCreature", menuName = "Reinos de Azoth/Creature Definition")]
public class CreatureDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string creatureName;
    public Sprite artwork;

    [Header("Stats")]
    public CreatureRank rank;
    public int maxHP;
    public int attack;

    [Header("Rewards")]
    public int coinReward;
    public int arcanePowerReward;

    [Header("Ability")]
    public CreatureAbility ability = CreatureAbility.None;

    [TextArea(2, 4)]
    public string abilityDescription;
}
