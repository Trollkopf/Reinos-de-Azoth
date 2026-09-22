using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(
    fileName = "NewSpell",
    menuName = "Reinos de Azoth/Spell Definition"
)]
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

    public string GetDescription(int level)
    {
        return level switch
        {
            1 => level1Description,
            2 => level2Description,
            3 => level3Description,
            _ => level1Description
        };
    }
}