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


    private void Start()
    {
        Refresh();
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
            nameText.text = player.gameObject.name;
        }

        if (healthText != null)
        {
            healthText.text = $"Vida: {player.currentHP}/{player.maxHP}";
        }

        if (shieldText != null)
        {
            shieldText.text = $"Escudo: {player.shield}";
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
}
