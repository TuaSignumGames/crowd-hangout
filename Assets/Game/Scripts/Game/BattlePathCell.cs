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

    private BattlePathCellAddress address;

    public BattlePathCellType Type => type;

    public Vector3 Position => position;

    public BattlePathCellAddress Address => address;

    public BattlePathCell(GameObject gameObject, BattlePathCellAddress address, BattlePathCellType type)
    {
        this.type = type;
        this.address = address;

        position = gameObject.transform.position;

        pillar = gameObject.transform.GetChild(0).GetChild(1).gameObject;

        contents = gameObject.transform.GetChild(1).GetGameObjectsInChildren();
    }
}

[System.Serializable]
public struct BattlePathCellAddress
{
    public BattlePathCellularStage stage;

    public int x;
    public int y;

    public BattlePathCellAddress(BattlePathCellularStage stage, int x, int y)
    {
        this.stage = stage;

        this.x = x;
        this.y = y;
    }
}
