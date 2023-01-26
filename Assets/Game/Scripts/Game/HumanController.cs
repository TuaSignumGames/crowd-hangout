using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : MonoBehaviour
{
    public new BoxCollider collider;
    public Animator animator;
    [Space]
    public Transform leftHandContainer;
    public Transform rightHandContainer;

    private void OnTriggerEnter(Collider other)
    {
        transform.SetParent(null);
    }

    public class HumanBone
    {
        private Transform transform;
        private Vector3 bendAxis;
    }
}