using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePathCellularStage
{
    private BattlePathCell[,] cells;

    private List<BattlePathCell> requestedCells;

    private BattlePathCell requestedCell;

    public BattlePathCellularStage(BattlePathCell[,] cells)
    {
        this.cells = cells;
    }

    public BattlePathCell GetCell(int x, int y)
    {
        return cells[x, y];
    }

    public BattlePathCell[] GetCells(BattlePathCellType cellType)
    {
        requestedCells = new List<BattlePathCell>();

        foreach (BattlePathCell cell in cells)
        {
            if (cell.Type == cellType)
            {
                requestedCells.Add(cell);
            }
        }

        return requestedCells.ToArray();
    }
}
