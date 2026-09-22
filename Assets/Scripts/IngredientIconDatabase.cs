using UnityEngine;

[CreateAssetMenu(
    fileName = "IngredientIconDatabase",
    menuName = "Reinos de Azoth/Ingredient Icon Database"
)]
public class IngredientIconDatabase : ScriptableObject
{
    [Header("Ingredient Sprites")]
    public Sprite redHerb;
    public Sprite pureWater;
    public Sprite sulfurMineral;
    public Sprite airCrystal;
    public Sprite boneDust;

    public Sprite GetSprite(IngredientType type)
    {
        return type switch
        {
            IngredientType.RedHerb => redHerb,
            IngredientType.PureWater => pureWater,
            IngredientType.SulfurMineral => sulfurMineral,
            IngredientType.AirCrystal => airCrystal,
            IngredientType.BoneDust => boneDust,
            _ => null
        };
    }
}