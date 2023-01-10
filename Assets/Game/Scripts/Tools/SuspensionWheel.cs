using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpringType { Compression, Torsion }

public class SuspensionWheel : MonoBehaviour
{
    [SerializeField] private Rigidbody _vehicleRigidbody;
    [SerializeField] private SuspensionSpringData _springSettings;
    [SerializeField] private SuspensionMotionData _motionPartSettings;
    [SerializeField] private SuspensionWheelData _wheelSettings;
    [Space]
    [SerializeField] private SuspensionWheelGuiSettings _gizmos;

    private Transform _motionPartTransform;

    private RaycastHit _raycastInfo;

    private Vector3 _wheelLocalVelocity;

    private Vector3 _torqueForce;
    private Vector3 _frictionForce;

    private float _torqueFactor;

    private float _rayDistance;
    private float _sqrWheelRadius;
    private float _sqrDoubleWheelRadius;

    private float _elasticForceValue;
    private float _dampingForceValue;
    private float _torqueForceValue;

    private float _currentSpringLength;

    private float _restSpringLength;
    private float _compressedSpringLength;
    private float _extendedSpringLength;
    private float _wheelDeepening;

    private float _previousSpringLength;
    private float _springAxialVelocity;

    private float _compressionSpringInclineAngle;
    private float _compressionSpringInclineCos;

    private float _currentSpringAngle;

    private float _restSpringAngle;
    private float _compressedSpringAngle;
    private float _extendedSpringAngle;

    private float _previousSpringAngle;
    private float _springAngularVelocity;

    private float _torsionSpringContactHypotenuse;

    private float _currentTorsionSpringLeg;

    private float _baseMotionPartCoordinate;

    private float _landingTime;
    private float _unlandingTime;
    private float _unstickingTime;

    private bool _isSpringInclined;
    private bool _isWheelInitialized;
    private bool _isTorqueApplying;
    private bool _isSticky;
    private bool _isGrounded;

    public Action<Vector3> OnGroundIgnored;

    public Rigidbody VehicleRigidbody => _vehicleRigidbody;
    public SuspensionSpringData SpringSettings => _springSettings;
    public SuspensionMotionData MotionPartSettings => _motionPartSettings;
    public SuspensionWheelData WheelSettings => _wheelSettings;
    public SuspensionWheelGuiSettings GuiSettings => _gizmos;

    public Vector3 GroundNormal => _raycastInfo.normal;

    public float AirTime => _unlandingTime > 0 ? Time.timeSinceLevelLoad - _unlandingTime : 0;

    public bool IsInitialized => _isWheelInitialized;
    public bool IsGrounded => _isGrounded;

    public void Initialize(int groundLayer = 0, float torqueForceValue = 0)
    {
        _wheelSettings.groundLayer = groundLayer;

        _torqueForceValue = torqueForceValue;

        _motionPartTransform = _motionPartSettings.motionPartTransform;

        if (_springSettings.springType == SpringType.Compression)
        {
            InitializeCompressionSpring();
        }
        else
        {
            InitializeTorsionSpring();
        }

        _rayDistance = _wheelSettings.wheelRadius * 2f;
        _sqrWheelRadius = _wheelSettings.wheelRadius * _wheelSettings.wheelRadius;
        _sqrDoubleWheelRadius = _sqrWheelRadius * 4f;

        _isWheelInitialized = true;
    }

