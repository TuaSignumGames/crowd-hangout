using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SuspensionWheel))]
public class SuspensionWheelEditor : Editor
{
    public SuspensionWheelGuiSettings _gizmos;

    private SuspensionWheel _suspensionWheel;

    private SuspensionSpringData _springSettings;
    private SuspensionMotionData _motionPartSettings;
    private SuspensionWheelData _wheelSettings;
    private SuspensionWheelGuiSettings _guiSettings;

    private Transform _motionPartTransform;

    private Transform _compressionTravelPointPivotTransform;
    private Transform _extensionTravelPointPivotTransform;
    private Transform _compressionTravelPointTransform;
    private Transform _extensionTravelPointTransform;

    private Vector3 _springDirection;
    private Vector3 _armDirection;

    private Vector3 _travelPointsOrigin;
    private Vector3 _compressionTravelPoint;
    private Vector3 _extensionTravelPoint;

    private Vector3 _torsionTravelPointLocalEulers;
    private Vector3 _torsionCompressionTravelPointDirection;
    private Vector3 _torsionExtensionTravelPointDirection;

    private float _rayLength;

    private float _baseMotionPartCoordinate;

    private bool _isSpringLengthDefined;

    private void OnSceneGUI()
    {
        _suspensionWheel = (SuspensionWheel)target;

        _springSettings = _suspensionWheel.SpringSettings;
        _motionPartSettings = _suspensionWheel.MotionPartSettings;
        _wheelSettings = _suspensionWheel.WheelSettings;
        _guiSettings = _suspensionWheel.GuiSettings;

        _motionPartTransform = _motionPartSettings.motionPartTransform;

        switch (_springSettings.springType)
        {
            case SpringType.Compression:

                DrawCompressionSpringGUI();
                break;

            case SpringType.Torsion:

                DrawTorsionSpringGUI();
                break;
        }

        if (_suspensionWheel.VehicleRigidbody && _wheelSettings.wheelContainerTransform)
        {
            DrawWireDisc(_wheelSettings.wheelContainerTransform.position, _wheelSettings.wheelContainerTransform.right, _guiSettings.pivotPointSettings.radius, _guiSettings.pivotPointSettings.thickness, _guiSettings.pivotPointSettings.color);
            DrawWireDisc(_wheelSettings.wheelContainerTransform.position, _wheelSettings.wheelContainerTransform.right, _wheelSettings.wheelRadius, _guiSettings.pivotPointSettings.thickness, _guiSettings.pivotPointSettings.color);

            DrawWireDisc(_wheelSettings.wheelContainerTransform.position - _suspensionWheel.VehicleRigidbody.transform.up * _wheelSettings.wheelRadius, _suspensionWheel.VehicleRigidbody.transform.up, _guiSettings.groundPointSettings.radius, _guiSettings.groundPointSettings.thickness, _guiSettings.groundPointSettings.color);

            DrawLine(_wheelSettings.wheelContainerTransform.position, _wheelSettings.wheelContainerTransform.position - _suspensionWheel.VehicleRigidbody.transform.up * _suspensionWheel.GetRayDistance(), _guiSettings.radiusLineSettings.thickness, _guiSettings.radiusLineSettings.color);
        }
    }

    private void DrawCompressionSpringGUI()
    {
        if (_springSettings.bodyJointPoint && _springSettings.motionPartJointPoint)
        {
            _springDirection = _springSettings.bodyJointPoint.position.GetDirectionTo(_springSettings.motionPartJointPoint.position);

            DrawWireDisc(_springSettings.bodyJointPoint.position, _springDirection, _guiSettings.bodyPointSettings.radius, _guiSettings.bodyPointSettings.thickness, _guiSettings.bodyPointSettings.color);
            DrawWireDisc(_springSettings.motionPartJointPoint.position, _springDirection, _guiSettings.wheelPointSettings.radius, _guiSettings.wheelPointSettings.thickness, _guiSettings.wheelPointSettings.color);

            DrawLine(_springSettings.bodyJointPoint.position, _springSettings.motionPartJointPoint.position, _guiSettings.springLineSettings.thickness, _guiSettings.springLineSettings.color);

            _travelPointsOrigin = _suspensionWheel.IsInitialized ? _springSettings.bodyJointPoint.position + _springDirection * _suspensionWheel.GetRestSpringLength() : _springSettings.motionPartJointPoint.position;

            _compressionTravelPoint = _travelPointsOrigin - _springDirection * _springSettings.travelLength;
            _extensionTravelPoint = _travelPointsOrigin + _springDirection * _springSettings.travelLength;

            DrawWireDisc(_extensionTravelPoint, _springDirection, _guiSettings.travelPointSettings.radius, _guiSettings.travelPointSettings.thickness, _guiSettings.travelPointSettings.color);
            DrawWireDisc(_compressionTravelPoint, _springDirection, _guiSettings.travelPointSettings.radius, _guiSettings.travelPointSettings.thickness, _guiSettings.travelPointSettings.color);

            DrawDottedLine(_compressionTravelPoint, _extensionTravelPoint, _guiSettings.travelLineSettings.thickness, _guiSettings.travelLineSettings.color);
        }
    }

