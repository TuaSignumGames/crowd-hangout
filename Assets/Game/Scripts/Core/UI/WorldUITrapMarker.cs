using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUITrapMarker : WorldUIElement
{
    public static WorldUITrapMarker Instance;

    [Space]
    public ShakeAgent shakeAgent;

    private Evaluator _evaluator;

    private float _shakingFactor;

    protected override void Awake()
    {
        base.Awake();

        Instance = this;

        _evaluator = new Evaluator(MonoUpdateType.FixedUpdate);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (shakeAgent.enabled)
        {
            _evaluator.Iterate(ref _shakingFactor);

            shakeAgent.weight = _shakingFactor;

            shakeAgent.transform.localScale = Vector3.one * (1f + 0.2f * _shakingFactor);
        }
    }
    
    public void OnTrapActivated()
    {
        /*
        _evaluator.Setup(0, 1f, 1f, EvaluationType.Linear, () => { TryHide(LevelManager.Instance.SwitchToNextTrap); ResetShaker(); });

        shakeAgent.enabled = true;
        */
    }

    private void ResetShaker()
    {
        shakeAgent.enabled = false;

        _shakingFactor = 0;

        shakeAgent.weight = _shakingFactor;

        shakeAgent.transform.localEulerAngles = Vector3.zero;
        shakeAgent.transform.localScale = Vector3.one;
    }
}
