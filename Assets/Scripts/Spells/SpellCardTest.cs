using UnityEngine;

public class SpellCardTest : MonoBehaviour
{
    [SerializeField] private SpellPageView spellCard;
    [SerializeField] private SpellDefinition fireball;

    private void Start()
    {
        SpellInstance instance =
            new SpellInstance(fireball);

        spellCard.Setup(instance);
    }
}