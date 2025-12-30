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
}