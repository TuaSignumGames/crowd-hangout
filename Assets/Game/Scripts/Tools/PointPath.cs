using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPath
{
    public PathPointInfo[] points;

    private List<Transform> _pathPointTransforms;

    private PathPointInfo[] _requestedPointsRange;

    private int _startPointsRangeIndex;

    public PointPath(IList<Transform> pathPointTransforms)
    {
        points = new PathPointInfo[pathPointTransforms.Count];

        _pathPointTransforms = new List<Transform>(pathPointTransforms);

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new PathPointInfo(pathPointTransforms[i], i, i == 0 ? null : points[i - 1]);
        }
    }

    public PathPointInfo GetPoint(Transform pointTransform, int indexShift = 0)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].pointTransform == pointTransform)
            {
                return points[Mathf.Clamp(i + indexShift, 0, points.Length - 1)];
            }
        }

        return null;
    }

    public Transform GetPointTransform(int pointIndex)
    {
        pointIndex = Mathf.Clamp(pointIndex, 0, points.Length - 1);

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].pointIndex == pointIndex)
            {
                return points[i].pointTransform;
            }
        }

        return null;
    }

    public Vector3 GetPointPosition(int pointIndex)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].pointIndex == pointIndex)
            {
                return points[i].Position;
            }
        }

        return Vector3.zero;
    }

    public Vector3 GetPointPosition(Transform pointTransform)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].pointTransform == pointTransform)
            {
                return points[i].Position;
            }
        }

        return Vector3.zero;
    }

    public int GetPointIndex(Transform pointTransform)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].pointTransform == pointTransform)
            {
                return points[i].pointIndex;
            }
        }

        return 0;
    }

    public PathPointInfo GetClosestPointTo(Transform other)
    {
        return points[GetPointIndex(_pathPointTransforms.GetClosest(other))];
    }

    public PathPointInfo GetClosestPointTo(Vector3 position)
    {
        Transform closestPointTransform = _pathPointTransforms.GetClosest(position);

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].pointTransform == closestPointTransform)
            {
                return points[i];
            }
        }

        throw new System.Exception($"Point is not found. Number of points in PointPath: {points.Length}");
    }

    public PathPointInfo[] GetPoints(Transform startPointTransform, int count)
    {
        _requestedPointsRange = new PathPointInfo[count];

        _startPointsRangeIndex = GetPointIndex(startPointTransform);

        for (int i = 0; i < count; i++)
        {
            if (_startPointsRangeIndex + i < points.Length)
            {
                _requestedPointsRange[i] = points[_startPointsRangeIndex + i];
            }
        }

        return _requestedPointsRange;
    }
}

public class PathPointInfo
{
    public Transform pointTransform;
    public int pointIndex;

    public PathPointInfo nextPointInfo;
    public PathPointInfo previousPointInfo;

    public Vector3 leftBorderPoint;
    public Vector3 rightBorderPoint;

    public bool isEnd;
    public bool isNegative;

    public Vector3 Position => pointTransform.position;

    public PathPointInfo(Transform pointTransform, int pointIndex, PathPointInfo previousPointInfo)
    {
        this.pointTransform = pointTransform;
        this.pointIndex = pointIndex;

        this.previousPointInfo = previousPointInfo;

        leftBorderPoint = pointTransform.position - pointTransform.right * 1f; // TrackConstructor.roadWidth / 2f;
        rightBorderPoint = pointTransform.position + pointTransform.right * 1f; // TrackConstructor.roadWidth / 2f;

        //isEnd = true;
        isNegative = pointTransform.name[0] == 'N';

        if (previousPointInfo != null)
        {
            previousPointInfo.nextPointInfo = this;

            //previousPointInfo.IsEnd();
        }
    }

    public bool IsEnd()
    {
        /*
        if (nextPointInfo != null && previousPointInfo != null)
        {
            isEnd = (nextPointInfo.Position - Position).GetPlanarMagnitude(Axis.Y) > TrackConstructor.interpointDistance * 1.5f || (previousPointInfo.Position - Position).GetPlanarMagnitude(Axis.Y) > TrackConstructor.interpointDistance * 1.5f;
        }

        if (isEnd)
        {
            Debug.Log($" - PathPoint[{pointIndex}] is end point: {isEnd}");
        }
        */

        return isEnd;
    }
}
