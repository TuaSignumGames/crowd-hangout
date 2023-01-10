using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpicTransitionsManager : Service<LerpicTransitionsManager>
{
    [SerializeField] private AnimationCurve _sigmoidTransitionCurve;

    public AnimationCurve SigmoidTransitionCurve { get { return _sigmoidTransitionCurve; } }
}
