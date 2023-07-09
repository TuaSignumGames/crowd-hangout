using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlePathCellType { None, Ground, Obstacle }

public class BattlePathCell
{
    private BattlePathCellType type;

    private GameObject[] contents;

    private GameObject pillar;

    private Vector3 position;

    private Vector2Int address;

    public BattlePathCellType Type => type;

    public Vector3 Position => position;

    public Vector2Int Address => address;

    public BattlePathCell(GameObject gameObject, Vector2Int address, BattlePathCellType type)
    {
        this.type = type;
        this.address = address;

        position = gameObject.transform.position;

        pillar = gameObject.transform.GetChild(0).GetChild(1).gameObject;

        contents = gameObject.transform.GetChild(1).GetGameObjectsInChildren();
    }
}
