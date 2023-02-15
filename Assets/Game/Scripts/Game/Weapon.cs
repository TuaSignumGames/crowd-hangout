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

    private float damage;

    private float availableAttackTime;

    private float sqrAttackDistance;
    private float sqrDistanceToTarget;

    private int ammoPoolSize;

    public Weapon Apply()
    {
        damage = damageRate * reloadingTime;

        sqrAttackDistance = attackDistance * attackDistance;

        if (ammoContainer && projectileSpeed > 0)
        {
            ammoPoolSize = Mathf.CeilToInt(attackDistance * reloadingTime / projectileSpeed) * 2;

            projectilePool = new Pool<Projectile>(GenerateProjectiles(ammoPoolSize));
        }

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
            sqrDistanceToTarget = (human.transform.position - ownerHuman.transform.position).GetPlanarSqrMagnitude(Axis.Y);

            if (sqrDistanceToTarget <= sqrAttackDistance)
            {
                if (projectilePool == null)
                {
                    human.Damage(damage);
                }
                else
                {
                    projectilePool.Eject().Launch(human.transform.position, projectileSpeed, () => human.Damage(damage));
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
                return false;
            }
        }

        return true;
    }

    private List<Projectile> GenerateProjectiles(int count)
    {
        projectiles = new List<Projectile>();

        projectileGameObject = ammoContainer.transform.GetChildren()[0].gameObject;

        for (int i = 0; i < count; i++)
        {
            projectiles.Add(new Projectile(i == 0 ? projectileGameObject : GameObject.Instantiate(projectileGameObject, projectileGameObject.transform.parent)));
        }

        return projectiles;
    }

    public class Projectile
    {
        private GameObject gameObject;

        private TrailRenderer trailRenderer;

        private Vector3 targetPoint;
        private Vector3 targetVector;
        private Vector3 velocityDelta;

        private float displacementDelta;

        private float targetPathLength;
        private float actualPathLength;

        private bool isLaunched;

        private Action onPathComplete;

        public bool IsLaunched => isLaunched;

        public Projectile(GameObject gameObject)
        {
            this.gameObject = gameObject;

            trailRenderer = gameObject.GetComponent<TrailRenderer>();
        }

        public void Update()
        {
            gameObject.transform.position += velocityDelta;

            if (actualPathLength > targetPathLength)
            {
                onPathComplete();

                Disable();
            }
        }

        public Projectile Launch(Vector3 point, float speed, Action onPointReached)
        {
            targetVector = point - gameObject.transform.position;

            targetPathLength = velocityDelta.magnitude;

            velocityDelta = targetVector.normalized * speed * Time.fixedDeltaTime;

            displacementDelta = velocityDelta.magnitude;

            onPathComplete = onPointReached;

            isLaunched = true;

            return this;
        }

        private void Disable()
        {
            if (trailRenderer)
            {
                trailRenderer.Clear();

                trailRenderer.enabled = false;
            }

            isLaunched = false;

            gameObject.SetActive(false);
        }
    }
}
