using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class LinesDrawer : MonoBehaviour
{
    [SerializeField] UserInput userInput;
    [SerializeField] int interacableLayer;

    private Line currentLine;
    private Route currentRoute;

    RaycastDetector raycastDetector = new();


    //event
    public UnityAction<Route> OnBeginDraw;
    public UnityAction OnDraw;
    public UnityAction OnEndDraw;


    private void Start()
    {
        userInput.OnMouseDown += OnmouseDownHalder;
        userInput.OnMouseMove += OnmouseMoveHalder;
        userInput.OnMouseUp += OnmouseUpHalder;

    }

    //Events
    public UnityAction<Route, List<Vector3>> OnParkLinkedToLine;



    //start drawing
    private void OnmouseDownHalder()
    {
        ContacInfo contacInfo = raycastDetector.RayCast(interacableLayer);

        if (contacInfo.contacted)
        {
            bool isCar = contacInfo.collider.TryGetComponent(out Car _car);

            if (isCar && _car.route.isActive)
            {
                currentRoute = _car.route;
                currentLine = currentRoute.line;
                currentLine.Init();
                currentLine.AddPoint(_car.bottomTransform.position);

                OnBeginDraw?.Invoke(currentRoute);
            }
        }
    }
    //drawing
    private void OnmouseMoveHalder()
    {
        if (currentRoute != null)
        {
            ContacInfo contacInfo = raycastDetector.RayCast(interacableLayer);

            if (contacInfo.contacted)
            {
                Vector3 newPoint = contacInfo.point;

                if (currentLine.length >= currentRoute.maxLineLength)
                {
                    currentLine.Clear();
                    OnmouseUpHalder();
                    return;
                }

                currentLine.AddPoint(newPoint);
                OnDraw?.Invoke();

                bool isPark = contacInfo.collider.TryGetComponent(out Park _park);

                if (isPark)
                {
                    Route parkRoute = _park.route;
                    if (parkRoute == currentRoute)
                    {
                        currentLine.AddPoint(contacInfo.transform.position);
                        OnDraw?.Invoke();
                    }
                    else
                    {
                        //delete line
                        currentLine.Clear();
                    }
                    OnmouseUpHalder();
                }


            }
        }

    }
    //end drawing
    private void OnmouseUpHalder()
    {
        if (currentRoute != null)
        {
            ContacInfo contacInfo = raycastDetector.RayCast(interacableLayer);

            if (contacInfo.contacted)
            {
                bool isPark = contacInfo.collider.TryGetComponent(out Park _park);

                if (currentLine.pointsCount < 2 || !isPark)
                {
                    currentLine.Clear();
                }
                else
                {
                    OnParkLinkedToLine?.Invoke(currentRoute, currentLine.points);
                    currentRoute.Disactivate();
                }
            }
            else
            {
                currentLine.Clear();
            }
        }
        ResetDrawer();
        OnEndDraw?.Invoke();
    }

    private void ResetDrawer()
    {
        currentRoute = null;
        currentLine = null;
        
    }
}
