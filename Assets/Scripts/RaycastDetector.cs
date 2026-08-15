using UnityEngine;

public struct ContacInfo
{
    public bool contacted;
    public Vector3 point;
    public Collider collider;
    public Transform transform;
}

public class RaycastDetector 
{

    public ContacInfo RayCast(int layerMask)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hit  = Physics.Raycast(ray, out  RaycastHit hitInfo, float.PositiveInfinity, 1<<layerMask);

        return new ContacInfo
        {
            contacted = hit,
            point = hitInfo.point,
            collider = hitInfo.collider,
            transform = hitInfo.transform
        };
    }
}
