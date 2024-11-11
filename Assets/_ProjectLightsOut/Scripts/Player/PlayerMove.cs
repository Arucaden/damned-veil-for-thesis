using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private bool isMoving = false;
    private List<Transform> waypoints = new List<Transform>();
    public Action<bool> OnPlayerMoving;

    private void Update()
    {
        Move();
    }

    private void OnEnable()
    {
        EventManager.AddListener<OnPlayerMove>(OnPlayerMove);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnPlayerMove>(OnPlayerMove);
    }

    private void OnPlayerMove(OnPlayerMove evt)
    {
        isMoving = evt.IsMoving;
        waypoints = evt.Waypoints;
        OnPlayerMoving?.Invoke(isMoving);
    }

    private void Move()
    {
        if (!isMoving)
        {
            return;
        }

        if (waypoints.Count > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, waypoints[0].position, moveSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, waypoints[0].position) < 0.1f)
            {
                waypoints.RemoveAt(0);
            }
        }

        else
        {
            isMoving = false;
            OnPlayerMoving?.Invoke(isMoving);
            EventManager.Broadcast(new OnPlayerFinishMove());
        }
    }
}
