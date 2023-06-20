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

    private List<HumanController> registeredHumans;

    private float cellSqrDistance;
    private float minCellSqrDistance;

    private int cellsCount;

    private int previousLayerIndex;
    private int filledLayersCount;

    private int counter;

    private bool isTriggerColliderRequired;

    public int humansCount;

    public Action<int> OnLayerIncremented;

    public int CellsCount => cellsCount;
    public int LayersCount => layers.Count;

    public int AvailableCellsCount => GetAvailableCellsCount();
    public int AvailableLayersCount => GetAvailableLayersCount();

    public int FilledCellsCount => cellsCount - GetAvailableCellsCount();
    public int FilledLayersCount => filledLayersCount;

    public HumanballCell[] FilledCells => filledCells == null ?  GetFilledCells().ToArray() : filledCells.ToArray();
    public HumanballCell[] UsedCells => usedCells.ToArray();

    public HumanController[] RegisteredHumans => registeredHumans.ToArray();

    public Humanball()
    {
        layers = new List<HumanballLayer>();

        usedCells = new List<HumanballCell>();

        registeredHumans = new List<HumanController>();
    }

    public Humanball(List<HumanballLayer> layers)
    {
        this.layers = new List<HumanballLayer>(layers);

        usedCells = new List<HumanballCell>();

        registeredHumans = new List<HumanController>();

        cellsCount = GetAvailableCellsCount();

        usedCells.Add(layers[0].cells[0]);
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
                    registeredHumans.Add(human);

                    if (isTriggerColliderRequired)
                    {
                        human.components.collider.isTrigger = true;
                    }

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
                    registeredHumans.Add(human);

                    if (isTriggerColliderRequired)
                    {
                        human.components.collider.isTrigger = true;
                    }

                    humansCount++;

                    return availableCell;
                }
            }
        }

        return null;
    }

    public void RegisterHuman(HumanController human)
    {
        registeredHumans.Add(human);
    }

    public void RemoveHuman(HumanController human)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].TryRemoveHuman(human))
            {
                registeredHumans.Remove(human);

                humansCount--;

                if (humansCount <= 0)
                {
                    PlayerController.Instance.Fail();
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

    public List<HumanController> GetActuallyPresentHumans()
    {
        List<HumanController> humans = new List<HumanController>();

        for (int i = 0; i < usedCells.Count; i++)
        {
            if (usedCells[i].transform.childCount > 0)
            {
                humans.Add(usedCells[i].transform.GetComponentInChildren<HumanController>());
            }
        }

        return humans;
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

    public void SetHumanColliderAsTriggers(bool isTrigger)
    {
        isTriggerColliderRequired = isTrigger;

        for (int i = 0; i < registeredHumans.Count; i++)
        {
            registeredHumans[i].components.collider.isTrigger = isTrigger;
        }
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