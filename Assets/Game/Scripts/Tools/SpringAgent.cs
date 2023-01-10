using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringAgent : MonoBehaviour
{
    [SerializeField] private Transform _springTarget;
    [SerializeField] private Transform _springPivot;
    [Space]
    [SerializeField] private Vector3 _springVector;
    [SerializeField] private Vector3 _dampingVector;
    [Space]
    [SerializeField] private Vector3 _gravityVector;
    [Space]
    [SerializeField] private float _springLength;

    private Vector3 _springTargetVelocity;
    private Vector3 _springTargetDisplacement;

    private Vector3 _springOffset;

    private Vector3 _previousMotionSourcePosition;

    private void FixedUpdate()
    {

    }
}
