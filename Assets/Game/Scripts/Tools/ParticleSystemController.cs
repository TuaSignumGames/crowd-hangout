using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _targetParticleSystem;

    private ParticleSystem.MainModule _mainModule;

    private ParticleSystem.MinMaxCurve _baseStartSize;

    private Evaluator _sizeMultiplierEvaluator;

    private float _startSizeMultiplier;

    private void Awake()
    {
        _mainModule = _targetParticleSystem.main;

        _baseStartSize = new ParticleSystem.MinMaxCurve(_mainModule.startSize.constantMin, _mainModule.startSize.constantMax);

        _sizeMultiplierEvaluator = new Evaluator(MonoUpdateType.LateUpdate);

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_sizeMultiplierEvaluator.Iterating)
        {
            _sizeMultiplierEvaluator.Iterate(ref _startSizeMultiplier);

            StartSizeMultiplier = _startSizeMultiplier;
        }
    }

    public void SetStartSizeMultiplier(float targetValue, float transitionDuration, System.Action onTransitionComplete = null)
    {
        _sizeMultiplierEvaluator.Setup(_startSizeMultiplier, targetValue, transitionDuration, EvaluationType.Smooth, onTransitionComplete);
    }

    public float StartSpeed
    {
        get { return _mainModule.startSpeed.constant; }
        set { _mainModule.startSpeed = new ParticleSystem.MinMaxCurve(value); }
    }

    public float StartSizeMultiplier
    {
        get { return _mainModule.startSize.constantMin / _baseStartSize.constantMin; }
        set { _mainModule.startSize = new ParticleSystem.MinMaxCurve(_baseStartSize.constantMin * value, _baseStartSize.constantMax * value); }
    }
}
