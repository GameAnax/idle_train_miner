using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Dreamteck.Splines;
using System.Collections.Generic;
using Dreamteck.Splines.Examples;
using System;

// [RequireComponent(typeof(SplineFollower))]
// [RequireComponent(typeof(TrainLoopHandler))]
public class TrainSplineDriver : MonoBehaviour
{
    private TrainLoopHandler trainLoopHandler;
    [SerializeField]
    private SplineFollower _splineFollower;
    public ModularGridAligner modularGridAligner;
    public SplineFollower GetSplineFollower => _splineFollower;
    public TrainLoopHandler GetTrainLoopHandler => trainLoopHandler;
    private float _targetSpeed;
    private float currentDistance = 0;
    private List<CustomeGrid> _pendingGrids = new List<CustomeGrid>();



    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool useSmoothStop = true;
    [SerializeField] private float manualDistance = 0;
    [SerializeField] private float smoothTime = 0.5f;

    public float carriageSpacing = 10f;

    public List<Boggy> boggies;

    void Awake()
    {
        trainLoopHandler = GetComponent<TrainLoopHandler>();
        _splineFollower = GetComponent<SplineFollower>();
        // // Shuruat me speed 0 rakhenge
        _splineFollower.followSpeed = 0;
    }


    void Update()
    {
        HandleInput();
        CheckLastWagonPassed();
        MoveBogeys();
    }
    void LateUpdate()
    {
        // MoveBogeys();
    }

    public bool isMainComplete;
    public float bridgeDistance;
    public float targetDistanceForThisBogey;

    public void MoveBogeys()
    {
        float leaderCurrentDist = (float)_splineFollower.CalculateLength() * (float)_splineFollower.result.percent;
        for (int i = 0; i < boggies.Count; i++)
        {
            var boggyItem = boggies[i];
            targetDistanceForThisBogey = leaderCurrentDist - ((i + 1) * boggyItem.space);

            // Agar targetDistance negative hai, iska matlab bogey abhi purani spline par honi chahiye
            if (targetDistanceForThisBogey < 0 || isMainComplete)
            {
                // Yahan aap logic laga sakte hain ki bogey purani spline ke end par ruk jaye
                // Ya phir use purani spline par hi movement karwayein
                if (trainLoopHandler.previewsCombinedSplineComputer != null)
                {
                    // Debug.Log("Call When Calculation Negitive");
                    if (boggyItem.splineFollower.spline != trainLoopHandler.previewsCombinedSplineComputer)
                    {
                        boggyItem.splineFollower.spline = trainLoopHandler.previewsCombinedSplineComputer;
                        boggyItem.splineFollower.RebuildImmediate();
                    }
                    isMainComplete = false;
                    float prevLength = (float)trainLoopHandler.previewsCombinedSplineComputer.CalculateLength();
                    bridgeDistance = prevLength + targetDistanceForThisBogey;
                    boggyItem.splineFollower.SetDistance(bridgeDistance);
                }
            }
            else
            {
                if (boggyItem.splineFollower.spline != _splineFollower.spline)
                {
                    // Debug.Log($"Train and Boggy spline not same at {targetDistanceForThisBogey}, and Train distance - {leaderCurrentDist}");
                    boggyItem.splineFollower.spline = _splineFollower.spline;
                    boggyItem.splineFollower.RebuildImmediate();
                }
                boggyItem.splineFollower.SetDistance(targetDistanceForThisBogey);
            }
        }
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
            // SetWegonSpeed(_targetSpeed);
        }
        else
        {
            _targetSpeed = 0;
            if (GameManager.instance.crusherArea != null)
            {
                GameManager.instance.crusherArea.TransferDebries();
            }
            // SetWegonSpeed(_targetSpeed);
        }

        // 3. Apply Speed (Smooth or Instant)
        if (useSmoothStop)
        {
            _splineFollower.followSpeed = Mathf.Lerp(_splineFollower.followSpeed, _targetSpeed, Time.deltaTime * smoothTime);
            // currentDistance += _targetSpeed * Time.deltaTime;

            // float totalLength = _splineFollower.spline.CalculateLength();
            // if (currentDistance > totalLength) currentDistance %= totalLength;

            // _splineFollower.SetDistance(currentDistance);
        }
        else
        {
            _splineFollower.followSpeed = _targetSpeed;
            // currentDistance += _targetSpeed * Time.deltaTime;

            // float totalLength = _splineFollower.spline.CalculateLength();
            // if (currentDistance > totalLength) currentDistance %= totalLength;

            // _splineFollower.SetDistance(currentDistance);
        }
    }

    private bool IsPointerOverUI()
    {
        // Ye check karta hai ki kya current touch/mouse UI element par hai
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }



    private void CheckLastWagonPassed()
    {
        if (_pendingGrids.Count == 0) return;

        // Aakhri wagon nikalne ka logic
        SplineFollower lastWagon = (boggies != null && boggies.Count > 0) ? boggies[boggies.Count - 1].splineFollower : _splineFollower;

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
            if (dot > 0 && distanceToGrid > 2.0f)
            {
                grid.OnGridDestroyed();
                grid.TriggerSplineUpdate();
                _pendingGrids.RemoveAt(i);
                modularGridAligner.StartGeneration();
                // Debug.Log("Last Wagon Passed: Spline Updated!");
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
    public void UpdateSpline()
    {
        foreach (var item in boggies)
        {
            item.splineFollower.spline = trainLoopHandler.previewsCombinedSplineComputer;
            item.splineFollower.RebuildImmediate();
            item.splineFollower.SetDistance(bridgeDistance);
        }
    }
    public void UpdateSpeed(int amount)
    {
        moveSpeed += amount;
    }
    [EasyButtons.Button]
    private void CalculatePosition()
    {
        float leaderCurrentDist = (float)_splineFollower.CalculateLength() * (float)_splineFollower.result.percent;
        for (int i = 0; i < boggies.Count; i++)
        {
            var boggyItem = boggies[i];
            float targetDistanceForThisBogey = leaderCurrentDist - ((i + 1) * boggyItem.space);
            boggyItem.splineFollower.SetDistance(targetDistanceForThisBogey);
        }
    }
}