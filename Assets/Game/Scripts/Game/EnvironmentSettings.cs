using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnvironmentSettings
{
    public List<ThemeInfo> themes;
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
