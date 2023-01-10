using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    public List<GameObject> gameObjects;

    private void Start()
    {
        Select();
    }

    public void Select()
    {
        gameObjects.GetRandom().SetActive(true);
    }
}
