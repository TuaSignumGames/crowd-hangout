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
    public float attackDistance;
    public float reloadingTime;
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

    private int weaponIndex;

    private bool isAttackRequested;
    private bool isTargetReachable;

    public float Power { get { return damageRate; } set { damageRate = value; damage = damageRate * reloadingTime; } }

    public int WeaponID => weaponIndex;

    public Weapon Apply(HumanController ownerHuman, int weaponIndex)
    {
        this.ownerHuman = ownerHuman;
        this.weaponIndex = weaponIndex;

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
            projectiles.Add(new Projectile(i == 0 ? projectileGameObject : GameObject.Instantiate(projectileGameObject, projectileGameObject.transform.parent), ammoContainer.transform, disableOnImpact));
        }

        return projectiles;
    }

    public class Projectile
    {
        private GameObject gameObject;

        private TrailRenderer trailRenderer;

        private Transform originTransform;

        private Vector3 targetPoint;
        private Vector3 targetVector;
        private Vector3 velocityDelta;

        private float displacementDelta;

        private float targetPathLength;
        private float actualPathLength;

        private bool disableOnImpact;

        private bool isLaunched;

        private System.Action onPathComplete;

        public bool IsLaunched => isLaunched;

        public Projectile(GameObject gameObject, Transform originTransform, bool disableOnImpact)
        {
            this.gameObject = gameObject;
            this.originTransform = originTransform;
            this.disableOnImpact = disableOnImpact;

            trailRenderer = gameObject.GetComponent<TrailRenderer>();
        }

        public void Update()
        {
            gameObject.transform.position += velocityDelta;

            actualPathLength += displacementDelta;

            if (actualPathLength > targetPathLength)
            {
                onPathComplete();

                if (disableOnImpact)
                {
                    Disable();
                }
            }
        }

        public Projectile Launch(Vector3 point, float speed, System.Action onPointReached)
        {
            gameObject.transform.position = originTransform.position;

            targetVector = point - gameObject.transform.position;

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

        private void Disable()
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
