using NUnit.Framework;

namespace ReinosDeAzoth.Tests.EditMode
{
    public class PlayerStatusEffectsTests
    {
        [Test]
        public void BeginTurn_MovesAndConsumesPendingRoots()
        {
            var effects = new PlayerStatusEffects { spellLimitNextTurn = 1 };
            effects.BeginTurn();
            Assert.That(effects.spellLimitThisTurn, Is.EqualTo(1));
            Assert.That(effects.spellLimitNextTurn, Is.EqualTo(-1));
            effects.BeginTurn();
            Assert.That(effects.spellLimitThisTurn, Is.EqualTo(-1));
        }

        [Test]
        public void EndTurn_ClearsTemporaryEffectsButPreservesPendingRootsAndBurns()
        {
            var effects = new PlayerStatusEffects
            {
                spellLimitThisTurn = 1, spellLimitNextTurn = 1,
                corroded = true, reflectNextDamage = true
            };
            effects.AddBurn(null);
            effects.ClearEndOfTurnEffects();
            Assert.That(effects.spellLimitThisTurn, Is.EqualTo(-1));
            Assert.That(effects.spellLimitNextTurn, Is.EqualTo(1));
            Assert.That(effects.corroded, Is.False);
            Assert.That(effects.reflectNextDamage, Is.True);
            Assert.That(effects.GetBurnCount(), Is.EqualTo(1));
        }

        [Test]
        public void AddBurn_AccumulatesIndependentStacks()
        {
            var effects = new PlayerStatusEffects();
            Assert.That(effects.HasBurns(), Is.False);
            effects.AddBurn(null);
            effects.AddBurn(null);
            Assert.That(effects.HasBurns(), Is.True);
            Assert.That(effects.GetBurnCount(), Is.EqualTo(2));
            Assert.That(effects.burns[0], Is.Not.SameAs(effects.burns[1]));
        }

        [Test]
        public void ClearNegativeEffects_RemovesBurnCorrosionAndBothRootsLimits()
        {
            var effects = new PlayerStatusEffects
            {
                spellLimitThisTurn = 1, spellLimitNextTurn = 1,
                corroded = true, reflectNextDamage = true
            };
            effects.AddBurn(null);
            effects.AddBurn(null);
            effects.ClearNegativeEffects();
            Assert.That(effects.HasBurns(), Is.False);
            Assert.That(effects.corroded, Is.False);
            Assert.That(effects.spellLimitThisTurn, Is.EqualTo(-1));
            Assert.That(effects.spellLimitNextTurn, Is.EqualTo(-1));
            Assert.That(effects.reflectNextDamage, Is.True);
        }

        [Test]
        public void Reflection_CanOnlyBeConsumedOnce()
        {
            var effects = new PlayerStatusEffects();
            Assert.That(effects.ConsumeReflection(), Is.False);
            effects.reflectNextDamage = true;
            Assert.That(effects.ConsumeReflection(), Is.True);
            Assert.That(effects.ConsumeReflection(), Is.False);
            Assert.That(effects.reflectNextDamage, Is.False);
        }
    }
}
