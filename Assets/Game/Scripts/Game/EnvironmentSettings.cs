using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnvironmentSettings
{
    public List<ThemeInfo> themes;
    public EnvironmentParticlesInfo particles;
    [Space]
    public EnvironmentSegmentsInfo segments;
    public List<ForceAreaData> forceAreas;
    [Space]
    public Transform backgroundContainer;

    public string[] GetThemeTitles()
    {
        string[] titles = new string[themes.Count];

        for (int i = 0; i < themes.Count; i++)
        {
            titles[i] = themes[i].title;
        }

        return titles;
    }

    public ForceArea TryGetForceArea(Collider collider)
    {
        for (int i = 0; i < forceAreas.Count; i++)
        {
            if (forceAreas[i].tag == collider.tag)
            {
                return new ForceArea(forceAreas[i], collider);
            }
        }

        return null;
    }
}

[System.Serializable]
public class EnvironmentSegmentsInfo
{
    public LandscapeSegmentData lakeSegmentData;
    public LandscapeSegmentData lavaSegmentData;
    public LandscapeSegmentData confusionSegmentData;
}

[System.Serializable]
public class EnvironmentParticlesInfo
{
    public GameObject particlesContainer;
    [Space]
    public ParticleSystem cliffFracturesPrefab;
    public ParticleSystem foilFracturesPrefab;
    public ParticleSystem dangerFracturesPrefab;

    public Pool<ParticleSystem> cliffFracturePool;
    public Pool<ParticleSystem> foilFracturePool;
    public Pool<ParticleSystem> dangerFracturePool;

    private ParticleSystem[] cliffFractureInstances;
    private ParticleSystem[] foilFractureInstances;
    private ParticleSystem[] dangerFractureInstances;

    public void InitializePools(int poolSize)
    {
        cliffFractureInstances = new ParticleSystem[poolSize];
        foilFractureInstances = new ParticleSystem[poolSize];
        dangerFractureInstances = new ParticleSystem[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            cliffFractureInstances[i] = GameObject.Instantiate(cliffFracturesPrefab, particlesContainer.transform);
            foilFractureInstances[i] = GameObject.Instantiate(foilFracturesPrefab, particlesContainer.transform);
            dangerFractureInstances[i] = GameObject.Instantiate(dangerFracturesPrefab, particlesContainer.transform);
        }

        cliffFracturePool = new Pool<ParticleSystem>(cliffFractureInstances);
        foilFracturePool = new Pool<ParticleSystem>(foilFractureInstances);
        dangerFracturePool = new Pool<ParticleSystem>(dangerFractureInstances);
    }
}
