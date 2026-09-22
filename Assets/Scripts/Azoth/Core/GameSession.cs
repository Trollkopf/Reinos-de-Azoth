using System;
using System.Collections.Generic;

namespace Azoth.Core
{
    public sealed class PlayerState
    {
        internal readonly List<IngredientType> Ingredients = new List<IngredientType>();
        public int Id { get; }
        public string Name { get; }
        public int Health { get; internal set; } = Rules.MaxHealth;
        public int Coins { get; internal set; } = 3;
        public int ArcanePower { get; internal set; }
        public IReadOnlyList<IngredientType> Hand { get; }
        public IReadOnlyList<SpellProgress> Spells { get; }
        internal PlayerState(int id, string name)
        {
            Id = id; Name = name; Hand = Ingredients.AsReadOnly();
            var spells = new List<SpellProgress>();
            foreach (var definition in SpellCatalog.All) spells.Add(new SpellProgress(definition.Id));
            Spells = spells.AsReadOnly();
        }
    }

    public sealed class MarketCard
    {
        public IngredientType Ingredient { get; }
        public int Quantity { get; }
        public int Price { get; }
        internal MarketCard(IngredientType ingredient, int quantity, int price)
        { Ingredient = ingredient; Quantity = quantity; Price = price; }
    }

    public enum ActionKind { BuyMarketCard, BeginMainPhase, EndTurn, DiscardIngredient }
    public readonly struct GameAction
    {
        public int PlayerId { get; }
        public ActionKind Kind { get; }
        public int Index { get; }
        public GameAction(int playerId, ActionKind kind, int index = -1)
        { PlayerId = playerId; Kind = kind; Index = index; }
    }
    public readonly struct ActionResult
    {
        public bool Success { get; }
        public string Error { get; }
        internal ActionResult(bool success, string error = null) { Success = success; Error = error; }
    }

    // Foundation only: combat, creatures, elimination and victory are pending.
    public sealed class GameSession
    {
        private readonly Random random;
        private readonly List<PlayerState> players = new List<PlayerState>();
        private readonly List<IngredientType> deck = new List<IngredientType>();
        private readonly List<IngredientType> discard = new List<IngredientType>();
        private readonly List<MarketCard> marketDeck = new List<MarketCard>();
        private readonly List<MarketCard> marketDiscard = new List<MarketCard>();
        private readonly List<MarketCard> market = new List<MarketCard>();
        private bool purchased;
        private int turnsInRound;
        public IReadOnlyList<PlayerState> Players { get; }
        public IReadOnlyList<MarketCard> Market { get; }
        public IReadOnlyList<IngredientType> IngredientDeck { get; }
        public IReadOnlyList<IngredientType> IngredientDiscard { get; }
        public PlayerState CurrentPlayer => players[CurrentPlayerIndex];
        public int CurrentPlayerIndex { get; private set; }
        public int InitialPlayerIndex { get; }
        public int Round { get; private set; } = 1;
        public GamePhase Phase { get; private set; }

        public GameSession(IReadOnlyList<string> playerNames, int seed)
        {
            if (playerNames == null || playerNames.Count < 2 || playerNames.Count > 4)
                throw new ArgumentException("Se necesitan entre 2 y 4 jugadores.", nameof(playerNames));
            random = new Random(seed);
            for (int i = 0; i < playerNames.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(playerNames[i])) throw new ArgumentException("Nombre vacío.");
                players.Add(new PlayerState(i, playerNames[i]));
            }
            Players = players.AsReadOnly(); Market = market.AsReadOnly();
            IngredientDeck = deck.AsReadOnly(); IngredientDiscard = discard.AsReadOnly();
            int[] counts = { 25, 25, 22, 20, 16 }, offers = { 12, 12, 8, 8, 8 }, prices = { 2, 2, 3, 3, 4 };
            for (int type = 0; type < counts.Length; type++)
            {
                for (int i = 0; i < counts[type]; i++) deck.Add((IngredientType)type);
                for (int i = 0; i < offers[type]; i++) marketDeck.Add(new MarketCard((IngredientType)type, type == 4 ? 1 : 2, prices[type]));
            }
            Shuffle(deck); Shuffle(marketDeck);
            for (int i = 0; i < 5; i++) RefillMarket();
            foreach (var player in players) for (int i = 0; i < 3; i++) Draw(player);
            CurrentPlayerIndex = random.Next(players.Count); InitialPlayerIndex = CurrentPlayerIndex;
            StartTurn();
        }

