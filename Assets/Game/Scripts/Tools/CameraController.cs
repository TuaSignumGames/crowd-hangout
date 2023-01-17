using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public new Camera camera;
    [Space]
    public Transform targetTransform;
    public float motionLerpingFactor;

    private Vector3 targetOffset;

    private void Awake()
    {
        Instance = this;

        targetOffset = transform.position;
    }

    private void FixedUpdate()
    {
        if (targetTransform)
        {
            transform.position = Vector3.Lerp(transform.position, targetTransform.position + targetOffset, motionLerpingFactor);
        }
    }

    public void ApplyToContainer(Transform container)
    {
        camera.transform.SetParent(container);

        camera.transform.localPosition = Vector3.zero;
        camera.transform.localEulerAngles = Vector3.zero;
    }
}
