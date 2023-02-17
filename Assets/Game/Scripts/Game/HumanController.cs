using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HumanTeam { Neutral, Yellow, Red }
public enum HumanAnimationType { Running, Flying, Attacking, Falling, Dying }

public class HumanController : MonoBehaviour
{
    public HumanControllerComponents components;
    [Space]
    public HumanTeam team;
    public List<HumanTeamInfo> teamSettings;
    public HumanMotionSettings motionSettings;
    public HumanRig rigSettings;
    public List<Weapon> weaponSettings;
    [Space]
    public ProgressBar healthBar; 

    public Crowd actualCrowd;
    public HumanPose actualPose;

    private HumanTeamInfo actualTeamInfo;

    private HumanAI ai;

    private MotionSimulator motionSimulator;

    private Weapon currentWeapon;

    private Vector3 motionVector;

    private float healthPoints;
    private float healthCapacity;

    private float targetSpeed;
    private float actualSpeed;

    private float targetPointSqrRadius;

    public HumanAI AI => ai;

    public Weapon Weapon => currentWeapon;

    public static HumanPose defaultPose;

    public static int animatorFlyHash;
    public static int animatorDefeatHash;
    public static int animatorGroundedHash;
    public static int animatorRunningHash;
    public static int animatorAttackingHash;
    public static int animatorSpeedFactorHash;
    public static int animatorAttackIdHash;
    public static int animatorDefeatIdHash;

    private bool inBattle;

    [HideInInspector] public bool isFree = true;

    public bool IsAlive => healthPoints > 0;

    public void Initialize(HumanTeam team, float health, int weaponIndex = 0)
    {
        if (!(this.team == HumanTeam.Yellow && team == HumanTeam.Yellow))
        {
            SetTeam(this.team = team);
        }

        SetWeapon(weaponSettings[weaponIndex]);

        healthCapacity = healthPoints = health;

        healthBar.Initialize();

        motionSimulator = new MotionSimulator(transform, LevelGenerator.Instance.BattlePath.position.y - components.animator.transform.localPosition.y, MonoUpdateType.FixedUpdate);
    }

    public void Initialize(float health, int weaponIndex = 0)
    {
        Initialize(team, health, weaponIndex);
    }

    public void Initialize(HumanTeam team, int weaponIndex = 0)
    {
        Initialize(team, 100f, weaponIndex);
    }

    private void Start()
    {
        Initialize(team);
    }

    private void FixedUpdate()
    {
        //if (IsAlive)
        {
            if (isFree)
            {
                if (inBattle)
                {
                    actualSpeed = Mathf.Lerp(actualSpeed, targetSpeed, motionSettings.speedLerpingFactor);
                }

                motionSimulator.Update();
            }
        }
    }

    private void LateUpdate()
    {
        //if (IsAlive)
        {
            if (isFree)
            {
                if (inBattle)
                {
                    ai.Update();

                    healthBar.Update();

                    components.animator.SetBool(animatorGroundedHash, motionSimulator.IsGrounded);
                }
            }
        }
    }

    public void FocusOn(Vector3 point, bool inPlane = false)
    {
        if (inPlane)
        {
            transform.forward = (point - transform.position).GetPlanarDirection(Axis.Y);
        }
        else
        {
            transform.forward = (point - transform.position).normalized;
        }
    }

    public void Attack(HumanController human)
    {
        if (motionSimulator.IsGrounded)
        {
            targetSpeed = 0;

            UpdateMotion();

            if (human.IsAlive)
            {
                FocusOn(human.transform.position, true);

                if (currentWeapon.Attack(human))
                {
                    PlayAnimation(HumanAnimationType.Attacking);
                }
            }
        }
    }

    public bool MoveTo(Vector3 position)
    {
        if (motionSimulator.IsGrounded)
        {
            motionVector = position - transform.position;

            transform.forward = motionVector.GetPlanarDirection(Axis.Y);

            targetSpeed = motionVector.GetPlanarSqrMagnitude(Axis.Y) > targetPointSqrRadius ? motionSettings.runSpeed : 0;

            components.animator.SetBool(animatorAttackingHash, false);

            UpdateMotion();

            return targetSpeed == 0;
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
            SetPose(cell.Pose);
        }
        else
        {
            SetPose(defaultPose);
        }

        GetComponent<Collider>().enabled = true;
    }

    public void DropFromCell(Vector3 impulse)
    {
        GetComponent<Collider>().enabled = false;

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

    public void Damage(float value, HumanController agressor = null)
    {
        healthPoints -= value;

        if (actualTeamInfo != null)
        {
            actualTeamInfo.impactVFX?.Play();
        }

        healthBar.SetValue(Mathf.Clamp01(healthPoints / healthCapacity));

        if (agressor)
        {
            ai.SetEnemy(agressor);
        }

        if (healthPoints <= 0)
        {
            Die();
        }
    }

    public void SetWeapon(Weapon weapon)
    {
        currentWeapon = weapon.Apply(this);
    }

    public void SetWeapon(int index)
    {
        currentWeapon = weaponSettings[index].Apply(this);
    }

    private void UpdateMotion()
    {
        motionSimulator.velocity = transform.forward * actualSpeed;

        components.animator.SetFloat(animatorSpeedFactorHash, Mathf.Clamp01(actualSpeed));
        components.animator.SetBool(animatorRunningHash, actualSpeed > 0);
    }

    private void SetTeam(HumanTeam teamType)
    {
        actualTeamInfo = teamSettings.Find((t) => t.teamType == teamType);

        components.skinRenderer.material = actualTeamInfo.skinMaterial;

        if (actualTeamInfo.impactVFX)
        {
            actualTeamInfo.impactVFX.gameObject.SetActive(true);
        }
    }

    private void SetPose(HumanPose pose)
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
        components.animator.SetBool(animatorAttackingHash, false);

        PlayAnimation(HumanAnimationType.Dying);

        SetTeam(HumanTeam.Neutral);

        actualCrowd?.RemoveMember(this);

        enabled = false;
    }

    private void PlayAnimation(HumanAnimationType animationType)
    {
        components.animator.enabled = true;

        switch (animationType)
        {
            case HumanAnimationType.Running: components.animator.SetBool(animatorGroundedHash, true); components.animator.SetBool(animatorRunningHash, true); break;
            case HumanAnimationType.Flying: components.animator.SetTrigger(animatorFlyHash); break;
            case HumanAnimationType.Attacking: components.animator.SetInteger(animatorAttackIdHash, currentWeapon.attackAnimationID); components.animator.SetBool(animatorAttackingHash, true); break;
            case HumanAnimationType.Falling: components.animator.SetBool(animatorGroundedHash, false); break;
            case HumanAnimationType.Dying: components.animator.SetInteger(animatorAttackIdHash, Random.Range(0, 5)); components.animator.SetTrigger(animatorDefeatHash); break;
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
        if (teamSettings.Count > 0)
        {
            for (int i = 0; i < teamSettings.Count; i++)
            {
                teamSettings[i].title = teamSettings[i].teamType.ToString();
            }

            SetTeam(team);
        }

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
public struct HumanControllerComponents
{
    public BoxCollider collider;
    [Space]
    public Animator animator;
    public SkinnedMeshRenderer skinRenderer;
}

[System.Serializable]
public class HumanTeamInfo
{
    [HideInInspector]
    public string title;

    public HumanTeam teamType;
    public Material skinMaterial;
    public ParticleSystem impactVFX;
}

[System.Serializable]
public class HumanMotionSettings
{
    public float runSpeed;
    public float turnSpeed;
    public float speedLerpingFactor;
    [Space]
    public float targetPointRadius;
}