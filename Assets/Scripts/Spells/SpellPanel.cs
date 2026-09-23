using System.Collections.Generic;
using UnityEngine;

public class SpellPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpellPageView spellCardPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private PlayerSpellBook spellBook;
    [SerializeField] private PlayerState player;

    private readonly List<SpellPageView> cards =
        new List<SpellPageView>();

    private void Start()
    {
        GenerateCards();
    }

    private void GenerateCards()
    {
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        cards.Clear();

        foreach (SpellInstance spell in spellBook.Spells)
        {
            SpellPageView card =
                Instantiate(spellCardPrefab, cardContainer);

            card.SetPlayer(player);
            card.Setup(spell);

            cards.Add(card);
        }
    }

    public void RefreshAll()
    {
        foreach (SpellPageView card in cards)
        {
            card.Refresh();
        }
    }
}