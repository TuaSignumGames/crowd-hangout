using System.Collections;
using System.Collections.Generic;
using Facebook.Unity.Example;
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
    public float viewDistanceIncrement;
    public float viewTransitionTime;

    private Ray ray;

    private RaycastHit raycastInfo;

    private Vector3 targetOffset;

    private TransformEvaluator worldEvaluator;
    private TransformEvaluator localEvaluator;

    private float actualViewDistance;

    public Vector3 Position => camera.transform.position;

    private void Awake()
    {
        Instance = this;

        targetOffset = transform.position;

        actualViewDistance = -camera.transform.localPosition.z;
    }

    private void Start()
    {
        PlayerController.Humanball.Structure.OnLayerIncremented += IncreaseViewDistance;
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

    public void FocusOn(Transform targetTransform, CameraViewData viewData)
    {
        this.targetTransform = targetTransform;

        SetView(viewData);

        targetOffset = Vector3.zero;
    }

    public void FocusOn(Vector3 targetPosition, CameraViewData viewData)
    {
        this.targetTransform = null;

        Translate(targetPosition, viewData.translationDuration, Space.World);

        SetView(viewData);

        targetOffset = Vector3.zero;
    }

    public void SetView(CameraViewData viewData)
    {
        InitializeEvaluator(viewData.translationSpace);
        InitializeEvaluator(viewData.rotationSpace);

        Translate(viewData.position, viewData.translationDuration, viewData.translationSpace);

        Rotate(viewData.eulerAngles, viewData.rotationDuration, viewData.rotationSpace);
    }

    public RaycastHit Raycast()
    {
        raycastInfo = new RaycastHit();

        ray = camera.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out raycastInfo, 400f);

        return raycastInfo;
    }

    public void IncreaseViewDistance(int distanceLevel)
    {
        actualViewDistance += viewDistanceIncrement;

        Translate(new Vector3(camera.transform.localPosition.x, camera.transform.localPosition.y, -actualViewDistance), viewTransitionTime, Space.Self);
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
