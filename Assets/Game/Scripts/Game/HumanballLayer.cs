using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanballLayer
{
    private GameObject container;

    private HumanballCell closestCell;

    private float cellSqrDistance;
    private float minCellSqrDistance;

    private int availableCellsCount;

    public List<HumanballCell> cells;

    public int AvailableCellsCount => availableCellsCount;

    public bool IsEmpty => availableCellsCount == cells.Count;
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

    public bool AddHumanInNextCell(HumanController human)
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

    public bool AddHumanInClosestCell(HumanController human)
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

    public bool AddHumanInRandomCell(HumanController human)
    {
        if (availableCellsCount == 0)
        {
            return false;
        }
        else
        {
            cells.GetRandom().PutHuman(human);

            availableCellsCount--;

            return true;
        }
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

    public HumanballCell GetClosestCell(Vector3 position)
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

    public HumanballCell GetPlanarClosestCell(Vector3 position, Axis planeAxis)
    {
        closestCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsAvailable)
            {
                cellSqrDistance = (cells[i].transform.position - position).GetPlanarSqrMagnitude(planeAxis);

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
