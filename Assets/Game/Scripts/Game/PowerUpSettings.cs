using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PowerUpSettings
{
    public Magnet magnet;
    public Propeller propeller;

    public bool IsAnyPowerUpActive()
    {
        return magnet.IsActive || propeller.IsActive;
    }
}
