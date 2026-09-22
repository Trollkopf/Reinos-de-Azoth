using System;
using System.Linq;
using Azoth.Core;
using NUnit.Framework;

public class GameSessionTests
{
    private static GameSession Create(int seed = 42) => new GameSession(new[] { "Humano", "Bot" }, seed);
    private static ActionResult Act(GameSession game, ActionKind kind, int index = -1)
        => game.Execute(new GameAction(game.CurrentPlayer.Id, kind, index));

    [Test] public void SetupPreservesIngredientDistributionAndInitialStats()
    {
        var game = Create();
        var all = game.IngredientDeck.Concat(game.Players.SelectMany(p => p.Hand)).ToArray();
        Assert.That(all.Length, Is.EqualTo(108));
        int[] counts = { 25, 25, 22, 20, 16 };
        for (int i = 0; i < counts.Length; i++) Assert.That(all.Count(x => (int)x == i), Is.EqualTo(counts[i]));
        Assert.That(game.Market.Count, Is.EqualTo(5));
        Assert.That(game.CurrentPlayer.Hand.Count, Is.EqualTo(4));
        foreach (var player in game.Players)
        {
            Assert.That(player.Health, Is.EqualTo(15));
            Assert.That(player.Coins, Is.EqualTo(3));
            Assert.That(player.ArcanePower, Is.Zero);
            Assert.That(player.Spells.Count, Is.EqualTo(9));
            Assert.That(player.Spells.All(s => s.Level == 1 && s.Mastery == 0), Is.True);
        }
    }
    [Test] public void SameSeedReproducesSetup()
    {
        var a = Create(); var b = Create();
        Assert.That(a.CurrentPlayerIndex, Is.EqualTo(b.CurrentPlayerIndex));
        Assert.That(a.IngredientDeck, Is.EqualTo(b.IngredientDeck));
        Assert.That(a.Market.Select(c => c.Ingredient), Is.EqualTo(b.Market.Select(c => c.Ingredient)));
        for (int i = 0; i < 2; i++) Assert.That(a.Players[i].Hand, Is.EqualTo(b.Players[i].Hand));
    }
    [Test] public void InvalidActionsDoNotChangeState()
    {
        var game = Create(); var hand = game.CurrentPlayer.Hand.ToArray();
        Assert.That(game.Execute(new GameAction(99, ActionKind.BeginMainPhase)).Success, Is.False);
        Assert.That(Act(game, ActionKind.BuyMarketCard, -1).Success, Is.False);
        Assert.That(Act(game, ActionKind.EndTurn).Success, Is.False);
        Assert.That(game.Phase, Is.EqualTo(GamePhase.Market));
        Assert.That(game.CurrentPlayer.Coins, Is.EqualTo(3));
        Assert.That(game.CurrentPlayer.Hand, Is.EqualTo(hand));
    }
    [Test] public void PurchaseChargesPriceAndAllowsOnlyOnePurchase()
    {
        var game = Create();
        int index = Enumerable.Range(0, 5).First(i => game.Market[i].Price <= 3);
        var card = game.Market[index]; int size = game.CurrentPlayer.Hand.Count;
        Assert.That(Act(game, ActionKind.BuyMarketCard, index).Success, Is.True);
        Assert.That(game.CurrentPlayer.Coins, Is.EqualTo(3 - card.Price));
        Assert.That(game.CurrentPlayer.Hand.Count, Is.EqualTo(size + card.Quantity));
        Assert.That(game.Market.Count, Is.EqualTo(5));
        Assert.That(Act(game, ActionKind.BuyMarketCard, 0).Success, Is.False);
        Assert.That(Act(game, ActionKind.BeginMainPhase).Success, Is.True);
        Assert.That(Act(game, ActionKind.BuyMarketCard, 0).Success, Is.False);
    }
    [Test] public void ManyTurnsRecycleDeckAndRequireDiscardBeforeAdvancing()
    {
        var game = Create(); bool discarded = false;
        for (int turn = 0; turn < 300; turn++)
        {
            var player = game.CurrentPlayer;
            int before = player.Hand.Count;
            Assert.That(Act(game, ActionKind.BeginMainPhase).Success, Is.True);
            Assert.That(Act(game, ActionKind.EndTurn).Success, Is.True);
            if (before > 7)
            {
                Assert.That(game.CurrentPlayer, Is.SameAs(player));
                Assert.That(game.Phase, Is.EqualTo(GamePhase.Discard));
                Assert.That(Act(game, ActionKind.DiscardIngredient, 999).Success, Is.False);
            }
            while (game.Phase == GamePhase.Discard)
            { Assert.That(Act(game, ActionKind.DiscardIngredient, 0).Success, Is.True); discarded = true; }
            Assert.That(player.Hand.Count, Is.LessThanOrEqualTo(7));
            Assert.That(game.CurrentPlayer, Is.Not.SameAs(player));
            Assert.That(game.IngredientDeck.Count + game.IngredientDiscard.Count + game.Players.Sum(p => p.Hand.Count), Is.EqualTo(108));
        }
        Assert.That(discarded, Is.True);
        Assert.That(game.Round, Is.EqualTo(151));
    }
    [Test] public void MasteryLevelsAndPerTurnRestriction()
    {
        var spell = new SpellProgress(SpellId.Fireball);
        for (int use = 1; use <= 7; use++)
        {
            spell.RecordCast();
            Assert.That(spell.Level, Is.EqualTo(use >= 6 ? 3 : use >= 3 ? 2 : 1));
            Assert.Throws<InvalidOperationException>(() => spell.RecordCast());
            Assert.That(spell.Mastery, Is.EqualTo(use));
            spell.ResetTurn();
        }
    }
    [Test] public void RootsRequiresTwoDistinctMineralCards()
    {
        var roots = new SpellProgress(SpellId.EarthRoots);
        Assert.That(roots.CanPay(new[] { IngredientType.SulfurMineral, IngredientType.PureWater }), Is.False);
        Assert.That(roots.CanPay(new[] { IngredientType.SulfurMineral, IngredientType.SulfurMineral, IngredientType.PureWater }), Is.True);
    }
    [Test] public void RejectsUnsupportedPlayerCountsAndEmptyNames()
    {
        Assert.Throws<ArgumentException>(() => new GameSession(new[] { "Solo" }, 1));
        Assert.Throws<ArgumentException>(() => new GameSession(new[] { "A", " " }, 1));
        Assert.That(new GameSession(new[] { "A", "B", "C", "D" }, 1).Players.Count, Is.EqualTo(4));
    }
}
