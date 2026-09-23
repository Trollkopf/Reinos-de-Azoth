using UnityEngine;

public class MarketPanelController : MonoBehaviour
{
    [SerializeField] public GameObject marketPanel;

    private void Start()
    {
        CloseMarket();
    }

    public void OpenMarket()
    {
        marketPanel.SetActive(true);
    }

    public void CloseMarket()
    {
        marketPanel.SetActive(false);
    }
}