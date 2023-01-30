using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : MonoBehaviour
{
    public new BoxCollider collider;
    public Animator animator;
    [Space]
    public HumanRig rigSettings;

    private void Start()
    {
        rigSettings.Initialize();
    }

    private void OnTriggerEnter(Collider other)
    {
        transform.SetParent(null);
    }

    [System.Serializable]
    public class HumanRig
    {
        public List<HumanBone> bones;
        public Transform leftHandContainer;
        public Transform rightHandContainer;
        public bool randomizePose;

        public void Initialize()
        {
            if (randomizePose)
            {
                RandomizePose();
            }
        }

        private void RandomizePose()
        {
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].BendRandomly();
            }
        }
    }

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
}