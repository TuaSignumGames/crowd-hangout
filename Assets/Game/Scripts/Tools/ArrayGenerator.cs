using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayGenerator : MonoBehaviour
{
    public string elementName = "Item";
    public GameObject elementPrefab;
    [Space]
    public ArrayConfigurationType arrayType;
    //public ArrayBuildingParameter placementParameter;
    public int elementsCount;
    public bool generateLast;
    [Space]
    public LinearArraySettings linearArraySettings;
    public ArcuateArraySettings arcuateArraySettings;
    public CircularArraySettings circularArraySettings;
    [Space]
    public PlacementCorrectionSettings placementCorrectionSettings;
    public bool correctPlacement;
    [Space]
    public bool drawGizmos;

    private List<Transform> _arrayContainerElements;

    private GameObject _newElementInstance;

    private RaycastHit _hitInfo;

    private Vector3 _originPoint;

    private float _step;

    public void Generate()
    {
        if (transform.childCount > 0)
        {
            ClearArray();
        }

        switch (arrayType)
        {
            case ArrayConfigurationType.Line: GenerateLinearArray(); break;

            case ArrayConfigurationType.Arc: GenerateArcuateArray(); break;

            case ArrayConfigurationType.Circle: GenerateCircularArray(); break;
        }

        if (correctPlacement)
        {
            CorrectPlacement();
        }
    }

    private void GenerateLinearArray()
    {
        _step = linearArraySettings.lineLength / (elementsCount - 1);

        for (int i = 0; i < (generateLast ? elementsCount : elementsCount - 1); i++)
        {
            GenerateElement();

            _newElementInstance.transform.position = transform.position + transform.forward * _step * i;
        }
    }

    private void GenerateArcuateArray()
    {
        _step = arcuateArraySettings.arcAngularLength / (elementsCount - 1);

        Vector3 pivotPoint = transform.position + transform.right * arcuateArraySettings.arcRadius;

        for (int i = 0; i < (generateLast ? elementsCount : elementsCount - 1); i++)
        {
            GenerateElement();

            _newElementInstance.transform.RotateAround(pivotPoint, transform.up, _step * i);
        }
    }

    private void GenerateCircularArray()
    {
        _step = 360f / (elementsCount - 1);

        for (int i = 0; i < (generateLast ? elementsCount : elementsCount - 1); i++)
        {
            GenerateElement();

            _newElementInstance.transform.position += transform.forward * circularArraySettings.circleRadius;

            _newElementInstance.transform.RotateAround(transform.position, transform.up, _step * i);
        }
    }

    public void CorrectPlacement()
    {
        if (transform.childCount > 0)
        {
            _arrayContainerElements = transform.GetChildren();

            for (int i = 0; i < _arrayContainerElements.Count; i++)
            {
                _originPoint = GetCorrectionOriginPoint(_arrayContainerElements[i]);

                if (Physics.Raycast(_originPoint, _arrayContainerElements[i].position - _originPoint, out _hitInfo, placementCorrectionSettings.rayLength, 1 << placementCorrectionSettings.collisionLayer))
                {
                    _arrayContainerElements[i].position = _hitInfo.point;
                }
            }
        }
    }

    public void ClearArray()
    {
        _arrayContainerElements = transform.GetChildren();

        for (int i = 0; i < _arrayContainerElements.Count; i++)
        {
            if (_arrayContainerElements[i].gameObject != elementPrefab)
            {
                DestroyImmediate(_arrayContainerElements[i].gameObject);
            }
        }
    }

    private Vector3 GetCorrectionOriginPoint(Transform pointTransform)
    {
        switch (placementCorrectionSettings.correctionAxis)
        {
            case Axis.X: return pointTransform.position + pointTransform.right * placementCorrectionSettings.rayLength;

            case Axis.Y: return pointTransform.position + pointTransform.up * placementCorrectionSettings.rayLength;

            case Axis.Z: return pointTransform.position + pointTransform.forward * placementCorrectionSettings.rayLength;

            default: return Vector3.zero;
        }
    }

    private GameObject GenerateElement()
    {
        if (elementPrefab)
        {
            if (elementPrefab.transform.parent != transform)
            {
                #if UNITY_EDITOR

                _newElementInstance = UnityEditor.PrefabUtility.InstantiatePrefab(elementPrefab, transform) as GameObject;

                #endif
            }
            else
            {
                _newElementInstance = Instantiate(elementPrefab, transform);
            }
        }
        else
        {
            _newElementInstance = new GameObject();

            _newElementInstance.transform.SetParent(transform);
        }

        _newElementInstance.transform.localPosition = Vector3.zero;

        _newElementInstance.name = $"{elementName}[{_newElementInstance.transform.GetSiblingIndex()}]";

        return _newElementInstance;
    }

    private void OnValidate()
    {
        if (elementPrefab)
        {
            elementName = elementPrefab.name;
        }
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            if (transform.childCount > 0)
            {
                _arrayContainerElements = transform.GetChildren();

                for (int i = 0; i < _arrayContainerElements.Count; i++)
                {
                    GizmoDrawer.DrawSphereGizmo(_arrayContainerElements[i].position, 1f, Color.cyan, GizmoRenderMode.Solid);
                }
            }
        }
    }

    [Serializable]
    public class LinearArraySettings
    {
        public float lineStep;
        public float lineLength;
    }

    [Serializable]
    public class ArcuateArraySettings
    {
        public float arcRadius;
        public float arcAngularStep;
        public float arcAngularLength;
    }

    [Serializable]
    public class CircularArraySettings
    {
        public float circleRadius;
    }

    [Serializable]
    public class PlacementCorrectionSettings
    {
        public int collisionLayer;
        [Space]
        public Axis correctionAxis = Axis.Y;
        public float rayLength = 10f;
    }
}
