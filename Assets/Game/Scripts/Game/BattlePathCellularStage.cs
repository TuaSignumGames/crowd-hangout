using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePathCellularStage
{
    private BattlePathCell[,] cells;

    private List<BattlePathCell> requestedCells;

    private BattlePathCell requestedCell;

    private Vector2Int gridSize;

    private int minAvailableX;
    private int minAvailableY;
    private int maxAvailableX;
    private int maxAvailableY;

    public BattlePathCellularStage(BattlePathCell[,] cells)
    {
        this.cells = cells;

        gridSize = new Vector2Int(cells.GetLength(0), cells.GetLength(1));
    }

    public BattlePathCell GetCell(int x, int y)
    {
        return cells[x, y];
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
