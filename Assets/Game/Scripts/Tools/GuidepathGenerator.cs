using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidepathGenerator : MonoBehaviour
{
    public Transform pointerOriginal;
    public AnimationCurve sideOffsetCurve;
    public int skippedPointers;
    [Space]
    public float effectionDistance;
    public AnimationCurve scaleByDistanceCurve;
    [Space]
    public float activationDistance;

    private Transform userTransform;

    private List<Transform> pointerTransforms;

    private PointPath trackPath;

    private Transform newPointerTransform;

    private PathPointInfo basePathPoint;
    private PathPointInfo targetPathPoint;

    private float baseSideOffset;
    private float targetSideOffset;

    private float pointerPlanarDistance;
    private float pointerScale;

    private int pathPointIndex;
    private int activationPathPointIndex;

    private float pathPointIndicesRange;

    private bool isGenerated;

    public int PathPointIndex => pathPointIndex;
    public int ActivationPathPointIndex => activationPathPointIndex;

    public void Initialize()
    {
        //trackPath = TrackConstructor.ActiveTrack.PointPath;

        pathPointIndex = trackPath.GetClosestPointTo(transform.position).pointIndex;
        //activationPathPointIndex = Mathf.Clamp(pathPointIndex - Mathf.RoundToInt(activationDistance / TrackConstructor.interpointDistance), 0, int.MaxValue);
    }

    public void LateUpdate()
    {
        if (isGenerated)
        {
            UpdateVisibility();
        }
    }

    public void Generate(Transform userTransform)
    {
        if (!isGenerated)
        {
            this.userTransform = userTransform;

            pointerTransforms = new List<Transform>();

            basePathPoint = trackPath.GetClosestPointTo(userTransform.position);
            targetPathPoint = trackPath.points[pathPointIndex];

            baseSideOffset = Vector3.Project(userTransform.position - basePathPoint.Position, basePathPoint.pointTransform.right).x;
            targetSideOffset = transform.localPosition.x;

            pathPointIndicesRange = targetPathPoint.pointIndex - basePathPoint.pointIndex;

            for (int i = basePathPoint.pointIndex; i < targetPathPoint.pointIndex; i++)
            {
                if (i % (skippedPointers + 1) == 0)
                {
                    newPointerTransform = Instantiate(pointerOriginal, pointerOriginal.parent);

                    newPointerTransform.position = trackPath.points[i].Position + trackPath.points[i].pointTransform.up * pointerOriginal.transform.localPosition.y + trackPath.points[i].pointTransform.right * Mathf.Lerp(baseSideOffset, targetSideOffset, sideOffsetCurve.Evaluate((i - basePathPoint.pointIndex) / pathPointIndicesRange));

                    pointerTransforms.Add(newPointerTransform);
                }
            }

            if (targetPathPoint.pointIndex % (skippedPointers + 1) == 0)
            {
                pointerTransforms.Add(pointerOriginal);
            }
            else
            {
                pointerOriginal.gameObject.SetActive(false);
            }

            for (int i = 0; i < pointerTransforms.Count - 1; i++)
            {
                pointerTransforms[i].forward = pointerTransforms[i + 1].position - pointerTransforms[i].position;
            }

            isGenerated = true;

            UpdateVisibility();
        }
    }

    private void UpdateVisibility()
    {
        for (int i = 0; i < pointerTransforms.Count; i++)
        {
            pointerPlanarDistance = (pointerTransforms[i].position - userTransform.position).GetPlanarMagnitude(Axis.Y);

            if (pointerTransforms[i].gameObject.activeSelf && pointerPlanarDistance < effectionDistance)
            {
                pointerScale = scaleByDistanceCurve.Evaluate(pointerPlanarDistance / effectionDistance);

                pointerTransforms[i].localScale = new Vector3(pointerScale, pointerScale, pointerScale);

                if (pointerScale < 0.1f)
                {
                    pointerTransforms[i].gameObject.SetActive(false);
                }
            }
        }
    }
}