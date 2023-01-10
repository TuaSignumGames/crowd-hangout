using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTriggerAgent : MonoBehaviour
{
    [SerializeField] private string[] _collisionTags;
    [SerializeField] private bool _trackCollisions;
    [Space]
    [SerializeField] private UnityEvent _onCollisionEnter;
    [Space]
    [SerializeField] private UnityEvent _customEvent;

    public void InvokeCustomEvent()
    {
        if (_onCollisionEnter.GetPersistentEventCount() > 0)
        {
            _customEvent.Invoke();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_trackCollisions)
        {
            if (_collisionTags.Length > 0)
            {
                for (int i = 0; i < _collisionTags.Length; i++)
                {
                    if (collision.gameObject.tag == _collisionTags[i])
                    {
                        _onCollisionEnter.Invoke();

                        break;
                    }
                }
            }
            else
            {
                _onCollisionEnter.Invoke();
            }
        }
    }
}
