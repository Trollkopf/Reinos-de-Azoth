using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ReinosDeAzoth.Tests.EditMode
{
    public class IngredientInventoryTests
    {
        private IngredientInventory inventory;
        private SpellDefinition spell;
        private int changes;

        [SetUp]
        public void SetUp()
        {
            inventory = new IngredientInventory();
            changes = 0;
            inventory.OnChanged += () => changes++;
            spell = ScriptableObject.CreateInstance<SpellDefinition>();
            spell.cost = new List<IngredientCost>
            {
                new IngredientCost { type = IngredientType.RedHerb, amount = 2 },
                new IngredientCost { type = IngredientType.RedHerb, amount = 1 },
                new IngredientCost { type = IngredientType.PureWater, amount = 1 }
            };
        }

        [TearDown] public void TearDown() => UnityEngine.Object.DestroyImmediate(spell);

        [Test]
        public void NewInventory_AllIngredientTypesStartEmpty()
        {
            foreach (IngredientType type in Enum.GetValues(typeof(IngredientType)))
                Assert.That(inventory.GetAmount(type), Is.Zero);
            Assert.That(inventory.GetTotalCount(), Is.Zero);
        }

        [Test]
        public void AddAndRemove_UpdateCountsAndNotifyOnlySuccessfulChanges()
        {
            inventory.Add(IngredientType.RedHerb, 3);
            inventory.Add(IngredientType.PureWater);
            Assert.That(inventory.GetTotalCount(), Is.EqualTo(4));
            Assert.That(inventory.Has(IngredientType.RedHerb, 3), Is.True);
            Assert.That(inventory.Has(IngredientType.RedHerb, 4), Is.False);
            Assert.That(inventory.Remove(IngredientType.RedHerb, 2), Is.True);
            Assert.That(inventory.Remove(IngredientType.RedHerb, 2), Is.False);
            Assert.That(inventory.GetAmount(IngredientType.RedHerb), Is.EqualTo(1));
            Assert.That(inventory.GetTotalCount(), Is.EqualTo(2));
            Assert.That(changes, Is.EqualTo(3));
        }

        [Test]
        public void CanAfford_AggregatesDuplicateCostsWithoutSpending()
        {
            inventory.Add(IngredientType.RedHerb, 2);
            inventory.Add(IngredientType.PureWater);
            Assert.That(inventory.CanAfford(spell), Is.False);
            inventory.Add(IngredientType.RedHerb);
            Assert.That(inventory.CanAfford(spell), Is.True);
            Assert.That(inventory.GetTotalCount(), Is.EqualTo(4));
            Assert.That(changes, Is.EqualTo(3));
        }

        [Test]
        public void Spend_RemovesCombinedCostAndPreservesOtherIngredients()
        {
            inventory.Add(IngredientType.RedHerb, 4);
            inventory.Add(IngredientType.PureWater);
            inventory.Add(IngredientType.BoneDust, 2);
            Assert.That(inventory.Spend(spell), Is.True);
            Assert.That(inventory.GetAmount(IngredientType.RedHerb), Is.EqualTo(1));
            Assert.That(inventory.GetAmount(IngredientType.PureWater), Is.Zero);
            Assert.That(inventory.GetAmount(IngredientType.BoneDust), Is.EqualTo(2));
            Assert.That(changes, Is.EqualTo(4));
        }

        [Test]
        public void Spend_InsufficientCostIsAtomicAndDoesNotNotify()
        {
            inventory.Add(IngredientType.RedHerb, 3);
            Assert.That(inventory.Spend(spell), Is.False);
            Assert.That(inventory.GetAmount(IngredientType.RedHerb), Is.EqualTo(3));
            Assert.That(inventory.GetAmount(IngredientType.PureWater), Is.Zero);
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void EmptyCost_IsAffordableAndDoesNotRemoveIngredients()
        {
            spell.cost.Clear();
            Assert.That(inventory.CanAfford(spell), Is.True);
            Assert.That(inventory.Spend(spell), Is.True);
            Assert.That(inventory.GetTotalCount(), Is.Zero);
        }
    }
}