    private void FixedUpdate()
    {
        if (Raycast(_wheelSettings.groundLayer, _rayDistance))
        {
            if (_springSettings.springType == SpringType.Compression)
            {
                _previousSpringLength = _currentSpringLength;

                _wheelDeepening = _wheelSettings.wheelRadius - _raycastInfo.distance;
                _currentSpringLength = Mathf.Clamp(_currentSpringLength - _wheelDeepening / _compressionSpringInclineCos, _compressedSpringLength, _extendedSpringLength);

                _springAxialVelocity = (_currentSpringLength - _previousSpringLength) / Time.fixedDeltaTime;

                _elasticForceValue = _springSettings.stiffnessFactor * (_restSpringLength - _currentSpringLength);
                _dampingForceValue = _springSettings.dampingFactor * -_springAxialVelocity;

                if (_currentSpringLength <= _restSpringLength || _isSticky)
                {
                    _vehicleRigidbody.AddForceAtPosition((_elasticForceValue + _dampingForceValue) * transform.up, _raycastInfo.point);
                }

                _isGrounded = _currentSpringLength <= _extendedSpringLength;
            }
            else
            {
                _previousSpringAngle = _currentSpringAngle;

                _currentTorsionSpringLeg = Mathf.Sin(_currentSpringAngle * Mathf.Deg2Rad) * _torsionSpringContactHypotenuse;

                _wheelDeepening = _wheelSettings.wheelRadius - _raycastInfo.distance;
                _currentSpringAngle = Mathf.Clamp(GetSignedAngle(Mathf.Asin(_currentTorsionSpringLeg + _wheelDeepening) * Mathf.Rad2Deg), _extendedSpringAngle, _compressedSpringAngle);

                _springAngularVelocity = (_currentSpringAngle - _previousSpringAngle) / Time.fixedDeltaTime;

                _elasticForceValue = _springSettings.stiffnessFactor * (_currentSpringAngle - _restSpringAngle);
                _dampingForceValue = _springSettings.dampingFactor * _springAngularVelocity;

                if (_currentSpringAngle >= _restSpringAngle || _isSticky)
                {
                    _vehicleRigidbody.AddForceAtPosition((_elasticForceValue + _dampingForceValue) * transform.up, _raycastInfo.point);
                }

                _isGrounded = _currentSpringAngle >= _extendedSpringAngle;
            }

            if (_isGrounded)
            {
                UpdateMotionPartPosition();

                if (_landingTime == 0)
                {
                    _landingTime = Time.timeSinceLevelLoad;
                    _unstickingTime = _landingTime + _wheelSettings.unstickingDelay;
                }

                if (_isSticky && Time.timeSinceLevelLoad > _unstickingTime)
                {
                    _isSticky = false;
                }

                _unlandingTime = 0;
            }

            _wheelLocalVelocity = _wheelSettings.wheelContainerTransform.InverseTransformDirection(_vehicleRigidbody.GetPointVelocity(_raycastInfo.point));

            _torqueForce = _torqueForceValue * _wheelSettings.wheelContainerTransform.forward * _torqueFactor;
            _frictionForce = _wheelSettings.frictionFactor * _wheelLocalVelocity.x * -_wheelSettings.wheelContainerTransform.right;

            _vehicleRigidbody.AddForceAtPosition(_torqueForce + _frictionForce, _raycastInfo.point);
        }
        else
        {
            _currentSpringLength = _extendedSpringLength;
            _currentSpringAngle = _extendedSpringAngle;

            UpdateMotionPartPosition();

            _isGrounded = false;
            _isSticky = true;

            if (_unlandingTime == 0)
            {
                _unlandingTime = Time.timeSinceLevelLoad;
            }

            _landingTime = 0;

            if (Raycast(_wheelSettings.wheelTransform.position + _vehicleRigidbody.transform.up * 5f, -_vehicleRigidbody.transform.up, _wheelSettings.groundLayer, 5f))
            {
                if (_springSettings.springType == SpringType.Compression)
                {
                    _currentSpringLength = _compressedSpringLength;
                }
                else
                {
                    _currentSpringAngle = _compressedSpringAngle;
                }

                UpdateMotionPartPosition();

                if (OnGroundIgnored != null)
                {
                    OnGroundIgnored(_raycastInfo.point);

                    _vehicleRigidbody.velocity -= Vector3.Project(_vehicleRigidbody.velocity, _raycastInfo.normal) * 0.5f;

                    _isGrounded = true;
                }
            }
        }
    }

