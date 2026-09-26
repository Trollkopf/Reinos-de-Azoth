using TMPro;
using UnityEngine;

public class EndGamePanel : MonoBehaviour
{
    [SerializeField]
    private GameObject panelRoot;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text resultText;

    [SerializeField]
    private PlayerState humanPlayer;

    public void Show(PlayerState winner)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = "Partida Terminada";
        }

        if (resultText != null)
        {
            if (winner == humanPlayer)
            {
                resultText.text = "¡Victoria!";
                resultText.color = Color.darkGreen;
            }
            else
            {
                resultText.text = "Derrota";
                resultText.color = Color.darkRed;
            }
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    
}
