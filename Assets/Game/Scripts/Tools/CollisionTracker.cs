using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTracker : MonoBehaviour
{
    [SerializeField] private string[] _tagFilters;

    private bool _isColliding;

    public bool IsColliding => _isColliding;

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < _tagFilters.Length; i++)
        {
            if (_tagFilters[i] == collision.gameObject.tag)
            {
                _isColliding = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        _isColliding = false;
    }
}
