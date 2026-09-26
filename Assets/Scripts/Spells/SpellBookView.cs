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
    private SpellDetailsPanel spellDetailsPanel;

    [Header("References")]
    [SerializeField]
    private IngredientInventoryView inventoryView;

    [SerializeField]
    private CreatureSelectionManager creatureSelectionManager;

    [SerializeField]
    private SpellResolver spellResolver;

    [SerializeField]
    private PlayerTargetSelectionManager playerTargetSelectionManager;

    private List<SpellInstance> spells;
    private int currentSpreadIndex = 0;

    private bool masteryRewardMode = false;

    private PlayerState masteryRewardPlayer = null;

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
        {
            return;
        }

        int leftIndex = currentSpreadIndex * 2;

        int rightIndex = leftIndex + 1;

        SetupPage(leftPage, leftIndex);

        SetupPage(rightPage, rightIndex);

        int totalSpreads = Mathf.CeilToInt(spells.Count / 2f);

        pageIndicator.text = $"Página {currentSpreadIndex + 1} / {totalSpreads}";

        previousButton.interactable = currentSpreadIndex > 0;

        nextButton.interactable = currentSpreadIndex < totalSpreads - 1;
    }

    private void SetupPage(SpellPageView page, int spellIndex)
    {
        if (spellIndex >= spells.Count)
        {
            page.gameObject.SetActive(false);

            return;
        }

        page.gameObject.SetActive(true);

        page.SetPlayer(player);

        page.SetInventoryView(inventoryView);

        page.SetCreatureSelectionManager(creatureSelectionManager);

        page.SetSpellResolver(spellResolver);

        page.SetPlayerTargetSelectionManager(playerTargetSelectionManager);

        page.Setup(spells[spellIndex]);

        page.SetMasteryRewardMode(masteryRewardMode, HandleMasteryRewardSelection);

        page.SetSpellDetailsPanel(spellDetailsPanel);
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

    public void BeginMasteryReward(PlayerState rewardPlayer)
    {
        if (rewardPlayer == null || rewardPlayer != player)
        {
            return;
        }

        masteryRewardMode = true;
        masteryRewardPlayer = rewardPlayer;

        Debug.Log(
            $"{rewardPlayer.gameObject.name} puede elegir "
                + "un hechizo para recibir +1 de Maestría."
        );

        RefreshBook();
    }

    private void HandleMasteryRewardSelection(SpellInstance selectedSpell)
    {
        if (
            !masteryRewardMode
            || masteryRewardPlayer == null
            || selectedSpell == null
            || selectedSpell.definition == null
        )
        {
            return;
        }

        int previousLevel = selectedSpell.level;

        int previousMastery = selectedSpell.mastery;

        selectedSpell.AddMastery();

        Debug.Log(
            $"{selectedSpell.definition.spellName} recibe "
                + $"+1 de Maestría por derrotar al Señor Espectral. "
                + $"{previousMastery} → {selectedSpell.mastery}."
        );

        if (selectedSpell.level > previousLevel)
        {
            Debug.Log(
                $"¡{selectedSpell.definition.spellName} " + $"sube a Nivel {selectedSpell.level}!"
            );
        }

        masteryRewardMode = false;
        masteryRewardPlayer = null;

        RefreshBook();
    }

    public bool IsPlayer(PlayerState target)
    {
        return player == target;
    }
}
