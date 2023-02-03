using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Humanball
{
    private List<HumanballLayer> layers;

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
            if (layers[i].AddHuman(human))
            {
                return;
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