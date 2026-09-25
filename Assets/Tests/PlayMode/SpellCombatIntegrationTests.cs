using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ReinosDeAzoth.Tests.PlayMode
{
    public class SpellCombatIntegrationTests : SpellIntegrationFixture
    {
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 1)]
        public void Fireball_AppliesBurnOnlyAtLevelThree_ToCreatureAndPlayer(int level, int count)
        {
            var view = Creatures().GetActiveCreatureViews()[0];
            var spell = Spell("Fireball", level);
            resolver.Resolve(spell, view, caster);
            resolver.ResolveAgainstPlayer(spell, opponent, caster);
            var creature = view.GetCreatureInstance();
            Assert.That(
                creature.currentHP,
                Is.EqualTo(30 - spell.definition.GetEffectValue(level))
            );
            Assert.That(
                opponent.currentHP,
                Is.EqualTo(15 - spell.definition.GetEffectValue(level))
            );
            Assert.That(creature.statusEffects.GetBurnCount(), Is.EqualTo(count));
            Assert.That(opponent.statusEffects.GetBurnCount(), Is.EqualTo(count));
            if (count == 1)
            {
                Assert.That(creature.statusEffects.burns[0].source, Is.SameAs(caster));
                Assert.That(creature.statusEffects.burns[0].turnsRemaining, Is.EqualTo(2));
                Assert.That(opponent.statusEffects.burns[0].source, Is.SameAs(caster));
            }
        }

        [Test]
        public void Fireball_DoesNotApplyBurnToDeadTargets()
        {
            var view = Creatures().GetActiveCreatureViews()[0];
            var creature = view.GetCreatureInstance();
            creature.currentHP = 1;
            opponent.currentHP = 1;
            resolver.Resolve(Spell("Fireball", 3), view, caster);
            resolver.ResolveAgainstPlayer(Spell("Fireball", 3), opponent, caster);
            Assert.That(creature.IsDead, Is.True);
            Assert.That(creature.statusEffects.HasBurns(), Is.False);
            Assert.That(opponent.currentHP, Is.Zero);
            Assert.That(opponent.statusEffects.HasBurns(), Is.False);
        }

        [TestCase(1, 6, 15, 4)]
        [TestCase(2, 6, 14, 4)]
        [TestCase(3, 6, 12, 6)]
        [TestCase(1, 1, 14, 0)]
        [TestCase(2, 1, 13, 0)]
        [TestCase(3, 1, 12, 1)]
        [TestCase(1, 0, 13, 0)]
        [TestCase(2, 0, 12, 0)]
        [TestCase(3, 0, 12, 0)]
        public void Plasma_UsesEachLevelsShieldPiercing(
            int level,
            int shield,
            int hp,
            int remainingShield
        )
        {
            opponent.shield = shield;
            resolver.ResolveAgainstPlayer(Spell("PlasmaRay", level), opponent, caster);
            Assert.That(opponent.currentHP, Is.EqualTo(hp));
            Assert.That(opponent.shield, Is.EqualTo(remainingShield));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Roots_BlocksCreatureCounterattacksUntilEndOfTurn(int level)
        {
            var panel = Creatures();
            var view = panel.GetActiveCreatureViews()[0];
            var creature = view.GetCreatureInstance();
            resolver.Resolve(Spell("EarthRoots", level), view, caster);
            Assert.That(creature.statusEffects.rootedUntilEndOfTurn, Is.True);
            resolver.Resolve(Spell("Fireball", 1), view, caster);
            Assert.That(caster.currentHP, Is.EqualTo(15));
            panel.ClearEndOfTurnEffects();
            Assert.That(creature.statusEffects.rootedUntilEndOfTurn, Is.False);
            resolver.Resolve(Spell("Fireball", 1), view, caster);
            Assert.That(caster.currentHP, Is.EqualTo(13));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Roots_AllowsOnlyOneActualCastNextTurn_ThenExpires(int level)
        {
            var heal = Spell("Healing", 1);
            Fund(opponent, heal, 3);
            var page = Page(heal, opponent);
            resolver.ResolveAgainstPlayer(Spell("EarthRoots", level), opponent, caster);
            Assert.That(opponent.statusEffects.spellLimitNextTurn, Is.EqualTo(1));
            Assert.That(opponent.statusEffects.spellLimitThisTurn, Is.EqualTo(-1));
            opponent.BeginTurn();
            opponent.currentHP = 5;
            page.OnPointerClick(null);
            Assert.That(opponent.currentHP, Is.EqualTo(7));
            Assert.That(opponent.SpellsCastThisTurn, Is.EqualTo(1));
            int inventoryAfterFirst = opponent.inventory.GetTotalCount();
            int discardAfterFirst = ingredients.GetDiscardCount();
            page.OnPointerClick(null);
            Assert.That(opponent.currentHP, Is.EqualTo(7));
            Assert.That(opponent.SpellsCastThisTurn, Is.EqualTo(1));
            Assert.That(opponent.inventory.GetTotalCount(), Is.EqualTo(inventoryAfterFirst));
            Assert.That(ingredients.GetDiscardCount(), Is.EqualTo(discardAfterFirst));
            opponent.EndTurn();
            opponent.BeginTurn();
            page.OnPointerClick(null);
            Assert.That(opponent.currentHP, Is.EqualTo(9));
            Assert.That(opponent.SpellsCastThisTurn, Is.EqualTo(1));
        }

        [Test]
        public void AcidLevelThree_AddsOneToSubsequentCreatureDamage_AndExpires()
        {
            var panel = Creatures();
            var view = panel.GetActiveCreatureViews()[0];
            var creature = view.GetCreatureInstance();
            resolver.Resolve(Spell("AcidExplosion", 3), view, caster);
            Assert.That(
                creature.currentHP,
                Is.EqualTo(28),
                "First acid hit has no corrosion bonus."
            );
            Assert.That(creature.statusEffects.corroded, Is.True);
            resolver.Resolve(Spell("Fireball", 1), view, caster);
            Assert.That(creature.currentHP, Is.EqualTo(25));
            panel.ClearEndOfTurnEffects();
            Assert.That(creature.statusEffects.corroded, Is.False);
            Assert.That(creature.isCorroded, Is.False);
            resolver.Resolve(Spell("Fireball", 1), view, caster);
            Assert.That(creature.currentHP, Is.EqualTo(23));
        }

        [Test]
        public void AcidExplosionLevelThree_DamagesOpponentAndAppliesCorrosion()
        {
            var managerObject = new GameObject("PlayerManager");

            var manager = managerObject.AddComponent<PlayerManager>();

            try
            {
                SetField(manager, "players", new List<PlayerState> { caster, opponent });
                SetField(resolver, "playerManager", manager);
                opponent.shield = 1;
                resolver.ResolveAgainstPlayer(Spell("AcidExplosion", 3), opponent, caster);
                Assert.That(opponent.currentHP, Is.EqualTo(14));
                Assert.That(opponent.shield, Is.Zero);
                Assert.That(opponent.statusEffects.corroded, Is.True);
                Assert.That(caster.currentHP, Is.EqualTo(15));
            }
            finally
            {
                Object.DestroyImmediate(managerObject);
            }
        }

        [Test]
        public void AcidExplosionCorrosion_AddsOneDamageToFollowingSpellAndExpires()
        {
            var managerObject = new GameObject("PlayerManager");

            var manager = managerObject.AddComponent<PlayerManager>();

            try
            {
                SetField(manager, "players", new List<PlayerState> { caster, opponent });
                SetField(resolver, "playerManager", manager);
                resolver.ResolveAgainstPlayer(Spell("AcidExplosion", 3), opponent, caster);
                Assert.That(opponent.statusEffects.corroded, Is.True);
                int hpAfterAcid = opponent.currentHP;
                resolver.ResolveAgainstPlayer(Spell("Fireball", 1), opponent, caster);
                Assert.That(opponent.currentHP, Is.EqualTo(hpAfterAcid - 3));
                opponent.EndTurn();
                Assert.That(opponent.statusEffects.corroded, Is.False);
                int hpAfterEndTurn = opponent.currentHP;
                resolver.ResolveAgainstPlayer(Spell("Fireball", 1), opponent, caster);
                Assert.That(opponent.currentHP, Is.EqualTo(hpAfterEndTurn - 2));
            }
            finally
            {
                Object.DestroyImmediate(managerObject);
            }
        }

        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, true)]
        public void Healing_CleansesAllNegativeEffectsOnlyAtLevelThree(int level, bool cleanses)
        {
            caster.currentHP = 14;
            caster.statusEffects.AddBurn(opponent);
            caster.statusEffects.AddBurn(opponent);
            caster.statusEffects.corroded = true;
            caster.statusEffects.spellLimitNextTurn = 1;
            caster.statusEffects.spellLimitThisTurn = 1;
            caster.statusEffects.reflectNextDamage = true;
            resolver.Resolve(Spell("Healing", level), null, caster);
            Assert.That(caster.currentHP, Is.EqualTo(caster.maxHP));
            Assert.That(caster.statusEffects.GetBurnCount(), Is.EqualTo(cleanses ? 0 : 2));
            Assert.That(caster.statusEffects.corroded, Is.EqualTo(!cleanses));
            Assert.That(caster.statusEffects.spellLimitThisTurn, Is.EqualTo(cleanses ? -1 : 1));
            Assert.That(caster.statusEffects.spellLimitNextTurn, Is.EqualTo(cleanses ? -1 : 1));
            Assert.That(caster.statusEffects.reflectNextDamage, Is.True);
        }

        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, true)]
        public void Shield_ReflectsExactlyOneDamageOnceOnlyAtLevelThree(int level, bool reflects)
        {
            opponent.shield = 5;
            caster.shield = 4;
            resolver.Resolve(Spell("ArcaneShield", level), null, opponent);
            Assert.That(opponent.shield, Is.EqualTo(PlayerState.MaxShield));
            Assert.That(opponent.statusEffects.reflectNextDamage, Is.EqualTo(reflects));
            resolver.ResolveAgainstPlayer(Spell("Fireball", 1), opponent, caster);
            Assert.That(caster.currentHP, Is.EqualTo(reflects ? 14 : 15));
            Assert.That(caster.shield, Is.EqualTo(4), "Reflected damage goes directly to HP.");
            Assert.That(opponent.statusEffects.reflectNextDamage, Is.False);
            resolver.ResolveAgainstPlayer(Spell("Fireball", 1), opponent, caster);
            Assert.That(caster.currentHP, Is.EqualTo(reflects ? 14 : 15));
        }
    }
}
