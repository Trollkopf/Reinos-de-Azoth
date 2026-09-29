using UnityEngine;

public class GameUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject grimoirePanel;

    public void OpenGrimoire()
    {
        if (grimoirePanel == null)
            return;

        grimoirePanel.SetActive(true);
    }

    public void CloseGrimoire()
    {
        if (grimoirePanel == null)
            return;

        grimoirePanel.SetActive(false);
    }

    public void ToggleGrimoire()
    {
        if (grimoirePanel == null)
            return;

        grimoirePanel.SetActive(
            !grimoirePanel.activeSelf
        );
    }
}