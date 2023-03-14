using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSimulatorController : MonoBehaviour
{
    public Vector3 impulse;
    public Vector3 angularMomentum;

    private MotionSimulator motionSimulator;

    private void Awake()
    {
        motionSimulator = new MotionSimulator(transform, MonoUpdateType.FixedUpdate);

        motionSimulator.Gravity = new Vector3();

        motionSimulator.velocity = impulse;
        motionSimulator.angularVelocity = angularMomentum;
    }

    private void FixedUpdate()
    {
        motionSimulator.Update();
    }
}