    private void InitializeCompressionSpring()
    {
        _currentSpringLength = (_springSettings.bodyJointPoint.position - _springSettings.motionPartJointPoint.position).magnitude;

        _restSpringLength = _currentSpringLength;
        _compressedSpringLength = _restSpringLength - _springSettings.travelLength;
        _extendedSpringLength = _restSpringLength + _springSettings.travelLength;

        _compressionSpringInclineAngle = Vector3.Angle(_wheelSettings.wheelContainerTransform.up, _springSettings.bodyJointPoint.position - _springSettings.motionPartJointPoint.position);

        _isSpringInclined = _compressionSpringInclineAngle > 0.5f;

        if (_isSpringInclined)
        {
            _compressionSpringInclineCos = Mathf.Cos(_compressionSpringInclineAngle * Mathf.Deg2Rad);
        }

        _baseMotionPartCoordinate = GetBaseMotionPartCoordinate();
    }

    private void InitializeTorsionSpring()
    {
        _baseMotionPartCoordinate = GetSignedAngle(GetBaseMotionPartCoordinate());

        _currentSpringAngle = _baseMotionPartCoordinate;

        _restSpringAngle = _baseMotionPartCoordinate;
        _compressedSpringAngle = _restSpringAngle + _springSettings.travelLength;
        _extendedSpringAngle = _restSpringAngle - _springSettings.travelLength;

        _torsionSpringContactHypotenuse = (_wheelSettings.wheelContainerTransform.position - transform.up * _wheelSettings.wheelRadius - _springSettings.bodyJointPoint.position).magnitude;
    }

    private void UpdateMotionPartPosition()
    {
        if (_springSettings.springType == SpringType.Compression)
        {
            switch (_motionPartSettings.localMotionAxis)
            {
                case Axis.X:

                    if (_motionPartSettings.negativeDirection)
                    {
                        _motionPartTransform.localPosition = new Vector3(_baseMotionPartCoordinate - _currentSpringLength + _restSpringLength, 0, 0);
                    }
                    else
                    {
                        _motionPartTransform.localPosition = new Vector3(_baseMotionPartCoordinate + _currentSpringLength - _restSpringLength, 0, 0);
                    }
                    break;

                case Axis.Y:

                    if (_motionPartSettings.negativeDirection)
                    {
                        _motionPartTransform.localPosition = new Vector3(0, _baseMotionPartCoordinate - _currentSpringLength + _restSpringLength, 0);
                    }
                    else
                    {
                        _motionPartTransform.localPosition = new Vector3(0, _baseMotionPartCoordinate + _currentSpringLength - _restSpringLength, 0);
                    }
                    break;

                case Axis.Z:

                    if (_motionPartSettings.negativeDirection)
                    {
                        _motionPartTransform.localPosition = new Vector3(0, 0, _baseMotionPartCoordinate - _currentSpringLength + _restSpringLength);
                    }
                    else
                    {
                        _motionPartTransform.localPosition = new Vector3(0, 0, _baseMotionPartCoordinate + _currentSpringLength - _restSpringLength);
                    }
                    break;
            }
        }
        else
        {
            switch (_motionPartSettings.localMotionAxis)
            {
                case Axis.X:

                    _motionPartTransform.localEulerAngles = new Vector3(_motionPartSettings.negativeDirection ? -_currentSpringAngle : _currentSpringAngle, 0, 0);
                    break;

                case Axis.Y:

                    _motionPartTransform.localEulerAngles = new Vector3(0, _motionPartSettings.negativeDirection ? -_currentSpringAngle : _currentSpringAngle, 0);
                    break;

                case Axis.Z:

                    _motionPartTransform.localEulerAngles = new Vector3(0, 0, _motionPartSettings.negativeDirection ? -_currentSpringAngle : _currentSpringAngle);
                    break;
            }
        }
    }

