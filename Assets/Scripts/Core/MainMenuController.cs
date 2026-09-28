using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject mainPanel;

    [SerializeField]
    private GameObject newGamePanel;

    [SerializeField]
    private GameObject optionsPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        if (newGamePanel != null)
        {
            newGamePanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    public void ShowNewGame()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (newGamePanel != null)
        {
            newGamePanel.SetActive(true);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    public void ShowOptions()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (newGamePanel != null)
        {
            newGamePanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    
}
