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

    public override void Initialize(int elementsCount = 1)
    {
        base.Initialize(elementsCount);

        GenerateBuilding(elementsCount);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        for (int i = 0; i < stages.Length; i++)
        {
            stages[i].humanCountMarker?.Update();
        }
    }

    protected void GenerateBuilding(int humansCount)
    {
        stages = new BuildingStage[Mathf.CeilToInt(humansCount / (float)buildingSettings.stageCapacity) + 1];

        remainingHumansCount = humansCount;

        for (int i = 0; i < stages.Length; i++)
        {
            stages[i] = new BuildingStage(i == 0 ? buildingSettings.stage : (i == stages.Length - 1 ? buildingSettings.roof : Instantiate(buildingSettings.stage, buildingSettings.stagesContainer)), this);

            stages[i].transform.SetCoordinate(TransformComponent.Position, Axis.Y, Space.World, buildingSettings.stagesContainer.position.y + i * buildingSettings.stageHeight);

            if (i < stages.Length - 1)
            {
                stages[i].AddHumanCollectibles(humanCollectiblesPool.EjectRange(i < stages.Length - 2 ? buildingSettings.stageCapacity : remainingHumansCount));

                remainingHumansCount -= buildingSettings.stageCapacity;
            }
        }

        multicollectibleSettings.capsules = new MulticollectibleCapsule[stages.Length];

        for (int i = 0; i < stages.Length; i++)
        {
            multicollectibleSettings.capsules[i] = new MulticollectibleCapsule(stages[i].exterior, stages[i].fractures, stages[i].destructionVFX, multicollectibleSettings.capsuleScatteringSettings);
        }

        buildingHeight = 0.2f + buildingSettings.stageHeight * (stages.Length - 1) + 0.9f;

        buildingCollider = collectibleSettings.collider as BoxCollider;

        buildingCollider.center = new Vector3(0, buildingHeight / 2f, 0);
        buildingCollider.size = new Vector3(3.5f, buildingHeight, 2f);
    }

    protected override IEnumerator CollectingCoroutine()
    {
        contactStageIndex = stages.Length - 1;

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
            multicollectibleSettings.capsules[contactStageIndex - 1].BreakPartially(PlayerController.Humanball.Velocity, stages[contactStageIndex].transform.position, 2.5f);

            if (contactStageIndex == stages.Length - 1)
            {
                stages[stages.Length - 2].DropElements();
            }
        }

        for (int i = contactStageIndex; i < stages.Length; i++)
        {
            multicollectibleSettings.capsules[i].Break(PlayerController.Humanball.Velocity);

            if (i < stages.Length - 1)
            {
                stages[i].DropElements();
            }

            yield return new WaitForSeconds(buildingSettings.stageDestructionDelay);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        PlayerController.Humanball.DropHumans(0.5f);
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

        public TextMarker humanCountMarker;

        public ParticleSystem destructionVFX;

        private MulticollectibleEntity<HumanController>[] humanCollectibles;

        private BuildingMulticollectible buildingMulticollectible;

        private List<Transform> children;

        private Vector3 collectibleDropImpulse;

        public Transform transform => gameObject.transform;

        public BuildingStage(GameObject gameObject, BuildingMulticollectible buildingMulticollectible)
        {
            this.gameObject = gameObject;
            this.buildingMulticollectible = buildingMulticollectible;

            children = gameObject.transform.GetChildren();

            exterior = children[0].gameObject;
            fractures = children[1].GetGameObjectsInChildren();

            if (children.Count > 2)
            {
                destructionVFX = children[2].GetComponent<ParticleSystem>();

                if (children.Count > 3)
                {
                    humanCountMarker = new TextMarker(children[3]);
                }
            }
        }

        public void AddHumanCollectibles(MulticollectibleEntity<HumanController>[] humanCollectibles)
        {
            this.humanCollectibles = humanCollectibles;

            for (int i = 0; i < humanCollectibles.Length; i++)
            {
                humanCollectibles[i].Entity.gameObject.SetActive(false);
                humanCollectibles[i].Entity.transform.position = transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0.2f, 0);
            }

            if (humanCountMarker != null)
            {
                humanCountMarker.SetValue(humanCollectibles.Length.ToString());
            }
        }

        public void DropElements()
        {
            for (int i = 0; i < humanCollectibles.Length; i++)
            {
                humanCollectibles[i].Entity.EjectFromCell();

                buildingMulticollectible.DropElement(humanCollectibles[i].Element, transform.position, PlayerController.Humanball.Velocity);
            }

            if (humanCountMarker != null)
            {
                humanCountMarker.SetActive(false);
            }
        }
    }
}
