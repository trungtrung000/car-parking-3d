using UnityEngine;

public class Park : MonoBehaviour
{
    public Route route;

    [SerializeField] SpriteRenderer spriteRenderer;

    public void SetColor(Color color)
    {
        color.a = 1f;
        spriteRenderer.color = color;
    }
}
