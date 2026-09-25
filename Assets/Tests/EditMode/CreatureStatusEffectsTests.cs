using NUnit.Framework;

namespace ReinosDeAzoth.Tests.EditMode
{
    public class CreatureStatusEffectsTests
    {
        [Test]
        public void Burns_AccumulateAsIndependentStacks()
        {
            var effects = new CreatureStatusEffects();
            Assert.That(effects.HasBurns(), Is.False);
            effects.AddBurn(null);
            effects.AddBurn(null);
            Assert.That(effects.HasBurns(), Is.True);
            Assert.That(effects.GetBurnCount(), Is.EqualTo(2));
            Assert.That(effects.burns[0], Is.Not.SameAs(effects.burns[1]));
        }

        [Test]
        public void EndTurn_ClearsRootsAndCorrosionButPreservesBurns()
        {
            var effects = new CreatureStatusEffects
            { rootedUntilEndOfTurn = true, corroded = true };
            effects.AddBurn(null);
            effects.ClearEndOfTurnEffects();
            Assert.That(effects.rootedUntilEndOfTurn, Is.False);
            Assert.That(effects.corroded, Is.False);
            Assert.That(effects.GetBurnCount(), Is.EqualTo(1));
            Assert.That(effects.burns[0].turnsRemaining, Is.EqualTo(2));
        }
    }
}
