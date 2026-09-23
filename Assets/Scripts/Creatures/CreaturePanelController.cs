using UnityEngine;

public class CreaturePanelController : MonoBehaviour
{
    [SerializeField] private GameObject creaturePanel;

    private void Start()
    {
        ClosePanel();
    }

    public void OpenPanel()
    {
        creaturePanel.SetActive(true);
    }

    public void ClosePanel()
    {
        creaturePanel.SetActive(false);
    }
}