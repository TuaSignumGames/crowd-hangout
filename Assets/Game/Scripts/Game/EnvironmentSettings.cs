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

    public float GetForceMagnitude(string tag)
    {
        return forceAreas.Find((area) => area.tag == tag).forceMagnitude;
    }
}

[System.Serializable]
public class EnvironmentSegmentsInfo
{
    public LandscapeSegmentData lakeSegmentData;
    public LandscapeSegmentData lavaSegmentData;
}
