using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HumanTeam { Neutral, Yellow, Red }
public enum HumanAnimationType { Running, Flying, Attacking, Falling, Dying }

public class HumanController : MonoBehaviour
{
    public static HumanController selectedHuman;

    public HumanControllerComponents components;
    [Space]
    public HumanTeam team;
    public List<HumanTeamInfo> teamSettings;
    public HumanMotionSettings motionSettings;
    public List<Weapon> weaponSettings;
    [Space]
    public HumanRig rigSettings;
    public HumanPoseSettings poseSettings;
    [Space]
    public ProgressBar healthBar; 

    public Crowd actualCrowd;

    private HumanPose actualPose;
    private HumanTeamInfo actualTeamInfo;

    private HumanAI ai;

    private MotionSimulator motionSimulator;
    private AnimatorListener attackAnimatorListener;

    private Weapon currentWeapon;

    private AnimatorStateInfo currentAnimationInfo;

    private Vector3 motionVector;

    private float healthPoints;
    private float healthCapacity;

    private float targetSpeed;
    private float actualSpeed;

    private float targetPointSqrRadius;

    public HumanAI AI => ai;
    public HumanPose ActualPose => actualPose;

    public MotionSimulator MotionSimulator => motionSimulator;
    public AnimatorListener AttackAnimatorListener => attackAnimatorListener;

    public Weapon Weapon => currentWeapon;

    public static int animatorFlyHash;
    public static int animatorDefeatHash;
    public static int animatorGroundedHash;
    public static int animatorRunningHash;
    public static int animatorAttackingHash;
    public static int animatorSpeedFactorHash;
    public static int animatorAttackIdHash;
    public static int animatorDefeatIdHash;

    private bool isInitialized;

    private bool inBattle;

    [HideInInspector] public bool isFree = false;

    public bool IsInitialized => isInitialized;

    public bool IsAlive => healthPoints > 0;

    public void Initialize(HumanTeam team, float health, int weaponIndex = 0)
    {
        InitializeAnimatorHashes();

        motionSimulator = new MotionSimulator(transform, -1000f, MonoUpdateType.FixedUpdate);
        attackAnimatorListener = new AnimatorListener(components.animator);

        actualTeamInfo = teamSettings[1];

        if (!(this.team == HumanTeam.Yellow && team == HumanTeam.Yellow))
        {
            SetTeam(this.team = team);
        }

        SetWeapon(weaponSettings[weaponIndex]);

        healthCapacity = healthPoints = health;

        isInitialized = true;
    }

    public void Initialize(float health, int weaponIndex = 0)
    {
        Initialize(team, health, weaponIndex);
    }

    public void Initialize(HumanTeam team, int weaponIndex = 0)
    {
        Initialize(team, 100f, weaponIndex);
    }

    public void Initialize()
    {
        Initialize(team, 100f);
    }

