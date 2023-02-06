using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanballLayer
{
    private GameObject container;

    private List<HumanballCell> cells;

    private HumanballCell closestCell;

    private float cellSqrDistance;
    private float minCellSqrDistance;

    private int availableCellsCount;

    public int AvailableCellsCount => availableCellsCount;

    public bool IsAvailable => availableCellsCount > 0;

    public HumanballLayer(GameObject container, IList<HumanballCell> cells)
    {
        this.container = container;
        this.cells = new List<HumanballCell>(cells);

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsAvailable)
            {
                availableCellsCount++;
            }
        }
    }

    public bool AddHumanInNext(HumanController human)
    {
        if (availableCellsCount == 0)
        {
            return false;
        }
        else
        {
            cells[cells.Count - availableCellsCount].PutHuman(human);

            availableCellsCount--;

            return true;
        }
    }

    public bool AddHumanInClosest(HumanController human)
    {
        if (availableCellsCount == 0)
        {
            return false;
        }
        else if (GetClosestCell(human.transform.position) != null)
        {
            closestCell.PutHuman(human);

            availableCellsCount--;

            return true;
        }

        return false;
    }

    public bool TryRemoveHuman(HumanController human)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (!cells[i].IsAvailable && cells[i].Human == human)
            {
                cells[i].EjectHuman();

                availableCellsCount++;

                return true;
            }
        }

        return false;
    }

    private HumanballCell GetClosestCell(Vector3 position)
    {
        closestCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsAvailable)
            {
                cellSqrDistance = (cells[i].transform.position - position).sqrMagnitude;

                if (cellSqrDistance < minCellSqrDistance)
                {
                    minCellSqrDistance = cellSqrDistance;

                    closestCell = cells[i];
                }
            }
        }

        return closestCell;
    }
}
