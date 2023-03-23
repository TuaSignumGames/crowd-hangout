using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Humanball
{
    private List<HumanballLayer> layers;

    private List<HumanballCell> filledCells;
    private List<HumanballCell> emptyCells;
    private List<HumanballCell> usedCells;

    private Vector3 midpoint;

    private HumanballCell[] layerClosestCells;

    private HumanballCell closestCell;
    private HumanballCell availableCell;

    private float cellSqrDistance;
    private float minCellSqrDistance;

    private int cellsCount;
    private int humansCount;

    private int previousLayerIndex;
    private int filledLayersCount;

    private int counter;

    public Action<int> OnLayerIncremented;

    public int CellsCount => cellsCount;
    public int LayersCount => layers.Count;

    public int AvailableCellsCount => GetAvailableCellsCount();
    public int AvailableLayersCount => GetAvailableLayersCount();

    public int FilledCellsCount => cellsCount - GetAvailableCellsCount();
    public int FilledLayersCount => filledLayersCount;

    public int HumansCount => humansCount;

    public HumanballCell[] FilledCells => filledCells == null ?  GetFilledCells().ToArray() : filledCells.ToArray();

    public Humanball()
    {
        layers = new List<HumanballLayer>();

        usedCells = new List<HumanballCell>();
    }

    public Humanball(List<HumanballLayer> layers)
    {
        this.layers = new List<HumanballLayer>(layers);

        usedCells = new List<HumanballCell>();

        cellsCount = GetAvailableCellsCount();
    }

    public void LateUpdate()
    {
        for (int i = 0; i < usedCells.Count; i++)
        {
            usedCells[i].Update();
        }
    }

    public HumanballCell ReserveCell(HumanController human, bool closestCell = true)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (i == 0)
            {
                availableCell = layers[i].ReserveNextCell(human);

                if (availableCell != null)
                {
                    filledLayersCount = 1;

                    return availableCell;
                }
            }
            else
            {
                availableCell = (closestCell ? layers[i].ReserveClosestCell(human) : layers[i].ReserveRandomCell(human));

                if (availableCell != null)
                {
                    if (i > previousLayerIndex)
                    {
                        previousLayerIndex = i;

                        if (OnLayerIncremented != null)
                        {
                            OnLayerIncremented(i - 1);
                        }
                    }

                    filledLayersCount = i + 1;

                    return availableCell;
                }
            }
        }

        return null;
    }

    public HumanballCell AddHuman(HumanController human, bool closestCell = true, bool playVFX = true)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (i == 0)
            {
                availableCell = layers[i].AddHumanInNextCell(human, playVFX);

                if (availableCell != null)
                {
                    filledLayersCount = 1;

                    usedCells.Add(availableCell);

                    humansCount++;

                    return availableCell;
                }
            }
            else
            {
                availableCell = closestCell ? layers[i].AddHumanInClosestCell(human, playVFX) : layers[i].AddHumanInRandomCell(human, playVFX);

                if (availableCell != null)
                {
                    if (i > previousLayerIndex)
                    {
                        previousLayerIndex = i;

                        if (OnLayerIncremented != null)
                        {
                            OnLayerIncremented(i - 1);
                        }
                    }

                    filledLayersCount = i + 1;

                    usedCells.Add(availableCell);

                    humansCount++;

                    return availableCell;
                }
            }
        }

        return null;
    }

    public void RemoveHuman(HumanController human)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].TryRemoveHuman(human))
            {
                humansCount--;

                if (humansCount <= 0)
                {
                    LevelManager.Instance.OnLevelFinished(false);
                }

                return;
            }
        }
    }

    public List<HumanballCell> GetFilledCells()
    {
        filledCells = new List<HumanballCell>();

        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].cells.Count; j++)
            {
                if (!layers[i].cells[j].IsAvailable)
                {
                    filledCells.Add(layers[i].cells[j]);
                }
            }
        }

        return filledCells;
    }

    public List<HumanballCell> GetEmptyCells()
    {
        emptyCells = new List<HumanballCell>();

        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].cells.Count; j++)
            {
                if (layers[i].cells[j].IsAvailable)
                {
                    emptyCells.Add(layers[i].cells[j]);
                }
            }
        }

        return emptyCells;
    }

    public HumanballCell GetPlanarClosestEmptyCell(Vector3 position, Axis planeAxis)
    {
        layerClosestCells = new HumanballCell[layers.Count];

        for (int i = 0; i < layers.Count; i++)
        {
            if (!layers[i].IsEmpty)
            {
                layerClosestCells[i] = layers[i].GetPlanarClosestEmptyCell(position, planeAxis);
            }
        }

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < layerClosestCells.Length; i++)
        {
            if (layerClosestCells[i] != null)
            {
                CheckForPlanarClosestCell(layerClosestCells[i], position, planeAxis);
            }
        }

        return closestCell;
    }

    public HumanballCell GetPlanarClosestFilledCell(Vector3 position, Axis planeAxis)
    {
        layerClosestCells = new HumanballCell[layers.Count];

        for (int i = 0; i < layers.Count; i++)
        {
            if (!layers[i].IsEmpty)
            {
                layerClosestCells[i] = layers[i].GetPlanarClosestFilledCell(position, planeAxis);
            }
        }

        minCellSqrDistance = float.MaxValue;

        for (int i = 0; i < layerClosestCells.Length; i++)
        {
            if (layerClosestCells[i] != null)
            {
                CheckForPlanarClosestCell(layerClosestCells[i], position, planeAxis);
            }
        }

        return closestCell;
    }

    public Vector3 GetActiveCellsMidpoint()
    {
        midpoint = new Vector3();

        GetFilledCells();

        for (int i = 0; i < filledCells.Count; i++)
        {
            midpoint += filledCells[i].transform.position;
        }

        return new Vector3(midpoint.x / filledCells.Count, midpoint.y / filledCells.Count, 0);
    }

    private void CheckForPlanarClosestCell(HumanballCell cell, Vector3 point, Axis planeAxis)
    {
        cellSqrDistance = (point - cell.transform.position).GetPlanarSqrMagnitude(planeAxis);

        if (cellSqrDistance < minCellSqrDistance)
        {
            minCellSqrDistance = cellSqrDistance;

            closestCell = cell;
        }
    }

    private int GetAvailableCellsCount()
    {
        counter = 0;

        for (int i = 0; i < layers.Count; i++)
        {
            counter += layers[i].AvailableCellsCount;
        }

        return counter;
    }

    private int GetAvailableLayersCount()
    {
        counter = 0;

        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].IsAvailable)
            {
                counter++;
            }
        }

        return counter;
    }
}