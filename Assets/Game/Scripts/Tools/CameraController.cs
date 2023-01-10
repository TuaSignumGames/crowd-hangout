using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [SerializeField] protected Camera _camera;

    public Camera Camera => _camera;

    private void Awake()
    {
        Instance = this;
    }

    public void ApplyToContainer(Transform container)
    {
        _camera.transform.SetParent(container);

        _camera.transform.localPosition = Vector3.zero;
        _camera.transform.localEulerAngles = Vector3.zero;
    }
}
