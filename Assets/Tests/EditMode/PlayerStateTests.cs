using NUnit.Framework;
using UnityEngine;

namespace ReinosDeAzoth.Tests.EditMode
{
    public class PlayerStateTests
    {
        private PlayerState player;
        [SetUp] public void SetUp() => player = new GameObject("Player test").AddComponent<PlayerState>();
        [TearDown] public void TearDown() => Object.DestroyImmediate(player.gameObject);

        [TestCase(1, 1)]
        [TestCase(6, 6)]
        [TestCase(100, 6)]
        [TestCase(0, 0)]
        [TestCase(-1, 0)]
        public void AddShield_RespectsCapAndIgnoresNonPositiveAmounts(int amount, int expected)
        {
            player.AddShield(amount);
            Assert.That(player.shield, Is.EqualTo(expected));
            player.AddShield(100);
            player.AddShield(1);
            Assert.That(player.shield, Is.EqualTo(PlayerState.MaxShield));
        }

        [TestCase(2, 9)]
        [TestCase(100, 12)]
        [TestCase(0, 7)]
        [TestCase(-1, 7)]
        public void Heal_RespectsConfiguredMaxHP(int amount, int expected)
        {
            player.maxHP = 12;
            player.currentHP = 7;
            player.Heal(amount);
            Assert.That(player.currentHP, Is.EqualTo(expected));
        }

        [TestCase(-1, 4, true)]
        [TestCase(0, 0, false)]
        [TestCase(1, 0, true)]
        [TestCase(1, 1, false)]
        [TestCase(2, 1, true)]
        [TestCase(2, 2, false)]
        public void CanCastSpell_RespectsLimit(int limit, int casts, bool expected)
        {
            player.statusEffects.spellLimitThisTurn = limit;
            for (int i = 0; i < casts; i++) player.RegisterSpellCast();
            Assert.That(player.CanCastSpell(), Is.EqualTo(expected));
        }

        [Test]
        public void RegisterSpellCast_CountsEachCastAndBeginTurnResetsCount()
        {
            Assert.That(player.SpellsCastThisTurn, Is.Zero);
            player.RegisterSpellCast();
            Assert.That(player.SpellsCastThisTurn, Is.EqualTo(1));
            player.RegisterSpellCast();
            Assert.That(player.SpellsCastThisTurn, Is.EqualTo(2));
            player.statusEffects.spellLimitNextTurn = 1;
            player.BeginTurn();
            Assert.That(player.SpellsCastThisTurn, Is.Zero);
            Assert.That(player.GetRemainingSpellCasts(), Is.EqualTo(1));
            player.RegisterSpellCast();
            Assert.That(player.CanCastSpell(), Is.False);
            Assert.That(player.GetRemainingSpellCasts(), Is.Zero);
            player.EndTurn();
            Assert.That(player.CanCastSpell(), Is.True);
            Assert.That(player.GetRemainingSpellCasts(), Is.EqualTo(-1));
        }

        [Test]
        public void HealingLevelThree_ResolvesCleansingThroughPublicSpellResolver()
        {
            var definition = UnityEditor.AssetDatabase.LoadAssetAtPath<SpellDefinition>(
                "Assets/ScriptableObjects/Spells/Healing.asset");
            Assert.That(definition, Is.Not.Null);
            player.statusEffects.AddBurn(player);
            player.statusEffects.corroded = true;
            player.statusEffects.spellLimitThisTurn = 1;
            player.statusEffects.spellLimitNextTurn = 1;
            var spellResolver = player.gameObject.AddComponent<SpellResolver>();
            spellResolver.Resolve(new SpellInstance(definition) { level = 3 }, null, player);
            Assert.That(player.statusEffects.HasBurns(), Is.False);
            Assert.That(player.statusEffects.corroded, Is.False);
            Assert.That(player.statusEffects.spellLimitThisTurn, Is.EqualTo(-1));
            Assert.That(player.statusEffects.spellLimitNextTurn, Is.EqualTo(-1));
        }

        [Test]
        public void Burns_DealStackedDamageThroughShieldForExactlyTwoTurns()
        {
            player.shield = 1;
            player.statusEffects.AddBurn(player);
            player.statusEffects.AddBurn(player);
            player.BeginTurn();
            Assert.That(player.shield, Is.Zero);
            Assert.That(player.currentHP, Is.EqualTo(14));
            Assert.That(player.statusEffects.burns[0].turnsRemaining, Is.EqualTo(1));
            player.EndTurn();
            player.BeginTurn();
            Assert.That(player.currentHP, Is.EqualTo(12));
            Assert.That(player.statusEffects.HasBurns(), Is.False);
            player.EndTurn();
            player.BeginTurn();
            Assert.That(player.currentHP, Is.EqualTo(12));
        }
    }
}

