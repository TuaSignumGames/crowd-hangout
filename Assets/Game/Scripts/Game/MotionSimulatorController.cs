using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSimulatorController : MonoBehaviour
{
    public Vector3 impulse;
    public Vector3 angularMomentum;
    [Space]
    public Vector3 gravity;
    public bool usePhysicsSettings;

    private MotionSimulator motionSimulator;

    private void Awake()
    {
        motionSimulator = new MotionSimulator(transform, MonoUpdateType.FixedUpdate);

        if (usePhysicsSettings)
        {
            gravity = Physics.gravity;
        }

        motionSimulator.Gravity = gravity;

        motionSimulator.velocity = impulse;
        motionSimulator.angularVelocity = angularMomentum;
    }

    private void FixedUpdate()
    {
        motionSimulator.Update();
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump(LevelGenerator.Instance.GetBlockPair(transform.position).Position.y - transform.position.y);
        }
    }

    private void Jump(float height)
    {
        if (height > 0)
        {
            motionSimulator.velocity = new Vector3(0, Mathf.Sqrt(2 * -gravity.y * height), 0);
        }
    }
}