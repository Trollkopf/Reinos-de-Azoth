using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreatureView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TMP_Text creatureNameText;

    [SerializeField]
    private TMP_Text rankText;

    [SerializeField]
    private TMP_Text healthText;

    [SerializeField]
    private TMP_Text attackText;

    [SerializeField]
    private TMP_Text rewardText;

    [SerializeField]
    private TMP_Text abilityText;

    [SerializeField]
    private Image artwork;

    [SerializeField]
    private GameObject selectionHighlight;

    private CreatureSelectionManager selectionManager;

    private CreatureInstance creature;

    public void Setup(CreatureInstance newCreature)
    {
        creature = newCreature;
        SetSelected(false);
        Refresh();
    }

    public void Refresh()
    {
        if (creature == null)
            return;

        CreatureDefinition definition = creature.definition;

        creatureNameText.text = definition.creatureName;

        rankText.text = GetRankName(definition.rank);

        healthText.text = $"Vida: {creature.currentHP} / {definition.maxHP}";

        attackText.text = $"Ataque: {definition.attack}";

        rewardText.text =
            $"{definition.coinReward} monedas\n" + $"{definition.arcanePowerReward} Poder Arcano";

        abilityText.text = $"Habilidad: {definition.abilityDescription}";

        if (definition.artwork != null)
        {
            artwork.sprite = definition.artwork;
        }
    }

    private string GetRankName(CreatureRank rank)
    {
        return rank switch
        {
            CreatureRank.Common => "Común",
            CreatureRank.Intermediate => "Intermedia",
            CreatureRank.Epic => "Épica",
            _ => rank.ToString(),
        };
    }

    public void SetSelectionManager(CreatureSelectionManager manager)
    {
        selectionManager = manager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectionManager == null)
            return;

        selectionManager.SelectCreature(this);
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(selected);
        }
    }

    public string GetCreatureName()
    {
        if (creature == null)
            return "";

        return creature.definition.creatureName;
    }

    public CreatureInstance GetCreatureInstance()
    {
        return creature;
    }
}
