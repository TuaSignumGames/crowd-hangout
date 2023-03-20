using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
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
    public float projectileSpeed;
    [Space]
    public float directionAngularOffset;
    [Space]
    public int attackAnimationID;
    public ParticleSystem attackVFX;

    private HumanController ownerHuman;

    private List<Projectile> projectiles;
    private Pool<Projectile> projectilePool;

    private GameObject projectileGameObject;

    private TransformData modelRestTransformData;

    private float damage;

    private float availableAttackTime;

    private float sqrAttackDistance;
    private float sqrDistanceToTarget;

    private int ammoPoolSize;

    private bool isTargetReachable;

    public Weapon Apply(HumanController ownerHuman)
    {
        this.ownerHuman = ownerHuman;

        damage = damageRate * reloadingTime;

        sqrAttackDistance = attackDistance * attackDistance;

        if (ammoContainer && projectileSpeed > 0)
        {
            ammoPoolSize = Mathf.CeilToInt(attackDistance * reloadingTime / projectileSpeed) * 2;

            projectilePool = new Pool<Projectile>(GenerateProjectiles(ammoPoolSize));
        }

        modelRestTransformData = new TransformData(weaponModel.transform, Space.Self);

        weaponContainer.SetActive(true);

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
                weaponModel?.transform.SetData(new TransformData());

                if (projectilePool == null)
                {
                    human.Damage(damage, ownerHuman);
                }
                else
                {
                    projectilePool.Eject().Launch(human.transform.position, projectileSpeed, () => human.Damage(damage, ownerHuman));
                }

                if (attackVFX)
                {
                    attackVFX.Play();
                }

                availableAttackTime = Time.timeSinceLevelLoad + reloadingTime;

                return true;
            }
            else
            {
                weaponModel?.transform.SetData(modelRestTransformData);
            }

            return false;
        }

        return false;
    }

    public bool IsTargetReachable(Vector3 position)
    {
        sqrDistanceToTarget = (position - ownerHuman.transform.position).GetPlanarSqrMagnitude(Axis.Y);

        return isTargetReachable = sqrDistanceToTarget < sqrAttackDistance;
    }

    private List<Projectile> GenerateProjectiles(int count)
    {
        projectiles = new List<Projectile>();

        projectileGameObject = ammoContainer.transform.GetChildren()[0].gameObject;

        for (int i = 0; i < count; i++)
        {
            projectiles.Add(new Projectile(i == 0 ? projectileGameObject : GameObject.Instantiate(projectileGameObject, projectileGameObject.transform.parent), ammoContainer.transform));
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

        private bool isLaunched;

        private Action onPathComplete;

        public bool IsLaunched => isLaunched;

        public Projectile(GameObject gameObject, Transform originTransform)
        {
            this.gameObject = gameObject;
            this.originTransform = originTransform; 

            trailRenderer = gameObject.GetComponent<TrailRenderer>();
        }

        public void Update()
        {
            gameObject.transform.position += velocityDelta;

            actualPathLength += displacementDelta;

            if (actualPathLength > targetPathLength)
            {
                onPathComplete();

                Disable();
            }
        }

        public Projectile Launch(Vector3 point, float speed, Action onPointReached)
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
