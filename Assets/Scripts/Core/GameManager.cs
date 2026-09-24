using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerState player;
    [SerializeField] private IngredientDeck ingredientDeck;
    [SerializeField] private SpellBookView spellBookView;
    [SerializeField] private IngredientInventoryView inventoryView;
    [SerializeField] private PlayerStatusView playerStatusView;
    [SerializeField] private PlayerState botPlayer;

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        ingredientDeck.DrawToPlayer(player, 3);
        ingredientDeck.DrawToPlayer(botPlayer, 3);
        inventoryView.Refresh();

        Debug.Log(
            "Partida iniciada. El jugador y el bot roban 3 ingredientes."
        );

        spellBookView.RefreshBook();
        playerStatusView.Refresh();
    }
}