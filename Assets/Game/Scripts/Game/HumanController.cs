using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HumanAnimationType { Running, Flying, Attacking, Falling, Dying }

public class HumanController : MonoBehaviour
{
    public new BoxCollider collider;
    public Animator animator;
    [Space]
    public HumanMotionSettings motionSettings;
    public List<Weapon> weaponSettings;
    [Space]
    public HumanRig rigSettings;

    public Crowd actualCrowd;
    public HumanPose actualPose;

    private HumanAI ai;

    private MotionSimulator motionSimulator;

    private Weapon currentWeapon;

    private Vector3 motionVector;

    private float healthPoints = 100f;

    private float actualSpeed;

    private float targetPointSqrRadius;

    public HumanAI AI => ai;

    public static HumanPose defaultPose;

    public static int animatorFlyHash;
    public static int animatorDefeatHash;
    public static int animatorGroundedHash;
    public static int animatorRunningHash;
    public static int animatorAttackingHash;
    public static int animatorSpeedFactorHash;
    public static int animatorAttackIdHash;
    public static int animatorDefeatIdHash;

    private bool isFree = true;

    private bool inBattle;

    public bool IsAlive => healthPoints > 0;

    public void Initialize(bool isFree)
    {
        this.isFree = isFree;
    }

    private void Start()
    {
        motionSimulator = new MotionSimulator(transform, LevelGenerator.Instance.BattlePath.position.y - animator.transform.localPosition.y, MonoUpdateType.FixedUpdate);
    }

    private void FixedUpdate()
    {
        if (isFree)
        {
            motionSimulator.Update();
        }
    }

    private void LateUpdate()
    {
        if (isFree)
        {
            if (inBattle)
            {
                ai.Update();

                animator.SetBool(animatorGroundedHash, motionSimulator.IsGrounded);
            }
        }
    }

    public bool MoveTo(Vector3 position)
    {
        if (motionSimulator.IsGrounded)
        {
            motionVector = position - transform.position;

            transform.forward = motionVector.GetPlanarDirection(Axis.Y);

            actualSpeed = motionVector.GetPlanarSqrMagnitude(Axis.Y) > targetPointSqrRadius ? motionSettings.runSpeed : 0;

            motionSimulator.velocity = transform.forward * actualSpeed;

            animator.SetFloat(animatorSpeedFactorHash, Mathf.Clamp01(actualSpeed));

            return actualSpeed == 0;
        }

        return false;
    }

    public bool Attack(HumanController human)
    {
        if (human.IsAlive)
        {
            transform.forward = (human.transform.position - transform.position).normalized;

            return currentWeapon.Attack(human);
        }

        return false;
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

    public void DropFromCell(Vector3 impulse)
    {
        collider.enabled = false;

        transform.SetParent(null);

        motionSimulator.velocity = impulse;

        PlayAnimation(HumanAnimationType.Falling);

        isFree = true;
    }

    public void DropToBattle(Vector3 velocity)
    {
        ai = new HumanAI(this);

        DropFromCell(velocity);

        PlayAnimation(HumanAnimationType.Flying);

        transform.forward = Vector3.right;

        targetPointSqrRadius = motionSettings.targetPointRadius * motionSettings.targetPointRadius;

        inBattle = true;
    }

    public HumanPose PeekPose()
    {
        actualPose = new HumanPose(transform, rigSettings.bones);

        return actualPose;
    }

    public void Damage(float value)
    {
        healthPoints -= value;

        if (healthPoints <= 0)
        {
            Die();
        }
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

    private void Die()
    {
        PlayAnimation(HumanAnimationType.Dying);

        actualCrowd?.RemoveMember(this);
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
        animatorSpeedFactorHash = Animator.StringToHash("SpeedFactor");
        animatorAttackIdHash = Animator.StringToHash("AttackAnimationID");
        animatorDefeatIdHash = Animator.StringToHash("DefeatAnimationID");
    }
}

[System.Serializable]
public class HumanMotionSettings
{
    public float runSpeed;
    public float turnSpeed;
    [Space]
    public float targetPointRadius;
}