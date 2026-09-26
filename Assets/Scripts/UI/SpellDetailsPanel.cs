using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellDetailsPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject panelRoot;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text level1Text;

    [SerializeField]
    private TMP_Text level2Text;

    [SerializeField]
    private TMP_Text level3Text;

    [SerializeField]
    private Transform costPanel;

    [SerializeField]
    private IngredientIconView ingredientIconPrefab;

    [SerializeField]
    private IngredientIconDatabase ingredientIconDatabase;

    [SerializeField]
    private Image artworkImage;

    public void Show(SpellDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = definition.spellName;
        }

        if (artworkImage != null)
        {
            if (definition.artwork != null)
            {
                artworkImage.sprite = definition.artwork;

                artworkImage.enabled = true;
            }
            else
            {
                artworkImage.sprite = null;

                artworkImage.enabled = false;
            }
        }

        RefreshCost(definition);

        if (level1Text != null)
        {
            level1Text.text = $"{definition.GetDescription(1)}";
        }

        if (level2Text != null)
        {
            level2Text.text = $"{definition.GetDescription(2)}";
        }

        if (level3Text != null)
        {
            level3Text.text = $"{definition.GetDescription(3)}";
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void RefreshCost(SpellDefinition definition)
    {
        if (costPanel == null || ingredientIconPrefab == null || ingredientIconDatabase == null)
        {
            return;
        }

        foreach (Transform child in costPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (IngredientCost ingredientCost in definition.cost)
        {
            for (int i = 0; i < ingredientCost.amount; i++)
            {
                IngredientIconView icon = Instantiate(ingredientIconPrefab, costPanel);

                Sprite sprite = ingredientIconDatabase.GetSprite(ingredientCost.type);

                icon.Setup(sprite);
            }
        }
    }
}
