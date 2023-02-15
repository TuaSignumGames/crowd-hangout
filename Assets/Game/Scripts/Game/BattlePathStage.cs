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

    private Crowd guardCrew;

    private float reward;

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

    public void Initialize(Vector3 position, float rewardValue, GameObject guardPrefab)
    {
        gameObject.transform.position = position;

        reward = rewardValue;

        rewardText.text = $"${reward:N0}";

        if (guardPrefab)
        {
            guardCrew = new Crowd(GameObject.Instantiate(guardPrefab, guardContainer).GetComponentsInChildren<HumanController>());

            foreach (HumanController member in guardCrew.Members)
            {
                member.Initialize(100f, 0);

                member.DropToBattle(Vector3.zero);
            }
        }
    }
}
