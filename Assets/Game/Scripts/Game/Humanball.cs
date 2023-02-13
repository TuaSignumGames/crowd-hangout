using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Humanball
{
    private List<HumanballLayer> layers;

    private List<HumanballCell> requestedCells;

    private int cellsCount;

    private int counter;

    public int CellsCount => cellsCount;

    public Humanball()
    {
        layers = new List<HumanballLayer>();
    }

    public Humanball(List<HumanballLayer> layers)
    {
        this.layers = new List<HumanballLayer>(layers);

        cellsCount = GetAvailableCellsCount();
    }

    public void AddHuman(HumanController human)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (i == 0)
            {
                if (layers[i].AddHumanInNextCell(human))
                {
                    return;
                }
            }
            else
            {
                if (layers[i].AddHumanInClosestCell(human))
                {
                    return;
                }
            }

        }
    }

    public void RemoveHuman(HumanController human)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].TryRemoveHuman(human))
            {
                return;
            }
        }
    }

    public List<HumanballCell> GetFilledCells()
    {
        requestedCells = new List<HumanballCell>();

        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].cells.Count; j++)
            {
                if (!layers[i].cells[j].IsAvailable)
                {
                    requestedCells.Add(layers[i].cells[j]);
                }
            }
        }

        return requestedCells;
    }

    public List<HumanballCell> GetEmptyCells()
    {
        requestedCells = new List<HumanballCell>();

        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].cells.Count; j++)
            {
                if (layers[i].cells[j].IsAvailable)
                {
                    requestedCells.Add(layers[i].cells[j]);
                }
            }
        }

        return requestedCells;
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
}