using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputTrackingType { All, EmptySpace }

public class InputManager : Service<InputManager>
{
    public InputTrackingType trackingMode;
    [Space]
    public float tapTimeout;

    private static Vector3 _touchPivot;

    private static Vector3 _previousTouchPosition;
    private static Vector3 _slideDelta;

    private static float _touchTime;
    private static float _releaseTime;

    private static int _tapCounter;

    private static bool _isTouchPresent;

    private static bool _isTouchRegistered;
    private static bool _isTapRegistered;
    private static bool _isDoubleTapRegistered;

    public static float timeSinceTouchPresent => Time.timeSinceLevelLoad - _touchTime;
    public static float timeSinceTouchAbsent => Time.timeSinceLevelLoad - _releaseTime;

    public static Vector3 touchPosition => Input.mousePosition;
    public static Vector3 slideDelta => _slideDelta;

    public static bool touchPresent => _isTouchPresent;

    public static bool touch { get { if (!_isTouchRegistered && _isTouchPresent) { return _isTouchRegistered = true; } return false; } }
    public static bool tap { get { if (!_isTapRegistered && _tapCounter == 1) { return _isTapRegistered = true; } return false; } }
    public static bool doubleTap { get { if (!_isDoubleTapRegistered && _tapCounter == 1 && _isTouchPresent) { return _isDoubleTapRegistered = true; } return false; } }

    protected override void Awake()
    {
        base.Awake();

        GameManager.OnGameSceneLoaded += Reset;
    }

    private void FixedUpdate()
    {
        if (trackingMode == InputTrackingType.All ? Input.GetMouseButton(0) : UIManager.EmptySpaceInteraction)
        {
            if (!_isTouchPresent)
            {
                _touchPivot = Input.mousePosition;
                _touchTime = Time.timeSinceLevelLoad;
                _previousTouchPosition = _touchPivot;

                _isTouchPresent = true;
            }
        }
        else
        {
            if (_isTouchPresent)
            {
                _releaseTime = Time.timeSinceLevelLoad;
            }
            
            if (Time.timeSinceLevelLoad < _touchTime + tapTimeout)
            {
                if (_isTouchPresent)
                {
                    _tapCounter++;
                }
            }
            else
            {
                _tapCounter = 0;

                _isTapRegistered = false;
                _isDoubleTapRegistered = false;
            }

            _isTouchPresent = false;
            _isTouchRegistered = false;
        }

        if (_isTouchPresent)
        {
            _slideDelta = Input.mousePosition - _previousTouchPosition;
        }

        _previousTouchPosition = Input.mousePosition;
    }

    private void Reset()
    {
        _touchPivot = Vector3.zero;
        _previousTouchPosition = Vector3.zero;
        _slideDelta = Vector3.zero;

        _touchTime = 0;
        _releaseTime = 0;

        _isTouchPresent = false;
    }
}
