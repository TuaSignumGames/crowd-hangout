using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public new Camera camera;
    [Space]
    public Transform targetTransform;
    public float motionLerpingFactor;
    [Space]
    public float[] viewDistanceLevels;
    public float viewTransitionTime;

    private Vector3 targetOffset;

    private TransformEvaluator worldEvaluator;
    private TransformEvaluator localEvaluator;

    private void Awake()
    {
        Instance = this;

        targetOffset = transform.position;
    }

    private void Start()
    {
        PlayerController.Instance.Ball.Structure.OnLayerIncremented += SetViewDistance;
    }

    private void FixedUpdate()
    {
        if (targetTransform)
        {
            transform.position = Vector3.Lerp(transform.position, targetTransform.position + targetOffset, motionLerpingFactor);
        }

        if (worldEvaluator != null)
        {
            worldEvaluator.Update();
        }

        if (localEvaluator != null)
        {
            localEvaluator.Update();
        }
    }

    public void ApplyToContainer(Transform container)
    {
        camera.transform.SetParent(container);

        camera.transform.localPosition = Vector3.zero;
        camera.transform.localEulerAngles = Vector3.zero;
    }

    public void Translate(Vector3 targetPosition, float duration, Space space)
    {
        InitializeEvaluator(space);

        if (space == Space.World)
        {
            worldEvaluator.Translate(targetPosition, duration, EvaluationType.Smooth);
        }
        else
        {
            localEvaluator.TranslateLocal(targetPosition, duration, EvaluationType.Smooth);
        }
    }

    public void Rotate(Vector3 targetEulerAngles, float duration, Space space)
    {
        InitializeEvaluator(space);

        if (space == Space.World)
        {
            worldEvaluator.Rotate(targetEulerAngles, duration, EvaluationType.Smooth);
        }
        else
        {
            localEvaluator.RotateLocal(targetEulerAngles, duration, EvaluationType.Smooth);
        }
    }

    public void SetViewDistance(int distanceLevel)
    {
        Translate(new Vector3(camera.transform.localPosition.x, camera.transform.localPosition.y, -viewDistanceLevels[distanceLevel]), viewTransitionTime, Space.Self);
    }

    private void InitializeEvaluator(Space space)
    {
        if (space == Space.World)
        {
            if (worldEvaluator == null)
            {
                worldEvaluator = new TransformEvaluator(transform, MonoUpdateType.FixedUpdate);
            }
        }
        else
        {
            if (localEvaluator == null)
            {
                localEvaluator = new TransformEvaluator(camera.transform, MonoUpdateType.FixedUpdate);
            }
        }
    }
}
