using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ReinosDeAzoth.Tests.PlayMode
{
    // Reflection is limited to scene wiring and deterministic deck setup.
    // Gameplay is exercised through public entry points, never private methods.
    public abstract class SpellIntegrationFixture
    {
        private GameObject root;
        private readonly List<ScriptableObject> ownedAssets = new List<ScriptableObject>();
        private Random.State randomState;
        protected PlayerState caster;
        protected PlayerState opponent;
        protected SpellResolver resolver;
        protected IngredientDeck ingredients;

        [SetUp]
        public void SetUp()
        {
            randomState = Random.state;
            root = new GameObject("Spell integration fixture");
            caster = Component<PlayerState>("Caster");
            opponent = Component<PlayerState>("Opponent");
            ingredients = Component<IngredientDeck>("Ingredients");
            SetField(caster, "ingredientDeck", ingredients);
            SetField(opponent, "ingredientDeck", ingredients);
            resolver = Component<SpellResolver>("Resolver");
            SetField(resolver, "ingredientDeck", ingredients);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root);
            foreach (var asset in ownedAssets) Object.Destroy(asset);
            ownedAssets.Clear();
            yield return null; // Flush deferred destruction, including Illusion options.
            Random.state = randomState;
        }

        protected T Component<T>(string name, bool active = true, Transform parent = null)
            where T : UnityEngine.Component
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.SetActive(false);
            go.transform.SetParent(parent != null ? parent : root.transform, false);
            var component = go.AddComponent<T>();
            go.SetActive(active);
            return component;
        }

        protected T Asset<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            ownedAssets.Add(asset);
            return asset;
        }

        protected SpellInstance Spell(string assetName, int level)
        {
#if UNITY_EDITOR
            var definition = AssetDatabase.LoadAssetAtPath<SpellDefinition>(
                "Assets/ScriptableObjects/Spells/" + assetName + ".asset");
            Assert.That(definition, Is.Not.Null, "Missing project spell asset: " + assetName);
            return new SpellInstance(definition) { level = level };
#else
            Assert.Ignore("These integration tests use project assets and run in the Editor PlayMode runner.");
            return null;
#endif
        }

        protected static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing serialized dependency: " + name);
            field.SetValue(target, value);
        }

        protected static T Field<T>(object target, string name)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing fixture field: " + name);
            return (T)field.GetValue(target);
        }

        protected CreaturePanel Creatures()
        {
            var definition = Asset<CreatureDefinition>();
            definition.creatureName = "Test creature";
            definition.maxHP = 30;
            definition.attack = 2;
            definition.coinReward = 4;
            definition.arcanePowerReward = 2;

            var prefab = Component<CreatureView>("Creature prefab", false);
            foreach (var name in new[] { "creatureNameText", "rankText", "healthText",
                "attackText", "rewardText", "abilityText" })
                SetField(prefab, name, Component<TextMeshProUGUI>(name, false, prefab.transform));

            var deck = Component<CreatureDeck>("Creature deck", false);
            SetField(deck, "creatureDefinitions", new List<CreatureDefinition>
                { definition, definition, definition, definition, definition, definition });
            deck.gameObject.SetActive(true);

            var panel = Component<CreaturePanel>("Creature panel", false);
            SetField(panel, "creatureDeck", deck);
            SetField(panel, "creaturePrefab", prefab);
            SetField(panel, "creatureContainer", panel.transform);
            SetField(resolver, "creaturePanel", panel);
            panel.gameObject.SetActive(true); // Exercise actual Awake initialization.
            Assert.That(panel.GetActiveCreatureViews().Count, Is.EqualTo(3));
            return panel;
        }

        protected SpellPageView Page(SpellInstance spell, PlayerState player)
        {
            var page = Component<SpellPageView>("Spell page");
            foreach (var name in new[] { "spellNameText", "levelText", "descriptionText", "masteryText" })
                SetField(page, name, Component<TextMeshProUGUI>(name, false, page.transform));
            SetField(page, "artwork", Component<Image>("Artwork", false, page.transform));
            SetField(page, "masteryFill", Component<Image>("Mastery", false, page.transform));
            var costs = Component<CanvasGroup>("Costs", false, page.transform);
            SetField(page, "costPanel", costs.transform);
            var icon = Component<IngredientIconView>("Cost prefab", false);
            SetField(icon, "iconImage", Component<Image>("Icon", false, icon.transform));
            SetField(page, "ingredientIconPrefab", icon);
            SetField(page, "ingredientIconDatabase", Asset<IngredientIconDatabase>());
            page.SetPlayer(player);
            page.SetSpellResolver(resolver);
            page.Setup(spell);
            return page;
        }

        protected void Fund(PlayerState player, SpellInstance spell, int copies = 1)
        {
            foreach (var cost in spell.definition.cost)
                player.inventory.Add(cost.type, cost.amount * copies);
        }
    }
}

