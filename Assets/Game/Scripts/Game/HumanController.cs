using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO Implement BattlePath 

public enum HumanAnimationType { Running, Flying, Attacking, Falling, Dying }

public class HumanController : MonoBehaviour
{
    public new BoxCollider collider;
    public Animator animator;
    [Space]
    public HumanRig rigSettings;
    public List<Weapon> weaponSettings;

    public HumanPose actualPose;

    private Weapon currentWeapon;

    private float healthPoints;

    public static HumanPose defaultPose;

    public static int animatorFlyHash;
    public static int animatorDefeatHash;
    public static int animatorGroundedHash;
    public static int animatorRunningHash;
    public static int animatorAttackingHash;
    public static int animatorAttackIdHash;
    public static int animatorDefeatIdHash;

    private bool isFree = true;

    public void Initialize(bool isFree)
    {
        this.isFree = isFree;
    }

    public void PlaceInCell(HumanballCell cell)
    {
        isFree = false;

        transform.SetParent(cell.transform);

        transform.localPosition = Vector3.zero;
        transform.forward = cell.transform.forward;

        if (cell.Pose != null)
        {
            ApplyPose(cell.Pose);
        }
        else
        {
            ApplyPose(defaultPose);
        }

        collider.enabled = true;
    }

    public void DropFromCell()
    {
        collider.enabled = false;

        transform.SetParent(null);

        PlayAnimation(HumanAnimationType.Falling);

        isFree = true;
    }

    public HumanPose PeekPose()
    {
        actualPose = new HumanPose(transform, rigSettings.bones);

        return actualPose;
    }

    private void ApplyPose(HumanPose pose)
    {
        actualPose = pose;

        transform.ApplyData(actualPose.bodyTransformData);

        for (int i = 0; i < actualPose.boneTransformDatas.Length; i++)
        {
            rigSettings.bones[i].transform.ApplyData(actualPose.boneTransformDatas[i]);
        }
    }

    public void Damage(float value)
    {
        healthPoints -= value;
    }

    private void PlayAnimation(HumanAnimationType animationType)
    {
        animator.enabled = true;

        switch (animationType)
        {
            case HumanAnimationType.Running: animator.SetBool(animatorGroundedHash, true); animator.SetBool(animatorRunningHash, true); break;
            case HumanAnimationType.Flying: animator.SetTrigger(animatorFlyHash); break;
            case HumanAnimationType.Attacking: animator.SetInteger(animatorAttackIdHash, currentWeapon.attackAnimationID); animator.SetBool(animatorAttackingHash, true); break;
            case HumanAnimationType.Falling: animator.SetBool(animatorGroundedHash, false); break;
            case HumanAnimationType.Dying: animator.SetInteger(animatorAttackIdHash, Random.Range(0, 5)); animator.SetTrigger(animatorDefeatHash); break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isFree)
        {
            if (other.gameObject.layer == 7)
            {
                PlayerController.Instance.Ball.UnstickHuman(this);
            }
        }
    }

    private void OnValidate()
    {
        if (weaponSettings.Count > 0)
        {
            for (int i = 0; i < weaponSettings.Count; i++)
            {
                if (weaponSettings[i].weaponContainer)
                {
                    weaponSettings[i].title = weaponSettings[i].weaponContainer.name;
                }
            }
        }
    }

    public static void InitializeAnimatorHashes()
    {
        animatorFlyHash = Animator.StringToHash("Fly");
        animatorDefeatHash = Animator.StringToHash("Defeat");
        animatorGroundedHash = Animator.StringToHash("IsGrounded");
        animatorRunningHash = Animator.StringToHash("IsRunning");
        animatorAttackingHash = Animator.StringToHash("IsAttacking");
        animatorAttackIdHash = Animator.StringToHash("AttackAnimationID");
        animatorDefeatIdHash = Animator.StringToHash("DefeatAnimationID");
    }
}