using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePathCellularStage
{
    private GameObject container;

    private BattlePathCell entryCell;
    private BattlePathCell bossCell;

    private BattlePathCell[,] gridCells;

    private List<BattlePathCell> requestedCells;

    private List<BattleUnit> battleUnits;
    private List<BattleUnit> requestedBattleUnits;

    private BattlePathCell requestedCell;

    private Vector3 midPoint;
    private Vector3 endPoint;

    private Vector2 size;

    private Vector2Int gridMatrixSize;

    private float cellSqrDistance;
    private float closestCellSqrDistance;

    private float unitCost;
    private float reward;

    private int minAvailableX;
    private int minAvailableY;
    private int maxAvailableX;
    private int maxAvailableY;

    private int orderIndex;

    public Vector2 Size => size;

    public Vector3 Position => container.transform.position;

    public Vector3 MidPoint => new Vector3(Position.x + size.y / 2f, Position.y, Position.z);
    public Vector3 EndPoint => bossCell.Position + new Vector3(bossCell.Size.x / 2f, 0, 0);

    public float Reward => reward;

    public int OrderIndex => orderIndex;

    public BattlePathCellularStage(GameObject container, float unitCost, int orderIndex)
    {
        this.container = container;
        this.unitCost = unitCost;
        this.orderIndex = orderIndex;

        battleUnits = new List<BattleUnit>();
    }

    public void AddCells(BattlePathCell entryCell, BattlePathCell bossCell, BattlePathCell[,] gridCells)
    {
        this.gridCells = gridCells;

        gridMatrixSize = new Vector2Int(gridCells.GetLength(0), gridCells.GetLength(1));

        size = new Vector2(entryCell.Size.x, entryCell.Size.y + bossCell.Size.y + BattlePathGenerator.Instance.GridCellSize.x * gridCells.GetLength(1));
    }

    public void AddBattleUnit(BattleUnit battleUnit)
    {
        battleUnits.Add(battleUnit);
    }

    public void UpdateBattleUnitObjectives()
    {
        for (int i = 0; i < battleUnits.Count; i++)
        {
            battleUnits[i].UpdateBehavior();
        }
    }

    public void SetBattleUnitRangesActive(bool enabled)
    {
        for (int i = 0; i < battleUnits.Count; i++)
        {
            battleUnits[i].SetRangeVisible(enabled);
        }
    }

    public void SetVisible(bool isVisible)
    {

    }

    public BattleUnit GetBattleUnit(HumanTeam team, int weaponLevel)
    {
        for (int i = 0; i < battleUnits.Count; i++)
        {
            if (battleUnits[i].Team == team && battleUnits[i].WeaponLevel == weaponLevel)
            {
                return battleUnits[i];
            }
        }

        return null;
    }

    public BattleUnit[] GetBattleUnits(HumanTeam team)
    {
        requestedBattleUnits = new List<BattleUnit>();

        for (int i = 0; i < battleUnits.Count; i++)
        {
            if (battleUnits[i].Team == team)
            {
                requestedBattleUnits.Add(battleUnits[i]);
            }
        }

        return requestedBattleUnits.ToArray();
    }

    public BattlePathCell TryGetCell(int x, int y)
    {
        if (x < 0 || x > gridCells.GetLength(0) - 1 || y < 0 || y > gridCells.GetLength(1) - 1)
        {
            return null;
        }

        return gridCells[x, y];
    }

    public BattlePathCell GetClosestCell(Vector3 position)
    {
        closestCellSqrDistance = float.MaxValue;

        foreach (BattlePathCell cell in gridCells)
        {
            if (cell != null)
            {
                cellSqrDistance = (cell.Position - position).GetPlanarSqrMagnitude(Axis.Y);

                if (cellSqrDistance < closestCellSqrDistance)
                {
                    requestedCell = cell;

                    closestCellSqrDistance = cellSqrDistance;
                }
            }
        }

        return requestedCell;
    }

    public BattlePathCell GetClosestAvailableCell(Vector3 position)
    {
        closestCellSqrDistance = float.MaxValue;

        foreach (BattlePathCell cell in gridCells)
        {
            if (cell != null && cell.Type == BattlePathCellType.Ground && !cell.BattleUnit)
            {
                cellSqrDistance = (cell.Position - position).GetPlanarSqrMagnitude(Axis.Y);

                if (cellSqrDistance < closestCellSqrDistance)
                {
                    requestedCell = cell;

                    closestCellSqrDistance = cellSqrDistance;
                }
            }
        }

        return requestedCell;
    }

    public BattlePathCell[] GetCells(BattlePathCellType cellType, float minX, float maxX, float minY, float maxY)
    {
        requestedCells = new List<BattlePathCell>();

        minAvailableX = Mathf.RoundToInt(gridMatrixSize.x * minX);
        minAvailableY = Mathf.RoundToInt(gridMatrixSize.y * minY);
        maxAvailableX = Mathf.RoundToInt(gridMatrixSize.x * maxX);
        maxAvailableY = Mathf.RoundToInt(gridMatrixSize.y * maxY);

        foreach (BattlePathCell cell in gridCells)
        {
            if (cell != null)
            {
                if (cell.Type == cellType)
                {
                    if (cell.Address.x >= minAvailableX && cell.Address.x <= maxAvailableX && cell.Address.y >= minAvailableY && cell.Address.y <= maxAvailableY)
                    {
                        requestedCells.Add(cell);
                    }
                }
            }
        }

        return requestedCells.ToArray();
    }
}
