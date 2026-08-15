using UnityEngine;

public class Car : MonoBehaviour
{
    public Route route;

    public Transform bottomTransform;
    [SerializeField] MeshRenderer meshRenderer;

    public void SetColor(Color color)
    {
        meshRenderer.sharedMaterials[0].color = color;
    }
}
