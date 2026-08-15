using UnityEngine;
using System.Collections.Generic;


public class Line : MonoBehaviour
{
    public LineRenderer lineRenderer;
    [SerializeField] float minPointsDistance;


    [HideInInspector] public List<Vector3> points = new();
    [HideInInspector] public int pointsCount = 0;


    private float pointFixedYAxis;

    private void Start()
    {
        pointFixedYAxis = lineRenderer.GetPosition(0).y;
        Clear();
    }

    public void Clear()
    {
        gameObject.SetActive(false);
        lineRenderer.positionCount = 0;
        pointsCount = 0;
        points.Clear();
    }

    public void AddPoint(Vector3 newPoint)
    {
        newPoint.y = pointFixedYAxis;

        if (pointsCount >= 1 && Vector3.Distance(newPoint, GetLastPoint()) < minPointsDistance)
            return;

        points.Add(newPoint);
        pointsCount++;

        lineRenderer.positionCount =pointsCount;
        lineRenderer.SetPosition(pointsCount - 1, newPoint);
    }

    private Vector3 GetLastPoint()
    {
        return lineRenderer.GetPosition(pointsCount - 1);
    }

    public void Init()
    {
        gameObject.SetActive(true);

    }

    public void SetColor(Color color)
    {
        lineRenderer.sharedMaterials[0].color = color;
    }
}
