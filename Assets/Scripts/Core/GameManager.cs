using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerState player;
    [SerializeField] private IngredientDeck ingredientDeck;
    [SerializeField] private SpellBookView spellBookView;
    [SerializeField] private IngredientInventoryView inventoryView;
    [SerializeField]private PlayerStatusView playerStatusView;

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        ingredientDeck.DrawToPlayer(player, 30);
        inventoryView.Refresh();

        Debug.Log(
            "Partida iniciada. El jugador roba x ingredientes."
        );

        spellBookView.RefreshBook();
        playerStatusView.Refresh();
    }
}