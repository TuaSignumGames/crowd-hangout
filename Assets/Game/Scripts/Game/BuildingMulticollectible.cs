using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingMulticollectible : HumanMulticollectible
{
    [Space]
    public BuildingSettings buildingSettings;

    protected BoxCollider buildingCollider;

    protected BuildingStage[] stages;

    protected BuildingStage newStage;

    protected float buildingHeight;

    protected int stageHumansCount;
    protected int remainingHumansCount;

    protected int contactStageIndex;

    protected override void GenerateElements(int count)
    {
        base.GenerateElements(count);

        GenerateBuilding(count);
    }

    protected override void PullElements()
    {
        base.PullElements();

        for (int i = 0; i < elements.Length; i++)
        {
            if (!elements[i].IsCollected)
            {
                if (elements[i].Pull(PlayerController.Humanball.Transform))
                {
                    PlayerController.Humanball.StickHuman(humans[i], false);
                }
            }
        }
    }

    protected void GenerateBuilding(int humansCount)
    {
        stages = new BuildingStage[Mathf.CeilToInt(humansCount / (float)buildingSettings.stageCapacity) + 1];

        remainingHumansCount = humansCount;

        for (int i = 0; i < stages.Length; i++)
        {
            stages[i] = new BuildingStage(i == 0 ? buildingSettings.stage : (i == stages.Length - 1 ? buildingSettings.roof : Instantiate(buildingSettings.stage, buildingSettings.stagesContainer)));

            stages[i].transform.SetCoordinate(TransformComponent.Position, Axis.Y, Space.World, buildingSettings.stagesContainer.position.y + i * buildingSettings.stageHeight);

            if (i < stages.Length - 1)
            {
                humans.AddRange(stages[i].GenerateHumans(humanPrefab, humanContainer, i < stages.Length - 2 ? buildingSettings.stageCapacity : remainingHumansCount));

                remainingHumansCount -= buildingSettings.stageCapacity;
            }
        }

        multicollectibleSettings.capsules = new MulticollectibleCapsule[stages.Length];

        for (int i = 0; i < stages.Length; i++)
        {
            multicollectibleSettings.capsules[i] = new MulticollectibleCapsule(stages[i].exterior, stages[i].fractures, stages[i].destructionVFX);
        }

        for (int i = 0; i < humans.Count; i++)
        {
            elements[i] = new MulticollectibleElement(humans[i].MotionSimulator, multicollectibleSettings.CollectibleSpeed, multicollectibleSettings.CollectibleAcceleration, multicollectibleSettings.CollectiblePullingDelay, 1f);
        }

        buildingHeight = buildingSettings.stageHeight * (stages.Length - 1);

        buildingCollider = collectibleSettings.collider as BoxCollider;

        buildingCollider.center = new Vector3(0, buildingHeight / 2f, 0);
        buildingCollider.size = new Vector3(3.5f, buildingHeight, 2f);
    }

    protected override IEnumerator CollectingCoroutine()
    {
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].transform.position.y > contactPoint.y)
            {
                contactStageIndex = Mathf.Clamp(i - 1, 0, stages.Length - 1);

                break;
            }
        }

        if (contactStageIndex > 0)
        {
            multicollectibleSettings.capsules[contactStageIndex - 1].BreakPartially(1.5f);
        }

        for (int i = contactStageIndex; i < stages.Length; i++)
        {
            multicollectibleSettings.capsules[i].Break(PlayerController.Humanball.Velocity * 0.3f, new FloatRange(5f, 15f));

            if (i < stages.Length - 1)
            {
                stages[i].DropHumans();
            }

            yield return new WaitForSeconds(buildingSettings.stageDestructionDelay);
        }
    }

    [System.Serializable]
    public class BuildingSettings
    {
        public Transform stagesContainer;
        [Space]
        public GameObject roof;
        public GameObject stage;
        public float stageHeight;
        [Space]
        public int stageCapacity;
        [Space]
        public float stageDestructionDelay;
    }

    protected class BuildingStage
    {
        public GameObject gameObject;

        public GameObject exterior;

        public GameObject[] fractures;

        public ParticleSystem destructionVFX;

        private HumanController[] humans;

        private List<Transform> children;

        public Transform transform => gameObject.transform;

        public BuildingStage(GameObject gameObject)
        {
            this.gameObject = gameObject;

            children = gameObject.transform.GetChildren();

            exterior = children[0].gameObject;
            fractures = children[1].GetGameObjectsInChildren();

            if (children.Count > 2)
            {
                destructionVFX = children[2].GetComponent<ParticleSystem>();
            }

            Debug.Log($" - Fractures added: {fractures.Length}");
        }

        public HumanController[] GenerateHumans(HumanController humanPrefab, Transform container, int count)
        {
            humans = new HumanController[count];

            for (int i = 0; i < count; i++)
            {
                humans[i] = GameObject.Instantiate(humanPrefab, transform.position + Random.insideUnitSphere, Quaternion.identity, container);

                humans[i].Initialize();

                humans[i].components.collider.enabled = false;

                humans[i].gameObject.SetActive(false);
            }

            return humans;
        }

        public void DropHumans()
        {
            for (int i = 0; i < humans.Length; i++)
            {
                humans[i].gameObject.SetActive(true);

                humans[i].Drop((humans[i].transform.position - PlayerController.Humanball.Transform.position).normalized * PlayerController.Humanball.Velocity.magnitude, Random.insideUnitSphere.normalized * Random.Range(30f, 120f));
            }
        }
    }
}
