using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPathTracker
{
    private PointPath _pointPath;

    private Transform _userTransform;

    private PathPointInfo _targetPathPointInfo;
    private PathPointInfo _closestPathPointInfo;
    private PathPointInfo _previousTargetPathPointInfo;

    private PathPointInfo _newPathPointInfo;

    private Vector3 _middlePointPosition;

    private float _sqrDistanceToPoint;
    private float _minSqrDistanceToPoint;

    private float _sqrTrackWidth;

    private int _basePointIndex;
    private int _targetPathPointIndex;
    private int _closestPathPointIndex;
    private int _shiftedPathPointIndex;

    private bool _isSimplified;

    public Action OnTargetPointChanged;

    public PathPointInfo TargetPoint => _targetPathPointInfo;
    public PathPointInfo ClosestPoint => _closestPathPointInfo;

    public Transform UserTransform => _userTransform;

    public bool IsUserInsideTrack => (_closestPathPointInfo.leftBorderPoint - _userTransform.position).GetPlanarSqrMagnitude(Axis.Y) < _sqrTrackWidth && (_closestPathPointInfo.rightBorderPoint - _userTransform.position).GetPlanarSqrMagnitude(Axis.Y) < _sqrTrackWidth && _closestPathPointIndex > 0;

    public PointPathTracker(PointPath pointPath, Transform userTransform, bool simplified = true)
    {
        _pointPath = pointPath;
        _userTransform = userTransform;

        _sqrTrackWidth = 1f; // Mathf.Pow(TrackConstructor.roadWidth, 2);

        _isSimplified = simplified;
    }

    public PathPointInfo UpdateTargetPoint(int increment = 0)
    {
        _minSqrDistanceToPoint = float.MaxValue;

        for (int i = _isSimplified ? _basePointIndex : 0; i < _pointPath.points.Length; i++)
        {
            _sqrDistanceToPoint = (_pointPath.points[i].Position - _userTransform.position).GetPlanarSqrMagnitude(Axis.Y);

            if (_sqrDistanceToPoint < _minSqrDistanceToPoint)
            {
                _minSqrDistanceToPoint = _sqrDistanceToPoint;

                _targetPathPointIndex = Mathf.Clamp(i + increment, 0, _pointPath.points.Length - 1);
                _closestPathPointIndex = i;

                _targetPathPointInfo = _pointPath.points[_targetPathPointIndex];
                _closestPathPointInfo = _pointPath.points[_closestPathPointIndex];
            }
            else if (_isSimplified)
            {
                break;
            }
        }

        if (_targetPathPointInfo != null && _previousTargetPathPointInfo != null)
        {
            if (_targetPathPointInfo.pointIndex != _previousTargetPathPointInfo.pointIndex && _targetPathPointInfo.pointIndex > increment)
            {
                if (_isSimplified)
                {
                    _basePointIndex = Mathf.Clamp(_basePointIndex + 1, 0, _pointPath.points.Length - 1);
                }

                if (OnTargetPointChanged != null)
                {
                    OnTargetPointChanged();
                }
            }

            _previousTargetPathPointInfo = _targetPathPointInfo;
        }

        return _targetPathPointInfo;
    }

    public PathPointInfo GetClosestPoint(int indexShift)
    {
        _shiftedPathPointIndex = Mathf.Clamp(_closestPathPointIndex + indexShift, 0, _pointPath.points.Length - 1);

        return _pointPath.points[_shiftedPathPointIndex];
    }

    public Vector3 GetMiddlePoint(int rangeStartIndex, int rangeEndIndex)
    {
        _middlePointPosition = Vector3.zero;

        for (int i = rangeStartIndex; i <= rangeEndIndex; i++)
        {
            _middlePointPosition += _pointPath.points[Math.Clamp(i, 0, _pointPath.points.Length)].Position;
        }

        return _middlePointPosition / (rangeEndIndex - rangeStartIndex + 1f);
    }

    /*
    public PathPointInfo GetClosestPoint()
    {
        _minSqrDistanceToPoint = float.MaxValue;

        for (int i = 0; i < _pathPoints.Count; i++)
        {
            _sqrDistanceToPoint = (_pathPoints[i].position - _userTransform.position).sqrMagnitude;

            if (_sqrDistanceToPoint < _minSqrDistanceToPoint)
            {
                _minSqrDistanceToPoint = _sqrDistanceToPoint;

                _closestPathPointIndex = i;

                _closestPathPoint = _pathPoints[i];
            }
        }

        return new PathPointInfo(_closestPathPoint, _closestPathPointIndex);
    }
    */
}
