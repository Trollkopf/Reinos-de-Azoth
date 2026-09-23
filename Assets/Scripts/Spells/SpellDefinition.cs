using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Reinos de Azoth/Spell Definition")]
public class SpellDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string spellName;
    public Sprite artwork;

    [Header("Cost")]
    public List<IngredientCost> cost;

    [Header("Descriptions")]
    [TextArea(2, 5)]
    public string level1Description;

    [TextArea(2, 5)]
    public string level2Description;

    [TextArea(2, 5)]
    public string level3Description;

    [Header("Effect")]
    public SpellEffectType effectType;

    public int level1Value;
    public int level2Value;
    public int level3Value;

    [Header("Secondary Effect")]
    public int level1SecondaryValue;
    public int level2SecondaryValue;
    public int level3SecondaryValue;

    [Header("Shield Interaction")]
    public ShieldPiercingType level1ShieldPiercing;
    public ShieldPiercingType level2ShieldPiercing;
    public ShieldPiercingType level3ShieldPiercing;

    public int GetEffectValue(int level)
    {
        return level switch
        {
            1 => level1Value,
            2 => level2Value,
            3 => level3Value,
            _ => level1Value,
        };
    }

    public string GetDescription(int level)
    {
        return level switch
        {
            1 => level1Description,
            2 => level2Description,
            3 => level3Description,
            _ => level1Description,
        };
    }

    public ShieldPiercingType GetShieldPiercing(int level)
    {
        return level switch
        {
            1 => level1ShieldPiercing,
            2 => level2ShieldPiercing,
            3 => level3ShieldPiercing,
            _ => ShieldPiercingType.None,
        };
    }

    public int GetSecondaryEffectValue(int level)
    {
        return level switch
        {
            1 => level1SecondaryValue,
            2 => level2SecondaryValue,
            3 => level3SecondaryValue,
            _ => level1SecondaryValue,
        };
    }
}
