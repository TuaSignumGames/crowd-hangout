using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HumanBone
{
    public string title;
    [Space]
    public Transform transform;
    public Vector3 bendRangeFloor;
    public Vector3 bendRangeCeil;

    public void BendRandomly()
    {
        transform.localEulerAngles += new Vector3(Random.Range(bendRangeFloor.x, bendRangeCeil.x), Random.Range(bendRangeFloor.y, bendRangeCeil.y), Random.Range(bendRangeFloor.z, bendRangeCeil.z));
    }
}
