using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGamePanelController : MonoBehaviour
{
    [Header("Players")]
    [SerializeField]
    private TMP_Dropdown player1Dropdown;

    [SerializeField]
    private TMP_Dropdown player2Dropdown;

    [SerializeField]
    private TMP_Dropdown player3Dropdown;

    [SerializeField]
    private TMP_Dropdown player4Dropdown;

    [Header("Bot Difficulty")]
    [SerializeField]
    private TMP_Dropdown difficultyDropdown;

    public void StartGame()
    {
        GameSetup.Players[0] = DropdownToPlayerType(player1Dropdown.value);

        GameSetup.Players[1] = DropdownToPlayerType(player2Dropdown.value);

        GameSetup.Players[2] = DropdownToPlayerType(player3Dropdown.value);

        GameSetup.Players[3] = DropdownToPlayerType(player4Dropdown.value);

        GameSetup.Difficulty = DropdownToDifficulty(difficultyDropdown.value);

        int activePlayers = 0;

        foreach (PlayerType playerType in GameSetup.Players)
        {
            if (playerType != PlayerType.Empty)
            {
                activePlayers++;
            }
        }

        if (activePlayers < 2)
        {
            Debug.LogWarning("Se necesitan al menos 2 jugadores.");

            return;
        }

        SceneManager.LoadScene("Game");
    }

    private PlayerType DropdownToPlayerType(int value)
    {
        return value switch
        {
            0 => PlayerType.Human,
            1 => PlayerType.Bot,
            _ => PlayerType.Empty,
        };
    }

    private BotDifficulty DropdownToDifficulty(int value)
    {
        return value switch
        {
            0 => BotDifficulty.Easy,
            1 => BotDifficulty.Normal,
            2 => BotDifficulty.Hard,
            _ => BotDifficulty.Normal,
        };
    }
}
