using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePathCellularStage
{
    private BattlePathCell[,] cells;

    private List<BattlePathCell> requestedCells;

    private List<BattleUnit> battleUnits;
    private List<BattleUnit> requestedBattleUnits;

    private BattlePathCell requestedCell;

    private Vector2Int gridSize;

    private float cellSqrDistance;
    private float closestCellSqrDistance;

    private int minAvailableX;
    private int minAvailableY;
    private int maxAvailableX;
    private int maxAvailableY;

    public BattlePathCellularStage()
    {
        battleUnits = new List<BattleUnit>();
    }

    public void AddCells(BattlePathCell[,] cells)
    {
        this.cells = cells;

        gridSize = new Vector2Int(cells.GetLength(0), cells.GetLength(1));
    }

    public void AddBattleUnit(BattleUnit battleUnit)
    {
        battleUnits.Add(battleUnit);
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

    public void SetBattleUnitRanges(bool enabled)
    {
        for (int i = 0; i < battleUnits.Count; i++)
        {
            battleUnits[i].SetRangeVisible(enabled);
        }
    }

    public BattlePathCell TryGetCell(int x, int y)
    {
        if (x < 0 || x > cells.GetLength(0) - 1 || y < 0 || y > cells.GetLength(1) - 1)
        {
            return null;
        }

        return cells[x, y];
    }

    public BattlePathCell GetClosestCell(Vector3 position)
    {
        closestCellSqrDistance = float.MaxValue;

        foreach (BattlePathCell cell in cells)
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

    public BattlePathCell[] GetCells(BattlePathCellType cellType, float minX, float maxX, float minY, float maxY)
    {
        requestedCells = new List<BattlePathCell>();

        minAvailableX = Mathf.RoundToInt(gridSize.x * minX);
        minAvailableY = Mathf.RoundToInt(gridSize.y * minY);
        maxAvailableX = Mathf.RoundToInt(gridSize.x * maxX);
        maxAvailableY = Mathf.RoundToInt(gridSize.y * maxY);

        foreach (BattlePathCell cell in cells)
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
