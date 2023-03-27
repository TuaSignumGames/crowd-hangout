using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon
{
    public static float TopWeaponPower { get { return PlayerPrefs.GetFloat("WPN.TP", WorldManager.GetWeaponPower(0)); } set { PlayerPrefs.SetFloat("WPN.TP", value); } }

    [HideInInspector]
    public string title;

    public GameObject weaponContainer;
    public GameObject weaponModel;
    [Space]
    public GameObject ammoContainer;
    [Space]
    public float damageRate;
    public float reloadingTime;
    [Space]
    public float attackDistance;
    public float parabolaHeight;
    [Space]
    public float projectileSpeed;
    public bool disableOnImpact = true;
    [Space]
    public float directionAngularOffset;
    [Space]
    public int attackAnimationID;
    [Space]
    public bool animationRelated;
    public string animationName;
    public float keyframeTime;
    [Space]
    public ParticleSystem attackVFX;

    private HumanController ownerHuman;

    private List<Projectile> projectiles;
    private Pool<Projectile> projectilePool;

    private GameObject projectileGameObject;

    private TransformData modelRestTransformData;

    private float damage;

    private float previousAttackTime;
    private float availableAttackTime;

    private float sqrAttackDistance;
    private float sqrDistanceToTarget;

    private int ammoPoolSize;

    private bool isAttackRequested;
    private bool isTargetReachable;

    public float Power { get { return damageRate; } set { damageRate = value; damage = damageRate * reloadingTime; } }

    public int WeaponID => WorldManager.GetWeaponID(Power);

    public Weapon Apply(HumanController ownerHuman)
    {
        this.ownerHuman = ownerHuman;

        damage = damageRate * reloadingTime;

        sqrAttackDistance = attackDistance * attackDistance;

        if (ammoContainer && projectileSpeed > 0)
        {
            ammoPoolSize = Mathf.CeilToInt(attackDistance / (projectileSpeed * reloadingTime)) + 1;

            projectilePool = new Pool<Projectile>(GenerateProjectiles(ammoPoolSize));
        }

        if (weaponModel)
        {
            modelRestTransformData = new TransformData(weaponModel.transform, Space.Self);
        }

        weaponContainer.SetActive(true);

        if (animationRelated)
        {
            ownerHuman.AttackAnimatorListener.ListenToState(animationName);
        }

        return this;
    }

    public void Update()
    {
        if (projectiles != null)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i].IsLaunched)
                {
                    projectiles[i].Update();
                }
            }
        }
    }

    public bool Attack(HumanController human)
    {
        if (Time.timeSinceLevelLoad > availableAttackTime)
        {
            if (ownerHuman.team == HumanTeam.Yellow)
            {
                //Debug.Log($"");
            }

            if (isTargetReachable)
            {
                if (weaponModel)
                {
                    weaponModel.transform.Reset();
                }

                if (projectilePool == null)
                {
                    human.Damage(damage, ownerHuman);
                }
                else
                {
                    projectilePool.Eject().Launch(human.transform.position + Random.insideUnitSphere * 0.2f, projectileSpeed, () => human.Damage(damage, ownerHuman));
                }

                if (attackVFX)
                {
                    attackVFX.Play();
                }

                AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);

                availableAttackTime = Time.timeSinceLevelLoad + reloadingTime;

                return true;
            }

            return false;
        }

        return false;
    }

    public void AttackImmediate(HumanController human)
    {
        if (weaponModel)
        {
            weaponModel.transform.Reset();
        }

        if (projectilePool == null)
        {
            human.Damage(damageRate * Mathf.Clamp(Time.timeSinceLevelLoad - previousAttackTime, 0, reloadingTime), ownerHuman);
        }
        else
        {
            projectilePool.Eject().Launch(human.transform.position + Random.insideUnitSphere * 0.2f, projectileSpeed, () => human.Damage(damage, ownerHuman));
        }

        if (attackVFX)
        {
            attackVFX.Play();
        }

        previousAttackTime = Time.timeSinceLevelLoad;
    }

    public void AttackWithAnimation(HumanController human)
    {
        if (ownerHuman.AttackAnimatorListener.IsFrameReached(keyframeTime))
        {
            AttackImmediate(human);
        }
    }

    public void Lower()
    {
        isAttackRequested = false;

        ownerHuman.AttackAnimatorListener.Reset();

        if (weaponModel)
        {
            weaponModel.transform.SetData(modelRestTransformData);
        }
    }

    public void ClearProjectiles()
    {
        if (projectiles != null)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Disable();
            }
        }
    }

    public bool IsTargetReachable(Vector3 position)
    {
        sqrDistanceToTarget = (position - ownerHuman.transform.position).GetPlanarSqrMagnitude(Axis.Y);

        return isTargetReachable = sqrDistanceToTarget <= sqrAttackDistance;
    }

    private List<Projectile> GenerateProjectiles(int count)
    {
        projectiles = new List<Projectile>();

        projectileGameObject = ammoContainer.transform.GetChildren()[0].gameObject;

        for (int i = 0; i < count; i++)
        {
            projectiles.Add(new Projectile(this, i == 0 ? projectileGameObject : GameObject.Instantiate(projectileGameObject, projectileGameObject.transform.parent), ammoContainer.transform));
        }

        return projectiles;
    }

    public class Projectile
    {
        private Weapon weapon;

        private GameObject gameObject;

        private TrailRenderer trailRenderer;

        private ParticleSystem impactVFX;

        private Transform originTransform;

        private Vector3 targetPoint;
        private Vector3 actualPoint;
        private Vector3 targetVector;
        private Vector3 velocityDelta;

        private float displacementDelta;
        private float heightIncrement;

        private float targetPathLength;
        private float actualPathLength;

        private float actualParabolaHeight;

        private float progressFactor;

        private bool isLaunched;

        private System.Action onPathComplete;

        public bool IsLaunched => isLaunched;

        public Projectile(Weapon weapon, GameObject gameObject, Transform originTransform)
        {
            this.weapon = weapon;
            this.gameObject = gameObject;
            this.originTransform = originTransform;

            trailRenderer = gameObject.GetComponent<TrailRenderer>();

            impactVFX = gameObject.transform.GetComponentInChildren<ParticleSystem>();
        }

        public void Update()
        {
            actualPathLength += displacementDelta;

            actualPoint += velocityDelta;

            if (weapon.parabolaHeight > 0)
            {
                progressFactor = actualPathLength / targetPathLength;

                heightIncrement = actualParabolaHeight * Mathf.Sin(progressFactor * 3.1416f);

                gameObject.transform.position = actualPoint + new Vector3(0, heightIncrement, 0);
            }
            else
            {
                gameObject.transform.position = actualPoint;
            }

            if (actualPathLength > targetPathLength)
            {
                onPathComplete();

                if (impactVFX)
                {
                    impactVFX.transform.SetParent(null);
                    impactVFX.transform.position = gameObject.transform.position;

                    impactVFX.Play(true);
                }

                if (weapon.disableOnImpact)
                {
                    Disable();
                }
            }
        }

        public Projectile Launch(Vector3 point, float speed, System.Action onPointReached)
        {
            gameObject.transform.position = actualPoint = originTransform.position;

            targetVector = point - actualPoint;

            actualParabolaHeight = weapon.parabolaHeight * targetVector.magnitude / weapon.attackDistance;

            targetPathLength = targetVector.magnitude;
            actualPathLength = 0;

            velocityDelta = targetVector.normalized * speed * Time.fixedDeltaTime;

            displacementDelta = velocityDelta.magnitude;

            onPathComplete = onPointReached;

            if (trailRenderer)
            {
                trailRenderer.Clear();
                trailRenderer.enabled = true;
            }

            gameObject.transform.SetParent(null);
            gameObject.SetActive(true);

            isLaunched = true;

            return this;
        }

        public void Disable()
        {
            if (trailRenderer)
            {
                trailRenderer.enabled = false;
            }

            isLaunched = false;

            gameObject.SetActive(false);
        }
    }
}
