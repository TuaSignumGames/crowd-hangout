using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePath
{
    public GameObject gameObject;

    public Transform transform;

    public List<BattlePathStage> stages;

    public Vector3 position => transform.position;

    public BattlePath(GameObject pathGameObject)
    {
        gameObject = pathGameObject;

        transform = gameObject.transform;

        stages = new List<BattlePathStage>();
    }

    public BattlePathStage DefineStage(Vector3 position)
    {
        for (int i = 0; i < stages.Count; i++)
        {
            if (position.x < stages[i].position.x)
            {
                return stages[Mathf.Clamp(i - 1, 0, stages.Count - 1)];
            }
        }

        return null;
    }
}
