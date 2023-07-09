using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePathCellularStage
{
    private BattlePathCell[] cells;

    public BattlePathCellularStage(List<BattlePathCell> cells)
    {
        this.cells = cells.ToArray();
    }
}
