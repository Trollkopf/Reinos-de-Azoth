using TMPro;
using UnityEngine;

public class PlayerStatusView : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] public PlayerState player;

    [Header("Texts")]
    [SerializeField] public TMP_Text healthText;
    [SerializeField] public TMP_Text coinsText;
    [SerializeField] public TMP_Text arcanePowerText;
    [SerializeField] private TMP_Text shieldText;

    public void Refresh()
    {
        if (player == null)
            return;

        healthText.text =
            $"Vida: {player.currentHP} / {player.maxHP}";

        coinsText.text =
            $"Monedas: {player.coins}";

        arcanePowerText.text =
            $"Poder Arcano: {player.arcanePower} / 10";

        shieldText.text =
            $"Escudo: {player.shield}";
    }
}