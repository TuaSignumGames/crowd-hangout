using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Magnet
{
    public GameObject container;
    public Transform iconTransform;
    public float colliderMultiplier;

    public bool IsActive => container.activeSelf;

    public void SetActive(bool isActive)
    {
        container.SetActive(isActive);
    }
}
