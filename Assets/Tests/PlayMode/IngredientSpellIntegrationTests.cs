using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ReinosDeAzoth.Tests.PlayMode
{
    public class IngredientSpellIntegrationTests : SpellIntegrationFixture
    {
        [TestCase(1, 2, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(3, 3, 2)]
        public void Illusion_KeepsLevelSpecificCountAndDiscardsRemainingDuplicates(int level, int drawn, int kept)
        {
            // Duplicate cards expose accidental RemoveAll/discard bugs.
            var cards = new List<IngredientType>
                { IngredientType.RedHerb, IngredientType.RedHerb, IngredientType.PureWater };
            SetField(ingredients, "deck", new List<IngredientType>(cards));
            var choice = Component<IllusionChoicePanel>("Illusion choices", false);
            var options = Component<CanvasGroup>("Options", false, choice.transform);
            var prefab = Component<IllusionOptionView>("Option prefab", false);
            SetField(choice, "player", caster);
            SetField(choice, "optionsContainer", options.transform);
            SetField(choice, "optionPrefab", prefab);
            SetField(choice, "ingredientIconDatabase", Asset<IngredientIconDatabase>());
            SetField(choice, "ingredientDeck", ingredients);
            SetField(resolver, "illusionChoicePanel", choice);
            caster.inventory.Add(IngredientType.BoneDust, PlayerState.MaxHandSize);

            resolver.Resolve(Spell("Illusion", level), null, caster);

            Assert.That(ingredients.GetDeckCount(), Is.EqualTo(3 - drawn));
            Assert.That(options.transform.childCount, Is.EqualTo(drawn));
            Assert.That(caster.inventory.GetTotalCount(), Is.EqualTo(PlayerState.MaxHandSize));
            Assert.That(ingredients.GetDiscardCount(), Is.Zero);
            var views = options.GetComponentsInChildren<IllusionOptionView>(true);
            for (int i = 0; i < kept; i++) views[i].Choose();
            Assert.That(caster.inventory.GetAmount(IngredientType.RedHerb), Is.EqualTo(kept));
            Assert.That(caster.inventory.GetTotalCount(), Is.EqualTo(PlayerState.MaxHandSize + kept));
            Assert.That(ingredients.GetDiscardCount(), Is.EqualTo(drawn - kept));
            CollectionAssert.AreEqual(cards.GetRange(kept, drawn - kept),
                Field<List<IngredientType>>(ingredients, "discardPile"));
            Assert.That(choice.gameObject.activeSelf, Is.False);
            // A late queued click cannot keep another card after reaching the limit.
            views[drawn - 1].Choose();
            Assert.That(caster.inventory.GetTotalCount(), Is.EqualTo(PlayerState.MaxHandSize + kept));
        }

        [UnityTest]
        public IEnumerator Illusion_ClosesAndDestroysOptionsAfterSelection()
        {
            var choice = Component<IllusionChoicePanel>("Choices", false);
            var options = Component<CanvasGroup>("Options", false, choice.transform);
            SetField(choice, "player", caster);
            SetField(choice, "optionsContainer", options.transform);
            SetField(choice, "optionPrefab", Component<IllusionOptionView>("Option prefab", false));
            SetField(choice, "ingredientIconDatabase", Asset<IngredientIconDatabase>());
            SetField(choice, "ingredientDeck", ingredients);
            choice.ShowChoices(new List<IngredientType>
                { IngredientType.RedHerb, IngredientType.PureWater }, 1);
            options.GetComponentsInChildren<IllusionOptionView>(true)[0].Choose();
            yield return null;
            Assert.That(options.transform.childCount, Is.Zero);
            Assert.That(choice.gameObject.activeSelf, Is.False);
            Assert.That(ingredients.GetDiscardCount(), Is.EqualTo(1));
        }

        [Test]
        public void ActualSpellCast_SendsEverySpentIngredientToDiscard()
        {
            var spell = Spell("Healing", 1);
            Fund(caster, spell);
            caster.inventory.Add(IngredientType.BoneDust, 2);
            caster.currentHP = 5;
            var page = Page(spell, caster);
            var expected = new List<IngredientType>();
            foreach (var cost in spell.definition.cost)
                for (int i = 0; i < cost.amount; i++) expected.Add(cost.type);

            page.OnPointerClick(null);

            CollectionAssert.AreEquivalent(expected, Field<List<IngredientType>>(ingredients, "discardPile"));
            foreach (IngredientType type in Enum.GetValues(typeof(IngredientType)))
                Assert.That(caster.inventory.GetAmount(type),
                    Is.EqualTo(type == IngredientType.BoneDust ? 2 : 0));
            Assert.That(caster.currentHP, Is.EqualTo(7));
            Assert.That(caster.SpellsCastThisTurn, Is.EqualTo(1));
            Assert.That(spell.mastery, Is.EqualTo(1));
            page.OnPointerClick(null); // Insufficient ingredients: no second cast or discard.
            Assert.That(ingredients.GetDiscardCount(), Is.EqualTo(expected.Count));
            Assert.That(caster.SpellsCastThisTurn, Is.EqualTo(1));
            Assert.That(caster.currentHP, Is.EqualTo(7));
        }

        [Test]
        public void SpendIngredients_AggregatesDuplicatesAndFailureLeavesDiscardUntouched()
        {
            var definition = Asset<SpellDefinition>();
            definition.cost = new List<IngredientCost>
            {
                new IngredientCost { type = IngredientType.RedHerb, amount = 2 },
                new IngredientCost { type = IngredientType.RedHerb, amount = 1 },
                new IngredientCost { type = IngredientType.PureWater, amount = 1 }
            };
            caster.inventory.Add(IngredientType.RedHerb, 3);
            Assert.That(caster.SpendIngredientsForSpell(definition), Is.False);
            Assert.That(caster.inventory.GetAmount(IngredientType.RedHerb), Is.EqualTo(3));
            Assert.That(ingredients.GetDiscardCount(), Is.Zero);
            caster.inventory.Add(IngredientType.PureWater);
            Assert.That(caster.SpendIngredientsForSpell(definition), Is.True);
            CollectionAssert.AreEquivalent(new[] { IngredientType.RedHerb, IngredientType.RedHerb,
                IngredientType.RedHerb, IngredientType.PureWater },
                Field<List<IngredientType>>(ingredients, "discardPile"));
            Assert.That(caster.inventory.GetTotalCount(), Is.Zero);
            Assert.That(caster.SpendIngredientsForSpell(null), Is.False);
            Assert.That(ingredients.GetDiscardCount(), Is.EqualTo(4));
        }
    }
}