    private void Start()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    private void FixedUpdate()
    {
        if (isFree)
        {
            if (inBattle)
            {
                actualSpeed = Mathf.Lerp(actualSpeed, targetSpeed, motionSettings.speedLerpingFactor);

                currentWeapon.Update();
            }
            else
            {

            }

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

                healthBar.Update();

                components.animator.SetBool(animatorGroundedHash, motionSimulator.IsGrounded);
            }
            else
            {
                motionSimulator.SetGround(LevelGenerator.Instance.GetBlockPair(transform.position).floorBlock.transform.position.y);
            }

            if (motionSimulator.IsGrounded)
            {
                motionSimulator.angularVelocity = new Vector3();

                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            }
        }
    }

    public void FocusOn(Vector3 point, float angularOffset = 0, bool inPlane = false)
    {
        if (inPlane)
        {
            transform.forward = (point - transform.position).GetPlanarDirection(Axis.Y);

            transform.eulerAngles += new Vector3(0, angularOffset, 0);
        }
        else
        {
            transform.forward = (point - transform.position).normalized;

            transform.eulerAngles += transform.up * angularOffset;
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
                FocusOn(human.transform.position, currentWeapon.directionAngularOffset, true);

                if (currentWeapon.animationRelated)
                {
                    if (currentWeapon.IsTargetReachable(human.transform.position))
                    {
                        PlayAnimation(HumanAnimationType.Attacking);

                        currentWeapon.AttackWithAnimation(human);
                    }
                }
                else
                {
                    if (currentWeapon.Attack(human))
                    {
                        PlayAnimation(HumanAnimationType.Attacking);
                    }
                }
            }
        }
    }

    public bool MoveTo(Vector3 position)
    {
        if (motionSimulator.IsGrounded)
        {
            motionVector = position - transform.position;

            currentWeapon.Lower();

            if (motionVector.sqrMagnitude > targetPointSqrRadius)
            {
                transform.forward = motionVector.GetPlanarDirection(Axis.Y);

                targetSpeed = motionVector.GetPlanarSqrMagnitude(Axis.Y) > targetPointSqrRadius ? motionSettings.runSpeed : 0;

                components.animator.SetBool(animatorAttackingHash, false);

                UpdateMotion();

                return targetSpeed == 0;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public void Stop()
    {
        targetSpeed = 0;

        UpdateMotion();

        components.animator.SetBool(animatorAttackingHash, false);
    }

    public void PlaceInCell(HumanballCell cell, bool playVFX = true)
    {
        isFree = false;

        components.animator.enabled = false;

        transform.SetParent(cell.transform);

        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = new Vector3(0, 0, cell.Layer.IsBaked ? 0 : Random.Range(0, 360f));
        transform.localScale = Vector3.one;

        motionSimulator.enabled = false;

        components.collider.enabled = true;

        if (playVFX)
        {
            actualTeamInfo.impactVFX.Play();
        }
    }

    public void EjectFromCell(bool playVFX = false)
    {
        components.collider.enabled = false;

        transform.SetParent(null);
        transform.localScale = Vector3.one;

        motionSimulator.velocityMultiplier = 1f;
        motionSimulator.enabled = true;

        if (playVFX)
        {
            actualTeamInfo.impactVFX.Play();
        }

        PlayAnimation(HumanAnimationType.Falling);

        isFree = true;
    }

    public void Drop(Vector3 impulse, Vector3 angularMomentum, bool playVFX = false)
    {
        EjectFromCell(playVFX);

        motionSimulator.velocity = impulse;
        motionSimulator.angularVelocity = angularMomentum;
    }

    public void DropToBattle(Vector3 velocity)
    {
        ai = new HumanAI(this);

        motionSimulator.SetGround(LevelGenerator.Instance.BattlePath.position.y - components.animator.transform.localPosition.y);

        motionSimulator.rotationEnabled = false;

        Drop(velocity, Vector3.zero);

        transform.forward = Vector3.right;

        targetPointSqrRadius = motionSettings.targetPointRadius * motionSettings.targetPointRadius;

        PlayAnimation(HumanAnimationType.Flying);

        inBattle = true;
    }

    public HumanPose PeekPose()
    {
        actualPose = new HumanPose(rigSettings.bones);

        return actualPose;
    }

    public void Damage(float value, HumanController agressor = null)
    {
        healthPoints -= value;

        if (actualTeamInfo != null)
        {
            if (actualTeamInfo.impactVFX)
            {
                actualTeamInfo.impactVFX.Play();
            }
        }

        healthBar.SetValue(healthPoints / healthCapacity);

        if (agressor)
        {
            ai.SetEnemy(agressor);
        }

        if (healthPoints <= 0)
        {
            Die();
        }
    }

    public void Die(bool disableController = true)
    {
        components.animator.SetBool(animatorAttackingHash, false);

        PlayAnimation(HumanAnimationType.Dying);

        SetTeam(HumanTeam.Neutral);

        actualCrowd?.RemoveMember(this);

        enabled = !disableController;
    }

    public void SetPose(HumanPose pose)
    {
        actualPose = pose;

        for (int i = 0; i < actualPose.boneTransformDatas.Length; i++)
        {
            rigSettings.bones[i].transform.SetData(actualPose.boneTransformDatas[i]);
        }
    }

    public void SetWeapon(Weapon weapon)
    {
        currentWeapon = weapon.Apply(this, weaponSettings.IndexOf(weapon));
    }

    public void SetWeapon(int index)
    {
        currentWeapon = weaponSettings[index].Apply(this, index);
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
                PlayerController.Humanball.UnstickHuman(this);

                PlayerController.Humanball.Bump(transform.position);
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
                    weaponSettings[i].title = $"{weaponSettings[i].weaponContainer.name}  [P:{weaponSettings[i].damageRate * weaponSettings[i].attackDistance}]";
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