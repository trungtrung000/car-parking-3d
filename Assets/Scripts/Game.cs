using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class Game : MonoBehaviour
{
    //singleton class
    public static Game Instance;

    [HideInInspector] public List<Route> readyRoutes = new();

    private int totalRoutes;
    private int successfulParks;


    //event
    public UnityAction<Route> OnCarEntersPark;
    public UnityAction OnCarCollision;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        totalRoutes = transform.GetComponentsInChildren<Route>().Length;
        successfulParks = 0;

        OnCarEntersPark += OnCarEntersParkHandler;
        OnCarCollision += OnCarCollionHandler;
    }

    private void OnCarCollionHandler()
    {
        Debug.Log("Game Over");
        int currentlevel = SceneManager.GetActiveScene().buildIndex;
        DOVirtual.DelayedCall(2f, () =>
        {
                SceneManager.LoadScene(currentlevel);
        });
    }

    private void OnCarEntersParkHandler(Route route)
    {
        route.car.StopDancingAnim();
        successfulParks++;

        if (successfulParks == totalRoutes)
        {
            Debug.Log("You win");
            int nextlevel = SceneManager.GetActiveScene().buildIndex + 1;
            DOVirtual.DelayedCall(1.3f, () =>
            {
                if (nextlevel < SceneManager.sceneCountInBuildSettings)
                    SceneManager.LoadScene(nextlevel);
                else
                    Debug.Log("no level left");
            });
        }
    }

    public void RegisterRoute(Route route)
    {
        readyRoutes.Add(route);

        if (readyRoutes.Count == totalRoutes)
        {
            MoveAllCars();
        }
    }

    private void MoveAllCars()
    {
        foreach (var route in readyRoutes)
        {
            route.car.Move(route.linePoints);
        }

    }
}
