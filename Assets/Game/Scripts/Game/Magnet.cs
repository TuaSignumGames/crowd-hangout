using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Magnet
{
    public GameObject container;
    public Transform iconTransform;
    public float colliderMultiplier;
    [Space]
    public float duration;

    private float activationTime;

    public bool IsActive => container.activeSelf;

    public void SetActive(bool isActive)
    {
        if (isActive)
        {
            activationTime = Time.timeSinceLevelLoad;
        }

        container.SetActive(isActive);
    }

    public float GetNormalizedTime()
    {
        return (Time.timeSinceLevelLoad - activationTime) / duration;
    }
}
