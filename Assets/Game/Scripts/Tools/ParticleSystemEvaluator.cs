using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticleSystemNumericalParameter {  }

public class ParticleSystemEvaluator : MonoBehaviour
{
    [SerializeField] private ParticleSystem _targetParticleSystem;
    [Space]
    [SerializeField] private NumericalEvaluationData[] _numericalParameters;
    [Space]
    [Range(0, 1f)]
    [SerializeField] private float value;

    private void Awake()
    {
        
    }

    private void Update()
    {
        
    }
}

[System.Serializable]
public struct NumericalEvaluationData
{
    public ParticleSystemNumericalParameter parameter;
    public float baseValue;
    public float targetValue;
}
