using System;
using System.Collections.Generic;

namespace Azoth.Core
{
    public enum IngredientType { RedHerb, PureWater, SulfurMineral, AirCrystal, BoneDust }
    public enum SpellId { Fireball, PlasmaBolt, Healing, ArcaneShield, WindWhip, EarthRoots, LifeDrain, AcidExplosion, Illusion }
    public enum TargetType { Self, PlayerOrCreature, OtherPlayers }
    public enum GamePhase { Market, Main, Discard }

    public static class Rules
    {
        public const int MaxHealth = 15;
        public const int MaxHand = 7;
        public const int VictoryPower = 10;
    }

    public sealed class SpellDefinition
    {
        public SpellId Id { get; }
        public string Name { get; }
        public TargetType Target { get; }
        public IReadOnlyList<IngredientType> Cost { get; }
        internal SpellDefinition(SpellId id, string name, TargetType target, params IngredientType[] cost)
        { Id = id; Name = name; Target = target; Cost = Array.AsReadOnly(cost); }
    }

    public static class SpellCatalog
    {
        private static readonly IReadOnlyList<SpellDefinition> definitions = Array.AsReadOnly(new[]
        {
            new SpellDefinition(SpellId.Fireball, "Bola de Fuego", TargetType.PlayerOrCreature, IngredientType.RedHerb, IngredientType.SulfurMineral),
            new SpellDefinition(SpellId.PlasmaBolt, "Rayo de Plasma", TargetType.PlayerOrCreature, IngredientType.PureWater, IngredientType.AirCrystal),
            new SpellDefinition(SpellId.Healing, "Curación", TargetType.Self, IngredientType.PureWater, IngredientType.RedHerb),
            new SpellDefinition(SpellId.ArcaneShield, "Escudo Arcano", TargetType.Self, IngredientType.SulfurMineral, IngredientType.PureWater),
            new SpellDefinition(SpellId.WindWhip, "Látigo de Viento", TargetType.PlayerOrCreature, IngredientType.AirCrystal, IngredientType.RedHerb),
            new SpellDefinition(SpellId.EarthRoots, "Raíces de Tierra", TargetType.PlayerOrCreature, IngredientType.SulfurMineral, IngredientType.SulfurMineral, IngredientType.PureWater),
            new SpellDefinition(SpellId.LifeDrain, "Drenaje Vital", TargetType.PlayerOrCreature, IngredientType.BoneDust, IngredientType.RedHerb),
            new SpellDefinition(SpellId.AcidExplosion, "Explosión Ácida", TargetType.OtherPlayers, IngredientType.PureWater, IngredientType.BoneDust),
            new SpellDefinition(SpellId.Illusion, "Ilusión", TargetType.Self, IngredientType.AirCrystal, IngredientType.BoneDust)
        });
        public static IReadOnlyList<SpellDefinition> All => definitions;
        public static SpellDefinition Get(SpellId id)
        {
            foreach (var definition in definitions) if (definition.Id == id) return definition;
            throw new ArgumentOutOfRangeException(nameof(id));
        }
    }

    public sealed class SpellProgress
    {
        public SpellDefinition Definition { get; }
        public int Mastery { get; private set; }
        public int Level => Mastery >= 6 ? 3 : Mastery >= 3 ? 2 : 1;
        public bool UsedThisTurn { get; private set; }
        public SpellProgress(SpellId id) { Definition = SpellCatalog.Get(id); }
        // Invoke only after successful resolution, never during validation.
        public void RecordCast()
        {
            if (UsedThisTurn) throw new InvalidOperationException("Hechizo ya utilizado.");
            Mastery++;
            UsedThisTurn = true;
        }
        public void ResetTurn() { UsedThisTurn = false; }
        public bool CanPay(IReadOnlyList<IngredientType> hand)
        {
            var available = new List<IngredientType>(hand);
            foreach (var ingredient in Definition.Cost) if (!available.Remove(ingredient)) return false;
            return true;
        }
    }
}
