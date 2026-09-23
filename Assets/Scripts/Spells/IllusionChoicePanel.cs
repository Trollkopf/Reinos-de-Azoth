using UnityEngine;

public class IllusionChoicePanel : MonoBehaviour
{
    public void SelectIngredient(IngredientType ingredient, IllusionOptionView optionView)
    {
        Debug.Log($"Ingrediente seleccionado: {ingredient}");
    }
}
