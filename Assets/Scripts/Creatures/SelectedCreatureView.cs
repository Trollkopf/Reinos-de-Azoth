using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedCreatureView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text creatureNameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text rewardTitleText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text abilityText;

    [SerializeField] private Image artwork;

    [SerializeField] private TMP_Text noSelectionText;

    private CreatureView selectedCreature;

    private void Start()
    {
        Clear();
    }

    public void ShowCreature(CreatureView creatureView)
    {
        selectedCreature = creatureView;

        if (selectedCreature == null)
        {
            Clear();
            return;
        }

        CreatureInstance instance =
            selectedCreature.GetCreatureInstance();

        if (instance == null)
        {
            Clear();
            return;
        }

        CreatureDefinition definition =
            instance.definition;

        noSelectionText.gameObject.SetActive(false);

        creatureNameText.gameObject.SetActive(true);
        rankText.gameObject.SetActive(true);
        healthText.gameObject.SetActive(true);
        attackText.gameObject.SetActive(true);
        rewardTitleText.gameObject.SetActive(true);
        rewardText.gameObject.SetActive(true);
        abilityText.gameObject.SetActive(true);
        artwork.gameObject.SetActive(true);

        creatureNameText.text =
            definition.creatureName;

        rankText.text =
            GetRankName(definition.rank);

        healthText.text =
            $"Vida: {instance.currentHP} / {definition.maxHP}";

        attackText.text =
            $"Ataque: {definition.attack}";

        rewardText.text =
            $"{definition.coinReward} monedas\n" +
            $"{definition.arcanePowerReward} Poder Arcano";

        abilityText.text =
            definition.abilityDescription;

        if (definition.artwork != null)
        {
            artwork.sprite = definition.artwork;
        }
    }

    public void Refresh()
    {
        if (selectedCreature != null)
        {
            ShowCreature(selectedCreature);
        }
    }

    public void Clear()
    {
        selectedCreature = null;

        noSelectionText.gameObject.SetActive(true);

        creatureNameText.gameObject.SetActive(false);
        rankText.gameObject.SetActive(false);
        healthText.gameObject.SetActive(false);
        attackText.gameObject.SetActive(false);
        rewardTitleText.gameObject.SetActive(false);
        rewardText.gameObject.SetActive(false);
        abilityText.gameObject.SetActive(false);
        artwork.gameObject.SetActive(false);
    }

    private string GetRankName(CreatureRank rank)
    {
        return rank switch
        {
            CreatureRank.Common => "Común",
            CreatureRank.Intermediate => "Intermedia",
            CreatureRank.Epic => "Épica",
            _ => rank.ToString()
        };
    }
}