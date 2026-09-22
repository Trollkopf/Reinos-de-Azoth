using UnityEngine;
using UnityEngine.UI;

public class IngredientIconView : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public void Setup(Sprite sprite)
    {
        iconImage.sprite = sprite;
    }
}