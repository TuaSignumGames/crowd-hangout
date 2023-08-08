using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlePathCellType { None, Ground, Obstacle }

public class BattlePathCell
{
    private GameObject container;

    private BattlePathCellType type;

    private GameObject[] contents;

    private GameObject pillar;

    private BattleUnit registeredBattleUnit;

    private Vector2 size;

    private Vector3 position;

    private BattlePathCellAddress address;

    public Transform Transform => container.transform;

    public BattlePathCellType Type => type;

    public Vector2 Size => size;

    public Vector3 Position => container.transform.position;

    public BattlePathCellAddress Address => address;

    public BattleUnit BattleUnit => registeredBattleUnit;

    public BattlePathCell(GameObject gameObject, BattlePathCellAddress address, BattlePathCellType type)
    {
        container = gameObject;

        this.type = type;
        this.address = address;

        position = gameObject.transform.position;

        size = new Vector2(gameObject.transform.localScale.z, gameObject.transform.localScale.x);

        pillar = gameObject.transform.GetChild(0).GetChild(1).gameObject;

        contents = gameObject.transform.GetChild(1).GetGameObjectsInChildren();
    }

    public void RegisterBattleUnit(BattleUnit battleUnit)
    {
        registeredBattleUnit = battleUnit;
    }

    public void UnregisterBattleUnit()
    {
        registeredBattleUnit = null;
    }

    public void SetActive(bool isActive)
    {

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
