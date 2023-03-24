using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattlePathStage
{
    private GameObject gameObject;

    private List<Transform> stageContent;
    private List<Transform> platformContent;

    private Transform guardContainer;

    private TextMeshPro rewardText;

    private HumanController[] guardians;

    private HumanController guardianInstance;

    private Crowd guardCrew;

    private float reward;

    private int guardLineCapacity = 10;

    public Crowd GuardCrew => guardCrew;

    public Vector3 position => gameObject.transform.position;
    public Vector3 size => stageContent[0].localScale;

    public BattlePathStage(GameObject stageGameObject, bool isEven)
    {
        gameObject = stageGameObject;

        stageContent = gameObject.transform.GetChildren();
        platformContent = stageContent[0].GetChildren();

        platformContent[0].gameObject.SetActive(!isEven);
        platformContent[1].gameObject.SetActive(isEven);

        guardContainer = stageContent[1];

        rewardText = platformContent[2].GetComponent<TextMeshPro>();
    }

    public void Initialize(Vector3 position, float rewardValue)
    {
        gameObject.transform.position = position;

        reward = rewardValue;

        rewardText.text = $"${reward:N0}";
    }

    public void GenerateGuard(int guardiansCount, float damageRate)
    {
        guardians = new HumanController[guardiansCount];

        int linesCount = Mathf.CeilToInt(guardiansCount / (float)guardLineCapacity);
        int availableGuardiansCount = guardiansCount;

        int guardLineSize = 0;

        float lineShiftDistance = 0;

        for (int i = 0; i < linesCount; i++)
        {
            guardLineSize = i < linesCount - 1 ? guardLineCapacity : availableGuardiansCount; //Mathf.Clamp(availableGuardiansCount, 0, guardLineCapacity);

            lineShiftDistance = (guardLineSize - 1) / 2f;

            for (int j = 0; j < guardLineSize; j++)
            {
                guardians[i * guardLineCapacity + j] = InstantiateGuardian(guardContainer.position + new Vector3(-i, 0, j - lineShiftDistance));
            }

            availableGuardiansCount -= guardLineSize;
        }

        float damageRatePool = damageRate;
        float averageDamageRate = damageRate / guardiansCount;

        List<float> damageRatePortions = new List<float>();
        List<int> selectedWeaponIndices = new List<int>();

        int topWeaponID = WorldManager.humanPrefab.weaponSettings.Count - 2; //WorldManager.GetWeaponID(GameManager.WeaponUpgradeValue);

        for (int i = 0; i < guardiansCount; i++)
        {
            damageRatePortions.Add(i < guardiansCount - 1 ? averageDamageRate * 1f : damageRatePool);
            selectedWeaponIndices.Add(Random.Range(0, topWeaponID + 1));

            damageRatePool = Mathf.Clamp(damageRatePool - damageRatePortions.GetLast(), 0, float.MaxValue);
        }

        damageRatePortions.Sort((a, b) => a.CompareTo(b));
        selectedWeaponIndices.Sort((a, b) => a.CompareTo(b));

        /*
        for (int i = 0; i < guardiansCount; i++)
        {
            Debug.Log($" W[{selectedWeaponIndices[i]}] : P[{powerPortions[i]}]");
        }
        */

        List<HumanController> guardiansPool = new List<HumanController>(guardians);

        for (int i = 0; i < guardiansCount; i++)
        {
            guardiansPool.CutRandom().SetWeapon(selectedWeaponIndices[i], damageRatePortions[i]);
        }

        guardCrew = new Crowd(guardians);
    }

    private HumanController InstantiateGuardian(Vector3 position)
    {
        guardianInstance = GameObject.Instantiate(WorldManager.humanPrefab, position, Quaternion.identity, guardContainer);

        guardianInstance.Initialize(HumanTeam.Red);
        guardianInstance.MotionSimulator.SetGround(LevelGenerator.Instance.BattlePath.transform.position.y - guardianInstance.components.animator.transform.localPosition.y);
        guardianInstance.DropToBattle(Vector3.zero, -Vector3.right);

        return guardianInstance;
    }
}