using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpponentPortraitView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Image artworkImage;

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text nameTextShadow;

    [System.Serializable]
    public class OpponentIdentity
    {
        public string displayName;
        public Sprite artwork;
    }

    private PlayerState player;
    private PlayerTargetSelectionManager selectionManager;

    public void Setup(
        PlayerState targetPlayer,
        PlayerTargetSelectionManager targetSelectionManager,
        string displayName,
        Sprite artwork
    )
    {
        player = targetPlayer;
        selectionManager = targetSelectionManager;

        if (nameText != null)
        {
            nameText.text = displayName;
            nameTextShadow.text = displayName;
        }

        if (artworkImage != null)
        {
            artworkImage.sprite = artwork;
            artworkImage.enabled = artwork != null;
        }
    }

    public void SelectPlayer()
    {
        if (player == null || selectionManager == null)
        {
            return;
        }

        selectionManager.SelectPlayer(player);
    }

    public PlayerState GetPlayer()
    {
        return player;
    }
}
