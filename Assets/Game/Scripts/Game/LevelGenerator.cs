using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;

    public List<MapPatternData> wavePatterns;
    public float levelLength;

    private Vector2 perlinNoiseOrigin;

    private float[] offsetMap;

    private float perlinValue;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Generate((int)levelLength);
    }

    public void Generate(int blockCount)
    {
        perlinNoiseOrigin = new Vector2(Random.Range(-500f, 500f), Random.Range(-500f, 500f));

        offsetMap = new float[blockCount];

        for (int patternIndex = 0; patternIndex < wavePatterns.Count; patternIndex++)
        {
            for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                perlinValue = Mathf.PerlinNoise(perlinNoiseOrigin.x + (float)blockIndex / blockCount * wavePatterns[patternIndex].waveFrequency, perlinNoiseOrigin.y);

                offsetMap[blockIndex] += (perlinValue - 0.5f) * 2f * wavePatterns[patternIndex].waveHeight;
            }
        }

        for (int i = 0; i < offsetMap.Length; i++)
        {
            GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = new Vector3(i, offsetMap[i]);
        }
    }

    [System.Serializable]
    public struct MapPatternData
    {
        public float waveHeight;
        public float waveFrequency;
    }
}