        public ActionResult Execute(GameAction action)
        {
            if (action.PlayerId != CurrentPlayer.Id) return Fail("No es el turno de ese jugador.");
            switch (action.Kind)
            {
                case ActionKind.BuyMarketCard:
                    if (Phase != GamePhase.Market || purchased) return Fail("Compra no disponible.");
                    if (action.Index < 0 || action.Index >= market.Count) return Fail("Carta inválida.");
                    var card = market[action.Index];
                    if (CurrentPlayer.Coins < card.Price) return Fail("Monedas insuficientes.");
                    CurrentPlayer.Coins -= card.Price;
                    for (int i = 0; i < card.Quantity; i++) CurrentPlayer.Ingredients.Add(card.Ingredient);
                    market.RemoveAt(action.Index); marketDiscard.Add(card); RefillMarket(); purchased = true;
                    return Ok();
                case ActionKind.BeginMainPhase:
                    if (Phase != GamePhase.Market) return Fail("La fase de mercado ya terminó.");
                    Phase = GamePhase.Main; return Ok();
                case ActionKind.EndTurn:
                    if (Phase != GamePhase.Main) return Fail("Debes estar en la fase principal.");
                    if (CurrentPlayer.Hand.Count > Rules.MaxHand) Phase = GamePhase.Discard;
                    else AdvanceTurn();
                    return Ok();
                case ActionKind.DiscardIngredient:
                    if (Phase != GamePhase.Discard) return Fail("No hay descartes pendientes.");
                    if (action.Index < 0 || action.Index >= CurrentPlayer.Hand.Count) return Fail("Ingrediente inválido.");
                    discard.Add(CurrentPlayer.Ingredients[action.Index]); CurrentPlayer.Ingredients.RemoveAt(action.Index);
                    if (CurrentPlayer.Hand.Count <= Rules.MaxHand) AdvanceTurn();
                    return Ok();
                default: return Fail("Acción desconocida.");
            }
        }
        private void StartTurn() { purchased = false; Draw(CurrentPlayer); Phase = GamePhase.Market; }
        private void AdvanceTurn()
        {
            foreach (var spell in CurrentPlayer.Spells) spell.ResetTurn();
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % players.Count;
            if (++turnsInRound == players.Count) { turnsInRound = 0; Round++; }
            StartTurn();
        }
        private void Draw(PlayerState player)
        {
            if (deck.Count == 0) { deck.AddRange(discard); discard.Clear(); Shuffle(deck); }
            if (deck.Count > 0) player.Ingredients.Add(TakeLast(deck));
        }
        private void RefillMarket()
        {
            if (marketDeck.Count == 0) { marketDeck.AddRange(marketDiscard); marketDiscard.Clear(); Shuffle(marketDeck); }
            if (marketDeck.Count > 0) market.Add(TakeLast(marketDeck));
        }
        private void Shuffle<T>(List<T> items)
        {
            for (int i = items.Count - 1; i > 0; i--)
            { int j = random.Next(i + 1); T temp = items[i]; items[i] = items[j]; items[j] = temp; }
        }
        private static T TakeLast<T>(List<T> items)
        { int index = items.Count - 1; T result = items[index]; items.RemoveAt(index); return result; }
        private static ActionResult Ok() => new ActionResult(true);
        private static ActionResult Fail(string error) => new ActionResult(false, error);
    }
}
