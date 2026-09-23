using System.Collections.Generic;
using UnityEngine;

public class PlayerSpellBook : MonoBehaviour
{
    [Header("Spell Definitions")]
    [SerializeField] private List<SpellDefinition> startingSpells;

    public List<SpellInstance> Spells { get; private set; }

    private void Awake()
    {
        Spells = new List<SpellInstance>();

        foreach (SpellDefinition definition in startingSpells)
        {
            Spells.Add(new SpellInstance(definition));
        }
    }
}