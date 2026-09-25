using NUnit.Framework;

namespace ReinosDeAzoth.Tests.PlayMode
{
    public class BurnIntegrationTests : SpellIntegrationFixture
    {
        [Test]
        public void StaggeredBurns_ExpireIndependentlyAfterTwoTicks()
        {
            var panel = Creatures();
            var view = panel.GetActiveCreatureViews()[0];
            var creature = view.GetCreatureInstance();
            resolver.Resolve(Spell("Fireball", 3), view, caster);
            var first = creature.statusEffects.burns[0];
            panel.ClearEndOfTurnEffects();
            Assert.That(creature.currentHP, Is.EqualTo(26));
            Assert.That(first.turnsRemaining, Is.EqualTo(1));
            resolver.Resolve(Spell("Fireball", 3), view, opponent);
            var second = creature.statusEffects.burns[1];
            panel.ClearEndOfTurnEffects();
            Assert.That(creature.currentHP, Is.EqualTo(21));
            Assert.That(first.turnsRemaining, Is.Zero);
            Assert.That(creature.statusEffects.burns, Is.EqualTo(new[] { second }));
            Assert.That(second.turnsRemaining, Is.EqualTo(1));
            panel.ClearEndOfTurnEffects();
            Assert.That(creature.currentHP, Is.EqualTo(20));
            Assert.That(creature.statusEffects.burns, Is.Empty);
            panel.ClearEndOfTurnEffects();
            Assert.That(creature.currentHP, Is.EqualTo(20));
        }

        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(1, true)]
        [TestCase(2, true)]
        public void Burns_ResolveFIFO_RewardOnlyLethalStackOwner_AndStopAtDeath(int hp, bool reverseOwners)
        {
            var panel = Creatures();
            var view = panel.GetActiveCreatureViews()[0];
            var creature = view.GetCreatureInstance();
            var firstOwner = reverseOwners ? opponent : caster;
            var secondOwner = reverseOwners ? caster : opponent;
            resolver.Resolve(Spell("Fireball", 3), view, firstOwner);
            resolver.Resolve(Spell("Fireball", 3), view, secondOwner);
            resolver.Resolve(Spell("Fireball", 3), view, firstOwner);
            var first = creature.statusEffects.burns[0];
            var second = creature.statusEffects.burns[1];
            var third = creature.statusEffects.burns[2];
            creature.currentHP = hp;
            var winner = hp == 1 ? firstOwner : secondOwner;
            var loser = hp == 1 ? secondOwner : firstOwner;
            int winnerCoins = winner.coins;
            int loserCoins = loser.coins;
            int winnerPower = winner.arcanePower;
            int loserPower = loser.arcanePower;

            panel.ClearEndOfTurnEffects();

            Assert.That(creature.IsDead, Is.True);
            Assert.That(first.turnsRemaining, Is.EqualTo(1));
            Assert.That(second.turnsRemaining, Is.EqualTo(hp == 1 ? 2 : 1));
            Assert.That(third.turnsRemaining, Is.EqualTo(2), "Later burns must not tick after death.");
            Assert.That(winner.coins, Is.EqualTo(winnerCoins + creature.definition.coinReward));
            Assert.That(winner.arcanePower, Is.EqualTo(winnerPower + creature.definition.arcanePowerReward));
            Assert.That(loser.coins, Is.EqualTo(loserCoins));
            Assert.That(loser.arcanePower, Is.EqualTo(loserPower));
            var replacement = view.GetCreatureInstance();
            Assert.That(replacement, Is.Not.SameAs(creature));
            Assert.That(replacement.currentHP, Is.EqualTo(replacement.definition.maxHP));
            Assert.That(replacement.statusEffects.HasBurns(), Is.False);
            panel.ClearEndOfTurnEffects();
            Assert.That(winner.coins, Is.EqualTo(winnerCoins + creature.definition.coinReward),
                "The replacement must not award the old reward twice.");
        }
    }
}
