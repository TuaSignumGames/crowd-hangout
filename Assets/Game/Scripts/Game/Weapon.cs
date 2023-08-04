using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon
{
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

    private int id = -1;

    private bool isAttackRequested;
    private bool isTargetReachable;

    public const float aimingDurationLimit = 1.5f;

    public float Damage => damageRate * reloadingTime;

    public float Power { get { return damageRate; } set { damageRate = value; damage = damageRate * reloadingTime; } }

    public float Distance { get { return attackDistance; } set { attackDistance = value; sqrAttackDistance = value * value; } }

    public int WeaponID => id == -1 ? WorldManager.GetWeaponID(Power) : id;

    public Weapon Arm(HumanController ownerHuman)
    {
        this.ownerHuman = ownerHuman;

        id = WorldManager.GetWeaponID(Power);

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

    public void Hide()
    {
        weaponContainer.SetActive(false);
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
        if (!isAttackRequested)
        {
            isAttackRequested = true;

            availableAttackTime = Time.timeSinceLevelLoad + Random.Range(0, aimingDurationLimit);
        }

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

            if (gameObject.transform.childCount > 0)
            {
                foreach (Transform child in gameObject.transform.GetChildren())
                {
                    if (child.gameObject.name == "VFX[Impact]")
                    {
                        impactVFX = child.GetComponent<ParticleSystem>();
                    }
                }
            }
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
                    impactVFX.transform.localScale = Vector3.one;

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
