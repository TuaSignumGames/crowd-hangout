using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HumanballLayer
{
    private GameObject container;

    private HumanballCell requiredCell;

    private float layerRadius;

    private float cellSqrDistance;
    private float minCellSqrDistance;

    private int availableCellsCount;

    private bool isBaked;

    public List<HumanballCell> cells;
    public List<HumanballCell> requiredCells;

    public int AvailableCellsCount => availableCellsCount;

    public float Radius => layerRadius;

    public bool IsBaked => isBaked;

    public bool IsEmpty => availableCellsCount == cells.Count;
    public bool IsAvailable => availableCellsCount > 0;

    public HumanballLayer(GameObject container, IList<HumanballCell> cells, float radius, bool isBaked)
    {
        this.container = container;

        this.cells = new List<HumanballCell>(cells);

        this.isBaked = isBaked;

        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].SetLayer(this);

            if (cells[i].IsAvailable)
            {
                availableCellsCount++;
            }
        }
    }

    public HumanballCell ReserveNextCell(HumanController human)
    {
        if (availableCellsCount == 0)
        {
            return null;
        }
        else
        {
            cells[cells.Count - availableCellsCount].Reserve(human);

            return cells[cells.Count - availableCellsCount--];
        }
    }

    public HumanballCell ReserveClosestCell(HumanController human)
    {
        if (availableCellsCount == 0)
        {
            return null;
        }
        else if (GetClosestEmptyCell(human.transform.position) != null)
        {
            requiredCell.Reserve(human);

            availableCellsCount--;

            return requiredCell;
        }

        return null;
    }

    public HumanballCell ReserveRandomCell(HumanController human)
    {
        if (availableCellsCount == 0)
        {
            return null;
        }
        else
        {
            requiredCells = cells.FindAll((c) => c.IsAvailable);

            requiredCell = requiredCells.GetRandom();

            requiredCell.Reserve(human);

            availableCellsCount--;

            return requiredCell;
        }
    }

    public HumanballCell AddHumanInNextCell(HumanController human, bool playVFX = true)
    {
        if (availableCellsCount == 0)
        {
            return null;
        }
        else
        {
            cells[cells.Count - availableCellsCount].PutHuman(human, playVFX);

            return cells[cells.Count - availableCellsCount--];
        }
    }

    public HumanballCell AddHumanInClosestCell(HumanController human, bool playVFX = true)
    {
        if (availableCellsCount == 0)
        {
            return null;
        }
        else if (GetClosestEmptyCell(human.transform.position) != null)
        {
            requiredCell.PutHuman(human, playVFX);

            availableCellsCount--;

            return requiredCell;
        }

        return null;
    }

    public HumanballCell AddHumanInRandomCell(HumanController human, bool playVFX = true)
    {
        if (availableCellsCount == 0)
        {
            return null;
        }
        else
        {
            requiredCells = cells.FindAll((c) => c.IsAvailable);

            requiredCell = requiredCells.GetRandom();

            requiredCell.PutHuman(human, playVFX);

            availableCellsCount--;

            return requiredCell;
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
        requiredCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            CheckForClosestCell(cells[i], position);
        }

        return requiredCell;
    }

    public HumanballCell GetClosestEmptyCell(Vector3 position)
    {
        requiredCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsAvailable)
            {
                CheckForClosestCell(cells[i], position);
            }
        }

        return requiredCell;
    }

    public HumanballCell GetClosestFilledCell(Vector3 position)
    {
        requiredCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            if (!cells[i].IsAvailable)
            {
                CheckForClosestCell(cells[i], position);
            }
        }

        return requiredCell;
    }

    public HumanballCell GetPlanarClosestEmptyCell(Vector3 position, Axis planeAxis)
    {
        requiredCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsAvailable)
            {
                CheckForPlanarClosestCell(cells[i], position, planeAxis);
            }
        }

        return requiredCell;
    }

    public HumanballCell GetPlanarClosestFilledCell(Vector3 position, Axis planeAxis)
    {
        requiredCell = null;

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            if (!cells[i].IsAvailable)
            {
                CheckForPlanarClosestCell(cells[i], position, planeAxis);
            }
        }

        return requiredCell;
    }

    private void CheckForClosestCell(HumanballCell cell, Vector3 point)
    {
        cellSqrDistance = (cell.transform.position - point).sqrMagnitude;

        if (cellSqrDistance < minCellSqrDistance)
        {
            minCellSqrDistance = cellSqrDistance;

            requiredCell = cell;
        }
    }

    private void CheckForPlanarClosestCell(HumanballCell cell, Vector3 point, Axis planeAxis)
    {
        cellSqrDistance = (cell.transform.position - point).GetPlanarSqrMagnitude(planeAxis);

        if (cellSqrDistance < minCellSqrDistance)
        {
            minCellSqrDistance = cellSqrDistance;

            requiredCell = cell;
        }
    }
}
