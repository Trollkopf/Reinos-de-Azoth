using TMPro;
using UnityEngine;

public class PlayerTargetView : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerState player;

    [SerializeField]
    private PlayerTargetSelectionManager selectionManager;

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text healthText;

    [SerializeField]
    private TMP_Text shieldText;

    [SerializeField]
    private TMP_Text coinsText;

    [SerializeField]
    private TMP_Text arcanePowerText;

    [SerializeField]
    private OpponentLayoutController opponentLayoutController;

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.OnStatsChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnStatsChanged -= Refresh;
        }
    }

    public void RefreshView()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (player == null)
            return;

        if (nameText != null)
        {
            OpponentIdentity identity =
                opponentLayoutController != null
                    ? opponentLayoutController.GetIdentity(player)
                    : null;

            nameText.text = identity != null ? identity.displayName : player.gameObject.name;
        }

        if (healthText != null)
        {
            healthText.text = $"Vida: {player.currentHP}/{player.maxHP}";
        }

        if (shieldText != null)
        {
            shieldText.text = $"Escudo: {player.shield}";
        }

        if (coinsText != null)
        {
            coinsText.text = $"Oro: {player.coins}";
        }

        if (arcanePowerText != null)
        {
            arcanePowerText.text = $"Poder Arcano: {player.arcanePower}/10";
        }
    }

    public void SelectPlayer()
    {
        if (player == null || selectionManager == null)
        {
            return;
        }

        selectionManager.SelectPlayer(player);

        Debug.Log($"Seleccionado como objetivo PvP: {player.gameObject.name}");
    }

    public void SetPlayer(PlayerState newPlayer)
    {
        if (player != null)
        {
            player.OnStatsChanged -= Refresh;
        }

        player = newPlayer;

        if (player != null)
        {
            player.OnStatsChanged += Refresh;
        }

        Refresh();
    }
    
}
