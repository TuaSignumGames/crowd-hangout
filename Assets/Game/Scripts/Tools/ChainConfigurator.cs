using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainConfigurator : MonoBehaviour
{
    public Rigidbody connectionRigidbody;
    public Transform rootTransform;
    [Space]
    public float heaviestLinkMass;
    public Vector3 linkColliderSize;
    [Space]
    public AnimationCurve massDistributionCurve;
    public AnimationCurve sizeEvaluationCurve;

    private List<Rigidbody> _linkBodies;

    private Rigidbody _previousLinkRigidbody;

    private Transform _currentLinkTransform;
    private Transform _nextLinkTransform;

    private Rigidbody _currentLinkRigidbody;

    private float _evaluationFactor;

    public void Configurate()
    {
        _linkBodies = new List<Rigidbody>();

        _previousLinkRigidbody = connectionRigidbody;

        _currentLinkTransform = rootTransform;
        _nextLinkTransform = _currentLinkTransform.GetChild(0);

        _currentLinkRigidbody = null;

        while (_currentLinkTransform != null)
        {
            _linkBodies.Add(_currentLinkTransform.gameObject.AddComponent<Rigidbody>());

            _currentLinkTransform = _nextLinkTransform;

            if (_currentLinkTransform)
            {
                _nextLinkTransform = _currentLinkTransform.childCount > 0 ? _currentLinkTransform.GetChild(0) : null;
            }
        }

        for (int i = 0; i < _linkBodies.Count; i++)
        {
            _currentLinkRigidbody = _linkBodies[i];

            _evaluationFactor = i / (float)_linkBodies.Count;

            _currentLinkRigidbody.mass = heaviestLinkMass * massDistributionCurve.Evaluate(_evaluationFactor);

            _currentLinkRigidbody.gameObject.AddComponent<CharacterJoint>().connectedBody = _previousLinkRigidbody;
            _currentLinkRigidbody.gameObject.AddComponent<BoxCollider>().size = linkColliderSize * sizeEvaluationCurve.Evaluate(_evaluationFactor);

            _previousLinkRigidbody = _currentLinkRigidbody;
        }

        /*
        while (_currentLinkTransform != null)
        {
            _currentLinkRigidbody = _currentLinkTransform.gameObject.AddComponent<Rigidbody>();

            _currentLinkTransform.gameObject.AddComponent<CharacterJoint>().connectedBody = _previousLinkRigidbody;
            _currentLinkTransform.gameObject.AddComponent<BoxCollider>().size = linkColliderSize;

            _previousLinkRigidbody = _currentLinkRigidbody;

            _currentLinkTransform = _nextLinkTransform;

            if (_currentLinkTransform)
            {
                _nextLinkTransform = _currentLinkTransform.childCount > 0 ? _currentLinkTransform.GetChild(0) : null;
            }
        }
        */

        print($" --- Chain successfully configurated: {rootTransform.gameObject.name}");
    }

    public void Clear()
    {
        if (_linkBodies != null)
        {
            if (_linkBodies.Count > 0)
            {
                for (int i = 0; i < _linkBodies.Count; i++)
                {
                    DestroyImmediate(_linkBodies[i].GetComponent<CharacterJoint>());
                    DestroyImmediate(_linkBodies[i].GetComponent<BoxCollider>());
                    DestroyImmediate(_linkBodies[i]);
                }
            }
        }
        else
        {
            _currentLinkTransform = rootTransform;
            _nextLinkTransform = _currentLinkTransform.GetChild(0);

            while (_currentLinkTransform != null)
            {
                DestroyImmediate(_currentLinkTransform.GetComponent<CharacterJoint>());
                DestroyImmediate(_currentLinkTransform.GetComponent<BoxCollider>());
                DestroyImmediate(_currentLinkTransform.GetComponent<Rigidbody>());

                _currentLinkTransform = _nextLinkTransform;

                if (_currentLinkTransform)
                {
                    _nextLinkTransform = _currentLinkTransform.childCount > 0 ? _currentLinkTransform.GetChild(0) : null;
                }
            }
        }

        print($" --- Chain successfully cleared: {rootTransform.gameObject.name}");
    }
}