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
                stages[i].AddHumanCollectibles(humanCollectiblesPool.EjectRange(i < stages.Length - 2 ? buildingSettings.stageCapacity : remainingHumansCount));

                remainingHumansCount -= buildingSettings.stageCapacity;
            }
        }

        multicollectibleSettings.capsules = new MulticollectibleCapsule[stages.Length];

        for (int i = 0; i < stages.Length; i++)
        {
            multicollectibleSettings.capsules[i] = new MulticollectibleCapsule(stages[i].exterior, stages[i].fractures, stages[i].destructionVFX);
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
            //multicollectibleSettings.capsules[contactStageIndex - 1].BreakPartially(PlayerController.Humanball.Velocity * 0.3f, new FloatRange(5f, 15f), stages[contactStageIndex].transform.position, 1.5f);
        }

        for (int i = contactStageIndex; i < stages.Length; i++)
        {
            multicollectibleSettings.capsules[i].Break(PlayerController.Humanball.Velocity * 0.3f, new FloatRange(5f, 15f));

            if (i < stages.Length - 1)
            {
                stages[i].DropHumanCollectibles();
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

        private MulticollectibleEntity<HumanController>[] humanCollectibles;

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

        public void AddHumanCollectibles(MulticollectibleEntity<HumanController>[] humanCollectibles)
        {
            this.humanCollectibles = humanCollectibles;

            for (int i = 0; i < humanCollectibles.Length; i++)
            {
                humanCollectibles[i].Entity.gameObject.SetActive(false);
                humanCollectibles[i].Entity.transform.position = transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0.1f, 0);
            }
        }

        public void DropHumanCollectibles()
        {
            for (int i = 0; i < humanCollectibles.Length; i++)
            {
                humanCollectibles[i].Collect();

                humanCollectibles[i].Entity.Drop((humanCollectibles[i].Entity.transform.position - PlayerController.Humanball.Transform.position).normalized * PlayerController.Humanball.Velocity.magnitude, Random.insideUnitSphere.normalized * Random.Range(90f, 720f));
            }
        }
    }
}