    private void DrawTorsionSpringGUI()
    {
        if (_springSettings.bodyJointPoint && _wheelSettings.wheelContainerTransform)
        {
            _armDirection = _springSettings.bodyJointPoint.position.GetDirectionTo(_wheelSettings.wheelContainerTransform.position);

            DrawWireDisc(_springSettings.bodyJointPoint.position, _springSettings.bodyJointPoint.right, _guiSettings.bodyPointSettings.radius, _guiSettings.bodyPointSettings.thickness, _guiSettings.bodyPointSettings.color);

            DrawLine(_springSettings.bodyJointPoint.position + _armDirection * _guiSettings.bodyPointSettings.radius, _wheelSettings.wheelContainerTransform.position - _armDirection * _guiSettings.pivotPointSettings.radius, _guiSettings.springLineSettings.thickness, _guiSettings.springLineSettings.color);

            _torsionTravelPointLocalEulers = new Vector3(_motionPartSettings.localMotionAxis == Axis.X ? _springSettings.travelLength : 0, _motionPartSettings.localMotionAxis == Axis.Y ? _springSettings.travelLength : 0, _motionPartSettings.localMotionAxis == Axis.Z ? _springSettings.travelLength : 0);

            if (_suspensionWheel.IsInitialized)
            {
                _compressionTravelPointPivotTransform = _motionPartTransform.FindChildWithName("TP_C");

                if (_compressionTravelPointPivotTransform)
                {
                    _compressionTravelPointPivotTransform.SetParent(_motionPartTransform.parent);
                }
                else
                {
                    _compressionTravelPointPivotTransform = _motionPartTransform.parent.FindChildWithName("TP_C");
                }

                _compressionTravelPointTransform = _compressionTravelPointPivotTransform.GetChild(0);

                _extensionTravelPointPivotTransform = _motionPartTransform.FindChildWithName("TP_E");

                if (_extensionTravelPointPivotTransform)
                {
                    _extensionTravelPointPivotTransform.SetParent(_motionPartTransform.parent);
                }
                else
                {
                    _extensionTravelPointPivotTransform = _motionPartTransform.parent.FindChildWithName("TP_E");
                }

                _extensionTravelPointTransform = _extensionTravelPointPivotTransform.GetChild(0);

                _compressionTravelPointPivotTransform.localEulerAngles = _torsionTravelPointLocalEulers.normalized * _suspensionWheel.GetBaseMotionPartCoordinate() + _torsionTravelPointLocalEulers;
                _extensionTravelPointPivotTransform.localEulerAngles = _torsionTravelPointLocalEulers.normalized * _suspensionWheel.GetBaseMotionPartCoordinate() - _torsionTravelPointLocalEulers;
            }
            else
            {
                _compressionTravelPointPivotTransform = _motionPartTransform.parent.FindChildWithName("TP_C");

                if (_compressionTravelPointPivotTransform)
                {
                    GameObject.DestroyImmediate(_compressionTravelPointPivotTransform.gameObject);
                }

                if (!_motionPartTransform.FindChildWithName("TP_C"))
                {
                    _compressionTravelPointPivotTransform = new GameObject("TP_C").transform;
                    _compressionTravelPointPivotTransform.SetParent(_motionPartTransform);

                    _compressionTravelPointPivotTransform.localPosition = Vector3.zero;
                    _compressionTravelPointPivotTransform.localEulerAngles = Vector3.zero;

                    _compressionTravelPointTransform = new GameObject("Point").transform;
                    _compressionTravelPointTransform.SetParent(_compressionTravelPointPivotTransform);

                    _compressionTravelPointTransform.position = _wheelSettings.wheelContainerTransform.position;
                }
                else
                {
                    _compressionTravelPointPivotTransform = _motionPartTransform.FindChildWithName("TP_C");
                    _compressionTravelPointTransform = _compressionTravelPointPivotTransform.GetChild(0);
                }

                _extensionTravelPointPivotTransform = _motionPartTransform.parent.FindChildWithName("TP_C");

                if (_extensionTravelPointPivotTransform)
                {
                    GameObject.DestroyImmediate(_extensionTravelPointPivotTransform.gameObject);
                }

                if (!_motionPartTransform.FindChildWithName("TP_E"))
                {
                    _extensionTravelPointPivotTransform = new GameObject("TP_E").transform;
                    _extensionTravelPointPivotTransform.SetParent(_motionPartTransform);

                    _extensionTravelPointPivotTransform.localPosition = Vector3.zero;
                    _extensionTravelPointPivotTransform.localEulerAngles = Vector3.zero;

                    _extensionTravelPointTransform = new GameObject("Point").transform;
                    _extensionTravelPointTransform.SetParent(_extensionTravelPointPivotTransform);

                    _extensionTravelPointTransform.position = _wheelSettings.wheelContainerTransform.position;
                }
                else
                {
                    _extensionTravelPointPivotTransform = _motionPartTransform.FindChildWithName("TP_E");
                    _extensionTravelPointTransform = _extensionTravelPointPivotTransform.GetChild(0);
                }

                _compressionTravelPointPivotTransform.localEulerAngles = _torsionTravelPointLocalEulers;
                _extensionTravelPointPivotTransform.localEulerAngles = -_torsionTravelPointLocalEulers;
            }

            _torsionCompressionTravelPointDirection = _compressionTravelPointPivotTransform.position.GetDirectionTo(_compressionTravelPointTransform.position);
            _torsionExtensionTravelPointDirection = _extensionTravelPointPivotTransform.position.GetDirectionTo(_extensionTravelPointTransform.position);

            DrawWireDisc(_compressionTravelPointTransform.position, _motionPartTransform.right, _guiSettings.travelPointSettings.radius, _guiSettings.travelPointSettings.thickness, _guiSettings.travelPointSettings.color);
            DrawDottedLine(_compressionTravelPointPivotTransform.position + _torsionCompressionTravelPointDirection * _guiSettings.bodyPointSettings.radius, _compressionTravelPointTransform.position - _torsionCompressionTravelPointDirection * _guiSettings.travelPointSettings.radius, _guiSettings.travelLineSettings.thickness, _guiSettings.travelLineSettings.color);

            DrawWireDisc(_extensionTravelPointTransform.position, _motionPartTransform.right, _guiSettings.travelPointSettings.radius, _guiSettings.travelPointSettings.thickness, _guiSettings.travelPointSettings.color);
            DrawDottedLine(_extensionTravelPointPivotTransform.position + _torsionExtensionTravelPointDirection * _guiSettings.bodyPointSettings.radius, _extensionTravelPointTransform.position - _torsionExtensionTravelPointDirection * _guiSettings.travelPointSettings.radius, _guiSettings.travelLineSettings.thickness, _guiSettings.travelLineSettings.color);
        }
    }

    private void DrawWireDisc(Vector3 center, Vector3 normal, float radius, float thickness, Color color)
    {
        Handles.color = color;
        Handles.DrawWireDisc(center, normal, radius, thickness);
    }

    private void DrawQuad()
    {

    }

    private void DrawLine(Vector3 from, Vector3 to, float thickness, Color color)
    {
        Handles.color = color;
        Handles.DrawLine(from, to, thickness);
    }

    private void DrawDottedLine(Vector3 from, Vector3 to, float screenSpaceSize, Color color)
    {
        Handles.color = color;
        Handles.DrawDottedLine(from, to, screenSpaceSize);
    }

    private void DrawWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, float thickness, Color color)
    {
        Handles.color = color;
        Handles.DrawWireArc(center, normal, from, angle, radius, thickness);
    }
}
