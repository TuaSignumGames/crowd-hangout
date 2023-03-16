using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuffler : MonoBehaviour
{
    public List<Transform> transforms;

    private List<TransformData> transformsData;

    private void Start()
    {
        Shuffle();
    }

    public void Shuffle()
    {
        transformsData = new List<TransformData>();

        for (int i = 0; i < transforms.Count; i++)
        {
            transformsData.Add(new TransformData(transforms[i], Space.Self));
        }

        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i].SetData(transformsData.CutRandom());
        }
    }
}
