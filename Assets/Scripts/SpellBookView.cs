using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellBookView : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerSpellBook spellBook;
    [SerializeField] private PlayerState player;

    [Header("Pages")]
    [SerializeField] private SpellPageView leftPage;
    [SerializeField] private SpellPageView rightPage;

    [Header("Navigation")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text pageIndicator;

    private List<SpellInstance> spells;
    private int currentSpreadIndex = 0;

    private void Start()
    {
        spells = spellBook.Spells;

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
            leftPage.Setup(spells[leftIndex]);
        }
        else
        {
            leftPage.gameObject.SetActive(false);
        }

        if (rightIndex < spells.Count)
        {
            rightPage.gameObject.SetActive(true);
            rightPage.SetPlayer(player);
            rightPage.Setup(spells[rightIndex]);
        }
        else
        {
            rightPage.gameObject.SetActive(false);
        }

        int totalSpreads = Mathf.CeilToInt(spells.Count / 2f);

        pageIndicator.text =
            $"Página {currentSpreadIndex + 1} / {totalSpreads}";

        previousButton.interactable =
            currentSpreadIndex > 0;

        nextButton.interactable =
            currentSpreadIndex < totalSpreads - 1;
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
}