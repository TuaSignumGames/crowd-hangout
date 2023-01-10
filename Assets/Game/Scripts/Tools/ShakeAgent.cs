using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeAgent : MonoBehaviour
{
    [SerializeField] private ShakeData[] _shiftingPatterns;
    [SerializeField] private ShakeData[] _tiltingPatterns;
    [Space]
    [Range(0, 1f)]
    public float weight = 1f;
    public float multiplier = 1f;
    [Space]
    [Range(0, 1f)]
    public float shiftingWeight = 1f;
    [Range(0, 1f)]
    public float tiltingWeight = 1f;

    private PatternEvaluator[] _shiftPatternEvaluators;
    private PatternEvaluator[] _tiltPatternEvaluators;

    private Evaluator _multiplierEvaluator;

    private Vector3 _originalPosition;
    private Vector3 _originalEulers;

    private Vector3 _actualShift;
    private Vector3 _actualTilt;

    private void Awake()
    {
        _originalPosition = transform.localPosition;
        _originalEulers = transform.localEulerAngles;

        _shiftPatternEvaluators = new PatternEvaluator[_shiftingPatterns.Length];
        _tiltPatternEvaluators = new PatternEvaluator[_tiltingPatterns.Length];

        _multiplierEvaluator = new Evaluator(MonoUpdateType.Update);

        for (int i = 0; i < _shiftPatternEvaluators.Length; i++)
        {
            _shiftPatternEvaluators[i] = new PatternEvaluator(_shiftingPatterns[i], Random.Range(0, 500f));
        }

        for (int i = 0; i < _tiltPatternEvaluators.Length; i++)
        {
            _tiltPatternEvaluators[i] = new PatternEvaluator(_tiltingPatterns[i], Random.Range(0, 500f));
        }
    }

    private void Update()
    {
        if (_multiplierEvaluator.Iterating)
        {
            _multiplierEvaluator.Iterate(ref multiplier);
        }

        if (_shiftPatternEvaluators.Length > 0)
        {
            _actualShift = new Vector3();

            for (int i = 0; i < _shiftPatternEvaluators.Length; i++)
            {
                _actualShift += GetDisplacementIncrement(_shiftPatternEvaluators[i]);
            }

            transform.localPosition = _originalPosition + (_actualShift * shiftingWeight * weight * multiplier);
        }

        if (_tiltPatternEvaluators.Length > 0)
        {
            _actualTilt = new Vector3();

            for (int i = 0; i < _tiltPatternEvaluators.Length; i++)
            {
                _actualTilt += GetDisplacementIncrement(_tiltPatternEvaluators[i]);
            }

            transform.localEulerAngles = _originalEulers + (_actualTilt * tiltingWeight * weight * multiplier);
        }
    }

    public void SetMultiplier(float targetValue, float transitionDuration)
    {
        _multiplierEvaluator.Setup(multiplier, targetValue, transitionDuration, EvaluationType.Smooth);
    }

    private Vector3 GetDisplacementIncrement(PatternEvaluator evaluator)
    {
        if (evaluator.ShakeAxis == Axis.X)
        {
            return new Vector3(evaluator.Evaluate(), 0, 0);
        }
        if (evaluator.ShakeAxis == Axis.Y)
        {
            return new Vector3(0, evaluator.Evaluate(), 0);
        }
        else
        {
            return new Vector3(0, 0, evaluator.Evaluate());
        }
    }

    private void OnValidate()
    {
        for (int i = 0; i < _shiftingPatterns.Length; i++)
        {
            _shiftingPatterns[i].title = string.Format($"{_shiftingPatterns[i].axis} [F:{_shiftingPatterns[i].frequency} A:{_shiftingPatterns[i].amplitude}]");
        }

        for (int i = 0; i < _tiltingPatterns.Length; i++)
        {
            _tiltingPatterns[i].title = string.Format($"{_tiltingPatterns[i].axis} [F:{_tiltingPatterns[i].frequency} A:{_tiltingPatterns[i].amplitude}]");
        }
    }

    private class PatternEvaluator
    {
        private float _perlinSeed;
        private float _amplitude;

        private float _t;
        private float _dt;

        public Axis ShakeAxis { get; private set; }

        public PatternEvaluator(ShakeData shakeData, float perlinSeed)
        {
            _perlinSeed = perlinSeed;
            _amplitude = shakeData.amplitude;

            _dt = shakeData.frequency;

            ShakeAxis = shakeData.axis;
        }

        public float Evaluate()
        {
            _t += _dt * Time.deltaTime;

            return (Mathf.PerlinNoise(_t, _perlinSeed) - 0.5f) * 2f * _amplitude;
        }
    }
}

[System.Serializable]
public struct ShakeData
{
    [HideInInspector] public string title;

    public Axis axis;
    public float frequency;
    public float amplitude;
}
