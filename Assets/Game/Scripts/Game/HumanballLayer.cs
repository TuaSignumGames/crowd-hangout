using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HumanballLayer
{
    private GameObject container;

    private HumanballCell closestCell;

    private float layerRadius;

    private float cellSqrDistance;
    private float minCellSqrDistance;

    private int availableCellsCount;

    public List<HumanballCell> cells;
    public List<HumanballCell> requiredCells;

    public int AvailableCellsCount => availableCellsCount;

    public float Radius => layerRadius;

    public bool IsEmpty => availableCellsCount == cells.Count;
    public bool IsAvailable => availableCellsCount > 0;

    public HumanballLayer(GameObject container, IList<HumanballCell> cells, float radius)
    {
        this.container = container;
        this.cells = new List<HumanballCell>(cells);

        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].SetLayer(this);

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
        else if (GetClosestEmptyCell(human.transform.position) != null)
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
            requiredCells = cells.FindAll((c) => c.IsAvailable);

            requiredCells.GetRandom().PutHuman(human);

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
            CheckForClosestCell(cells[i], position);
        }

        return closestCell;
    }

    public HumanballCell GetClosestEmptyCell(Vector3 position)
    {
        closestCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsAvailable)
            {
                CheckForClosestCell(cells[i], position);
            }
        }

        return closestCell;
    }

    public HumanballCell GetClosestFilledCell(Vector3 position)
    {
        closestCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            if (!cells[i].IsAvailable)
            {
                CheckForClosestCell(cells[i], position);
            }
        }

        return closestCell;
    }

    public HumanballCell GetPlanarClosestEmptyCell(Vector3 position, Axis planeAxis)
    {
        closestCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsAvailable)
            {
                CheckForPlanarClosestCell(cells[i], position, planeAxis);
            }
        }

        return closestCell;
    }

    public HumanballCell GetPlanarClosestFilledCell(Vector3 position, Axis planeAxis)
    {
        closestCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            if (!cells[i].IsAvailable)
            {
                CheckForPlanarClosestCell(cells[i], position, planeAxis);
            }
        }

        return closestCell;
    }

    private void CheckForClosestCell(HumanballCell cell, Vector3 point)
    {
        cellSqrDistance = (cell.transform.position - point).sqrMagnitude;

        if (cellSqrDistance < minCellSqrDistance)
        {
            minCellSqrDistance = cellSqrDistance;

            closestCell = cell;
        }
    }

    private void CheckForPlanarClosestCell(HumanballCell cell, Vector3 point, Axis planeAxis)
    {
        cellSqrDistance = (cell.transform.position - point).GetPlanarSqrMagnitude(planeAxis);

        if (cellSqrDistance < minCellSqrDistance)
        {
            minCellSqrDistance = cellSqrDistance;

            closestCell = cell;
        }
    }
}
