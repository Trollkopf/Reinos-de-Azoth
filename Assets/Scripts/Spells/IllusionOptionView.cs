using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IllusionOptionView : MonoBehaviour
{
    [SerializeField]
    public Image icon;

    [SerializeField]
    public TMP_Text nameText;

    private IngredientType ingredient;
    public IllusionChoicePanel choicePanel;

    public void Setup(IngredientType newIngredient, Sprite sprite, IllusionChoicePanel panel)
    {
        ingredient = newIngredient;
        choicePanel = panel;

        if (icon != null)
        {
            icon.sprite = sprite;
        }

        if (nameText != null)
        {
            nameText.text = GetIngredientName(ingredient);
        }
    }

    public void Choose()
    {
        if (choicePanel != null)
        {
            choicePanel.SelectIngredient(ingredient, this);
        }
    }

    private string GetIngredientName(IngredientType type)
    {
        return type switch
        {
            IngredientType.RedHerb => "Hierba Roja",

            IngredientType.PureWater => "Agua Pura",

            IngredientType.SulfurMineral => "Mineral Sulfuroso",

            IngredientType.AirCrystal => "Cristal de Aire",

            IngredientType.BoneDust => "Polvo de Hueso",

            _ => type.ToString(),
        };
    }
}
