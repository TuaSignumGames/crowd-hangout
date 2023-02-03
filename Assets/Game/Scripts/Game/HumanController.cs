using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : MonoBehaviour
{
    public new BoxCollider collider;
    public Animator animator;
    [Space]
    public HumanRig rigSettings;

    private bool isFree = true;

    public void Initialize(bool isFree)
    {
        this.isFree = isFree;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            if (isFree)
            {
                isFree = false;

                PlayerController.Instance.Ball.StickHuman(this);
            }
        }
        else
        {
            if (!isFree)
            {
                isFree = true;

                PlayerController.Instance.Ball.UnstickHuman(this);
            }
        }
    }

    [System.Serializable]
    public class HumanRig
    {
        public List<HumanBone> bones;
        public Transform leftHandContainer;
        public Transform rightHandContainer;

        public void RandomizePose()
        {
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].BendRandomly();
            }
        }

        public void SavePose()
        {
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].SaveTransform();
            }
        }

        public void LoadPose()
        {
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].LoadTransform();
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

        private TransformData savedTransformData;

        public void BendRandomly()
        {
            transform.localEulerAngles += new Vector3(Random.Range(bendRangeFloor.x, bendRangeCeil.x), Random.Range(bendRangeFloor.y, bendRangeCeil.y), Random.Range(bendRangeFloor.z, bendRangeCeil.z));
        }

        public void SaveTransform()
        {
            savedTransformData = new TransformData(transform, Space.Self);
        }

        public void LoadTransform()
        {
            transform.ApplyData(savedTransformData);
        }
    }
}