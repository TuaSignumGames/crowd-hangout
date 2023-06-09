using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscapeSegment
{
    public List<BlockPair> blockPairs;

    public float[] ceilingDepths;
    public float[] groundDepths;

    public float ceilingBaseHeight;
    public float groundBaseHeight;

    public LandscapeSegment(BlockPair[] blockPairs, float ceilingBaseHeight, float groundBaseHeight)
    {
        this.blockPairs = new List<BlockPair>(blockPairs);

        this.ceilingBaseHeight = ceilingBaseHeight;
        this.groundBaseHeight = groundBaseHeight;

        //CalculateDepths();
    }

    public void CalculateDepths()
    {
        ceilingDepths = new float[blockPairs.Count];
        groundDepths = new float[blockPairs.Count];

        for (int i = 0; i < blockPairs.Count; i++)
        {
            ceilingDepths[i] = blockPairs[i].ceilingBlock.transform.position.y - ceilingBaseHeight;
            groundDepths[i] = groundBaseHeight - blockPairs[i].groundBlock.transform.position.y;
        }
    }
}
