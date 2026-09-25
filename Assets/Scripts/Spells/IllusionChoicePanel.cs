using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IllusionChoicePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerState player;

    [SerializeField]
    private Transform optionsContainer;

    [SerializeField]
    private IllusionOptionView optionPrefab;

    [SerializeField]
    private IngredientIconDatabase ingredientIconDatabase;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private IngredientDeck ingredientDeck;

    private int maxSelections;
    private int currentSelections;

    private readonly List<IngredientType> revealedIngredients = new List<IngredientType>();

    public void ShowChoices(List<IngredientType> ingredients, int keepAmount)
    {
        revealedIngredients.Clear();
        revealedIngredients.AddRange(ingredients);

        maxSelections = keepAmount;

        currentSelections = 0;

        ClearOptions();

        gameObject.SetActive(true);

        if (titleText != null)
        {
            titleText.text = $"Elige {keepAmount} ingrediente(s) para conservar";
        }

        foreach (IngredientType ingredient in revealedIngredients)
        {
            IllusionOptionView option = Instantiate(optionPrefab, optionsContainer);

            Sprite sprite = ingredientIconDatabase.GetSprite(ingredient);

            option.Setup(ingredient, sprite, this);
        }
    }

    public void SelectIngredient(IngredientType ingredient, IllusionOptionView optionView)
    {
        if (currentSelections >= maxSelections)
        {
            return;
        }

        player.inventory.Add(ingredient, 1);

        /*
         * Quitamos SOLO una aparición.
         * Esto es importante si Ilusión
         * revela dos ingredientes iguales.
         */
        revealedIngredients.Remove(ingredient);

        currentSelections++;

        Destroy(optionView.gameObject);

        Debug.Log($"Ilusión: conservas {ingredient}. " + $"{currentSelections}/{maxSelections}");

        if (currentSelections >= maxSelections)
        {
            DiscardRemainingIngredients();

            ClosePanel();
        }
    }

    private void DiscardRemainingIngredients()
    {
        if (revealedIngredients.Count == 0)
        {
            return;
        }

        if (ingredientDeck == null)
        {
            Debug.LogWarning(
                "IllusionChoicePanel: IngredientDeck no está asignado. "
                    + "Los ingredientes no elegidos no han podido ir al descarte."
            );

            return;
        }

        foreach (IngredientType ingredient in revealedIngredients)
        {
            ingredientDeck.Discard(ingredient);

            Debug.Log($"Ilusión: {ingredient} va a la pila de descarte.");
        }

        revealedIngredients.Clear();

        Debug.Log($"Pila de descartes: {ingredientDeck.GetDiscardCount()}.");
    }

    private void ClearOptions()
    {
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClosePanel()
    {
        ClearOptions();

        gameObject.SetActive(false);
    }
}
