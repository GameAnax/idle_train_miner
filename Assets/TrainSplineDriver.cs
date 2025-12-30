using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Dreamteck.Splines;
using System.Collections.Generic;

[RequireComponent(typeof(SplineFollower))]
public class TrainSplineDriver : MonoBehaviour
{
    private SplineFollower _splineFollower;
    private float _targetSpeed;
    private List<CustomeGrid> _pendingGrids = new List<CustomeGrid>();



    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool useSmoothStop = true;
    [SerializeField] private float smoothTime = 10f;

    public List<SplineFollower> wegons;

    void Awake()
    {
        _splineFollower = GetComponent<SplineFollower>();
        // Shuruat me speed 0 rakhenge
        _splineFollower.followSpeed = 0;
    }

    void Update()
    {
        HandleInput();
        CheckLastWagonPassed();
    }

    private void HandleInput()
    {
        // 1. Check Pointer (Mouse or Touch)
        if (Pointer.current == null) return;

        bool isPressing = Pointer.current.press.isPressed;

        // 2. Logic: Press ho raha hai aur UI ke upar nahi hai
        if (isPressing && !IsPointerOverUI())
        {
            _targetSpeed = moveSpeed;
            SetWegonSpeed(_targetSpeed);
        }
        else
        {
            _targetSpeed = 0;
            SetWegonSpeed(_targetSpeed);
        }

        // 3. Apply Speed (Smooth or Instant)
        if (useSmoothStop)
        {
            _splineFollower.followSpeed = Mathf.Lerp(_splineFollower.followSpeed, _targetSpeed, Time.deltaTime * smoothTime);
        }
        else
        {
            _splineFollower.followSpeed = _targetSpeed;
        }
    }

    private bool IsPointerOverUI()
    {
        // Ye check karta hai ki kya current touch/mouse UI element par hai
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void SetWegonSpeed(float speed)
    {
        foreach (var item in wegons)
        {
            item.followSpeed = speed;
        }
    }

    private void CheckLastWagonPassed()
    {
        if (_pendingGrids.Count == 0) return;

        // Aakhri wagon nikalne ka logic
        SplineFollower lastWagon = (wegons != null && wegons.Count > 0) ? wegons[wegons.Count - 1] : _splineFollower;

        // World position comparison sabse safe hai
        Vector3 lastWagonPos = lastWagon.transform.position;

        for (int i = _pendingGrids.Count - 1; i >= 0; i--)
        {
            CustomeGrid grid = _pendingGrids[i];
            Vector3 gridPos = grid.transform.position;

            // Distance check: Kya wagon grid se door ja chuki hai?
            float distanceToGrid = Vector3.Distance(lastWagonPos, gridPos);
            grid.distance = distanceToGrid;

            // Logic: Agar wagon grid ko cross kar chuki hai 
            // Hum dot product use karte hain check karne ke liye ki kya wagon aage nikal gayi hai
            Vector3 directionToWagon = (lastWagonPos - gridPos).normalized;
            Vector3 splineDirection = lastWagon.result.forward; // Spline jis taraf ja rahi hai

            float dot = Vector3.Dot(directionToWagon, splineDirection);

            // Agar dot > 0 hai, iska matlab wagon grid ke aage hai
            // 3.0f ek buffer distance hai safety ke liye
            if (dot > 0 && distanceToGrid > 3.0f)
            {
                grid.TriggerSplineUpdate();
                _pendingGrids.RemoveAt(i);
                Debug.Log("Last Wagon Passed: Spline Updated!");
            }
        }
    }



    public void RegisterPendingGrid(CustomeGrid grid)
    {
        if (!_pendingGrids.Contains(grid))
        {
            _pendingGrids.Add(grid);
        }
    }
}