    public void SetTorque(float torqueFactor)
    {
        _torqueFactor = torqueFactor;
    }

    public float GetRestSpringLength()
    {
        if (!_isWheelInitialized)
        {
            return (_springSettings.bodyJointPoint.position - _springSettings.motionPartJointPoint.position).magnitude;
        }

        return _restSpringLength;
    }

    public void SetSticky(bool isSticky)
    {
        if (isSticky)
        {
            _unstickingTime = Time.timeSinceLevelLoad + _wheelSettings.unstickingDelay;
        }

        _isSticky = isSticky;
    }

    public float GetRestSpringAngle()
    {
        if (!_isWheelInitialized)
        {
            return GetBaseMotionPartCoordinate();
        }

        return _restSpringAngle;
    }

    public float GetBaseMotionPartCoordinate()
    {
        if (!_isWheelInitialized)
        {
            if (_springSettings.springType == SpringType.Compression)
            {
                return _motionPartSettings.localMotionAxis == Axis.X ? _motionPartTransform.localPosition.x : (_motionPartSettings.localMotionAxis == Axis.Y ? _motionPartTransform.localPosition.y : _motionPartTransform.localPosition.z);
            }
            else
            {
                return GetSignedAngle(_motionPartSettings.localMotionAxis == Axis.X ? _motionPartTransform.localEulerAngles.x : (_motionPartSettings.localMotionAxis == Axis.Y ? _motionPartTransform.localEulerAngles.y : _motionPartTransform.localEulerAngles.z));
            }
        }

        return _baseMotionPartCoordinate;
    }

    public float GetRayDistance()
    {
        if (!_isWheelInitialized)
        {
            return _wheelSettings.wheelRadius * 2f;
        }

        return _rayDistance;
    }

    private float GetSignedAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    private bool Raycast(Vector3 origin, Vector3 direction, int layerMask = 0, float maxDistance = float.MaxValue)
    {
        return Physics.Raycast(origin, direction, out _raycastInfo, maxDistance, 1 << layerMask);
    }

    private bool Raycast(int layerMask = 0, float maxDistance = float.MaxValue)
    {
        return Physics.Raycast(_wheelSettings.wheelContainerTransform.position, -_vehicleRigidbody.transform.up, out _raycastInfo, maxDistance, 1 << layerMask);
    }
}

[Serializable]
public struct SuspensionSpringData
{
    public SpringType springType;
    [Space]
    public float stiffnessFactor;
    public float dampingFactor;
    [Space]
    public float travelLength;
    [Space]
    public Transform bodyJointPoint;
    public Transform motionPartJointPoint;
}

[Serializable]
public struct SuspensionMotionData
{
    public Axis localMotionAxis;
    public bool negativeDirection;
    [Space]
    public Transform motionPartTransform;
}

[Serializable]
public struct SuspensionWheelData
{
    public float wheelRadius;
    [Space]
    public float frictionFactor;
    public float unstickingDelay;
    [Space]
    public int groundLayer;
    public Axis rotationAxis;
    [Space]
    public Transform wheelContainerTransform;
    public Transform wheelTransform;
}

[Serializable]
public struct SuspensionWheelGuiSettings
{
    [Header("Spring")]
    public GuiPointSettings bodyPointSettings;
    public GuiPointSettings wheelPointSettings;
    public GuiLineSettings springLineSettings;
    [Space]
    public GuiPointSettings travelPointSettings;
    public GuiLineSettings travelLineSettings;
    [Header("Wheel")]
    public GuiPointSettings pivotPointSettings;
    public GuiPointSettings groundPointSettings;
    public GuiLineSettings radiusLineSettings;
}

[Serializable]
public struct GuiLineSettings
{
    public float thickness;
    public Color color;
}

[Serializable]
public struct GuiPointSettings
{
    public float radius;
    public float thickness;
    public Color color;
}