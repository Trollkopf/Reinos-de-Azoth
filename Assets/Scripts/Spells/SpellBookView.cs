using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellBookView : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private PlayerSpellBook spellBook;

    [SerializeField]
    private PlayerState player;

    [Header("Pages")]
    [SerializeField]
    private SpellPageView leftPage;

    [SerializeField]
    private SpellPageView rightPage;

    [Header("Navigation")]
    [SerializeField]
    private Button previousButton;

    [SerializeField]
    private Button nextButton;

    [SerializeField]
    private TMP_Text pageIndicator;

    [SerializeField]
    private IngredientInventoryView inventoryView;

    [SerializeField]
    private CreatureSelectionManager creatureSelectionManager;

    [SerializeField]
    private PlayerStatusView playerStatusView;

    [SerializeField]
    private SelectedCreatureView selectedCreatureView;

    [SerializeField]
    private CreaturePanel creaturePanel;

    [SerializeField]
    private SpellResolver spellResolver;

    [SerializeField]
    private PlayerTargetSelectionManager playerTargetSelectionManager;

    private List<SpellInstance> spells;
    private int currentSpreadIndex = 0;

    private void Start()
    {
        spells = spellBook.Spells;

        player.inventory.OnChanged += RefreshBook;

        previousButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);

        RefreshBook();
    }

    public void RefreshBook()
    {
        if (spells == null || spells.Count == 0)
            return;

        int leftIndex = currentSpreadIndex * 2;
        int rightIndex = leftIndex + 1;

        if (leftIndex < spells.Count)
        {
            leftPage.gameObject.SetActive(true);
            leftPage.SetPlayer(player);
            leftPage.SetInventoryView(inventoryView);
            leftPage.Setup(spells[leftIndex]);
            leftPage.SetCreatureSelectionManager(creatureSelectionManager);
            leftPage.SetPlayerStatusView(playerStatusView);
            leftPage.SetSelectedCreatureView(selectedCreatureView);
            leftPage.SetCreaturePanel(creaturePanel);
            leftPage.SetSpellResolver(spellResolver);
            leftPage.SetPlayerTargetSelectionManager(playerTargetSelectionManager);
        }
        else
        {
            leftPage.gameObject.SetActive(false);
        }

        if (rightIndex < spells.Count)
        {
            rightPage.gameObject.SetActive(true);
            rightPage.SetPlayer(player);
            rightPage.SetInventoryView(inventoryView);
            rightPage.Setup(spells[rightIndex]);
            rightPage.SetCreatureSelectionManager(creatureSelectionManager);
            rightPage.SetPlayerStatusView(playerStatusView);
            rightPage.SetSelectedCreatureView(selectedCreatureView);
            rightPage.SetCreaturePanel(creaturePanel);
            rightPage.SetSpellResolver(spellResolver);
            rightPage.SetPlayerTargetSelectionManager(playerTargetSelectionManager);
        }
        else
        {
            rightPage.gameObject.SetActive(false);
        }

        int totalSpreads = Mathf.CeilToInt(spells.Count / 2f);

        pageIndicator.text = $"Página {currentSpreadIndex + 1} / {totalSpreads}";

        previousButton.interactable = currentSpreadIndex > 0;

        nextButton.interactable = currentSpreadIndex < totalSpreads - 1;
    }

    public void NextPage()
    {
        int totalSpreads = Mathf.CeilToInt(spells.Count / 2f);

        if (currentSpreadIndex < totalSpreads - 1)
        {
            currentSpreadIndex++;
            RefreshBook();
        }
    }

    public void PreviousPage()
    {
        if (currentSpreadIndex > 0)
        {
            currentSpreadIndex--;
            RefreshBook();
        }
    }

    private void OnDestroy()
    {
        if (player != null && player.inventory != null)
        {
            player.inventory.OnChanged -= RefreshBook;
        }
    }
}
