using TMPro;
using UnityEngine;

public class GameLogManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private GameObject entryPrefab;

    [Header("Layout")]
    [SerializeField]
    private float entryHeight = 40f;

    [SerializeField]
    private float spacing = 6f;

    private int entryCount = 0;

    private void Start()
    {
        AddEntry("El registro de acciones está funcionando.");
    }

    public void AddEntry(string message)
    {
        if (content == null || entryPrefab == null)
        {
            Debug.LogWarning("GameLogManager: faltan referencias.");

            return;
        }

        GameObject entry = Instantiate(entryPrefab, content);

        RectTransform rect = entry.GetComponent<RectTransform>();

        TMP_Text text = entry.GetComponent<TMP_Text>();

        if (text != null)
        {
            text.text = message;
        }

        if (rect != null)
        {
            rect.anchorMin = new Vector2(0f, 1f);

            rect.anchorMax = new Vector2(1f, 1f);

            rect.pivot = new Vector2(0.5f, 1f);

            rect.offsetMin = new Vector2(0f, rect.offsetMin.y);

            rect.offsetMax = new Vector2(0f, rect.offsetMax.y);

            float y = -entryCount * (entryHeight + spacing);

            rect.anchoredPosition = new Vector2(0f, y);

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, entryHeight);
        }

        entryCount++;

        float contentHeight = entryCount * entryHeight + (entryCount - 1) * spacing;

        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
    }
}
