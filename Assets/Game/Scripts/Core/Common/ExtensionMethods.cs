using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MoreMountains.NiceVibrations;

public static class ExtensionMethods
{
    public static void ApplyBasicAttributes(this Button button, bool playHaptic = true, float pulseAmplitude = 0.05f, float pulseDuration = 0.3f, System.Action onPointerDownCallback = null)
    {
        if (button.GetComponent<EventTrigger>())
        {
            GameObject.Destroy(button.GetComponent<EventTrigger>());
        }

        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        var pointerUp = new EventTrigger.Entry();
        var pointerDown = new EventTrigger.Entry();

        float baseScale = button.transform.localScale.x;

        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((e) => button.transform.DOScale(baseScale, pulseDuration / 2f));

        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((e) => { button.transform.DOScale(baseScale * (1f - pulseAmplitude), pulseDuration / 2f); if (playHaptic) MMVibrationManager.Haptic(HapticTypes.LightImpact); if (onPointerDownCallback != null) onPointerDownCallback(); });

        trigger.triggers.Add(pointerUp);
        trigger.triggers.Add(pointerDown);
    }

    public static void ApplySelectionAttributes(this SelectionButton selectionButton, System.Action pushCallback, System.Action releaseCallback, bool playHaptic = true, float pressDownscale = 0.0f, float pulseDuration = 0.3f)
    {
        if (selectionButton.GetComponent<EventTrigger>())
        {
            GameObject.Destroy(selectionButton.GetComponent<EventTrigger>());
        }

        EventTrigger trigger = selectionButton.gameObject.AddComponent<EventTrigger>();

        var pointerUp = new EventTrigger.Entry();
        var pointerDown = new EventTrigger.Entry();

        float baseScale = selectionButton.transform.localScale.x;

        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((e) => releaseCallback());

        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((e) => { selectionButton.transform.DOScale(baseScale * (1f - pressDownscale), pulseDuration / 2f); if (playHaptic && selectionButton.isSelectable) MMVibrationManager.Haptic(HapticTypes.LightImpact); pushCallback(); });

        trigger.triggers.Add(pointerUp);
        trigger.triggers.Add(pointerDown);
    }

    public static float GetPlanarMagnitude(this Vector3 vector, Axis planeAxis)
    {
        if (planeAxis == Axis.X)
        {
            return new Vector2(vector.y, vector.z).magnitude;
        }
        else if (planeAxis == Axis.Y)
        {
            return new Vector2(vector.x, vector.z).magnitude;
        }
        else
        {
            return new Vector2(vector.x, vector.y).magnitude;
        }
    }

    public static float GetPlanarSqrMagnitude(this Vector3 vector, Axis planeAxis)
    {
        if (planeAxis == Axis.X)
        {
            return new Vector2(vector.y, vector.z).sqrMagnitude;
        }
        else if (planeAxis == Axis.Y)
        {
            return new Vector2(vector.x, vector.z).sqrMagnitude;
        }
        else
        {
            return new Vector2(vector.x, vector.y).sqrMagnitude;
        }
    }

    public static float GetPlanarAngleTo(this Vector3 from, Vector3 to, Axis axis)
    {
        if (axis == Axis.X)
        {
            return Vector2.SignedAngle(new Vector2(from.y, from.z), new Vector2(to.y, to.z));
        }
        else if (axis == Axis.Y)
        {
            return Vector2.SignedAngle(new Vector2(from.x, from.z), new Vector2(to.x, to.z));
        }
        else
        {
            return Vector2.SignedAngle(new Vector2(from.x, from.y), new Vector2(to.x, to.y));
        }
    }

    public static Vector2 Multiplied(this Vector2 vector, Vector2 multiplicationVector)
    {
        return new Vector3(vector.x * multiplicationVector.x, vector.y * multiplicationVector.y);
    }

    public static Vector2 Divided(this Vector2 vector, Vector2 divisionVector)
    {
        return new Vector3(vector.x / divisionVector.x, vector.y / divisionVector.y);
    }

    public static Vector2 ToVector2(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }

    public static Vector3 Multiplied(this Vector3 vector, Vector3 multiplicationVector)
    {
        return new Vector3(vector.x * multiplicationVector.x, vector.y * multiplicationVector.y, vector.z * multiplicationVector.z);
    }

    public static Vector3 Divided(this Vector3 vector, Vector3 divisionVector)
    {
        return new Vector3(vector.x / divisionVector.x, vector.y / divisionVector.y, vector.z / divisionVector.z);
    }

    public static Vector3 GetPlanarDirection(this Vector3 sourceVector, Axis axis)
    {
        if (axis == Axis.X)
        {
            return new Vector3(0, sourceVector.y, sourceVector.z).normalized;
        }
        else if (axis == Axis.Y)
        {
            return new Vector3(sourceVector.x, 0, sourceVector.z).normalized;
        }
        else
        {
            return new Vector3(sourceVector.x, sourceVector.y, 0).normalized;
        }
    }

    public static Vector3 GetDirectionTo(this Vector3 sourcePosition, Vector3 point)
    {
        return (point - sourcePosition).normalized;
    }

    public static Vector3 GetMidpoint(this IList<Vector3> positions)
    {
        Vector3 resultVector = new Vector3();

        for (int i = 0; i < positions.Count; i++)
        {
            resultVector += positions[i];
        }

        return resultVector / positions.Count;
    }

    public static Vector3 GetMidpoint(this IList<Transform> transforms)
    {
        Vector3 resultVector = new Vector3();

        for (int i = 0; i < transforms.Count; i++)
        {
            resultVector += transforms[i].position;
        }

        return resultVector / transforms.Count;
    }

    public static float ToSignedAngle(this float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    public static void AddEvent(this EventTrigger eventTrigger, EventTriggerType eventTriggerType, Action newEvent)
    {
        EventTrigger.Entry requestedTypeEntry = eventTrigger.triggers.Find((t) => t.eventID == eventTriggerType);

        if (requestedTypeEntry != null)
        {
            int requestedEntryEventsCount = requestedTypeEntry.callback.GetPersistentEventCount();

            for (int i = 0; i < requestedEntryEventsCount; i++)
            {
                if (requestedTypeEntry.callback.GetPersistentMethodName(i) == newEvent.Method.Name)
                {
                    return;
                }
            }

            requestedTypeEntry.callback.AddListener((e) => newEvent());
        }
        else
        {
            EventTrigger.Entry newEntry = new EventTrigger.Entry();

            newEntry.eventID = eventTriggerType;
            newEntry.callback.AddListener((e) => newEvent());

            eventTrigger.triggers.Add(newEntry);
        }
    }

    public static void OverrideEvent(this EventTrigger eventTrigger, EventTriggerType eventTriggerType, Action newEvent)
    {
        EventTrigger.Entry requestedTypeEntry = eventTrigger.triggers.Find((t) => t.eventID == eventTriggerType);

        if (requestedTypeEntry != null)
        {
            requestedTypeEntry.callback.RemoveAllListeners();

            requestedTypeEntry.callback.AddListener((e) => newEvent());
        }
    }

    public static void RemoveAllListeners(this EventTrigger eventTrigger, EventTriggerType eventTriggerType)
    {
        EventTrigger.Entry requestedTypeEntry = eventTrigger.triggers.Find((t) => t.eventID == eventTriggerType);

        if (requestedTypeEntry != null)
        {
            requestedTypeEntry.callback.RemoveAllListeners();
        }
    }

    public static void RemoveAllListeners(this EventTrigger eventTrigger)
    {
        for (int i = 0; i < eventTrigger.triggers.Count; i++)
        {
            eventTrigger.triggers[i].callback.RemoveAllListeners();
        }
    }

    public static void AddHaptic(this EventTrigger eventTrigger, HapticTypes hapticType)
    {
        eventTrigger.AddEvent(EventTriggerType.PointerDown, () => AppManager.Instance.PlayHaptic(hapticType));
    }

    public static bool IsHostAvailable(this TcpClient tcpClient)
    {
        try
        {
            return !(tcpClient.Client.Poll(1, SelectMode.SelectRead) && tcpClient.Client.Available == 0);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsNumeric(this object obj)
    {
        return obj.GetType() == typeof(byte) || obj.GetType() == typeof(sbyte) || obj.GetType() == typeof(short) || obj.GetType() == typeof(ushort) || obj.GetType() == typeof(int) ||
            obj.GetType() == typeof(uint) || obj.GetType() == typeof(long) || obj.GetType() == typeof(ulong) || obj.GetType() == typeof(float) || obj.GetType() == typeof(double) ||
            obj.GetType() == typeof(decimal);
    }

    public static bool IsPrimitive(this Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) ||
            type == typeof(ulong) || type == typeof(float) || type == typeof(double) || type == typeof(decimal) || type == typeof(bool) || type == typeof(char) ||
            type == typeof(string) || type == typeof(object);
    }

    public static bool IsEqualToFloat(this float callerValue, float value, int comparisonDecimalPlaces = 100)
    {
        return callerValue * comparisonDecimalPlaces == value * comparisonDecimalPlaces;
    }

    public static bool IsPrefab(this GameObject gameObject)
    {
        return gameObject.scene.rootCount == 0;
    }

    public static List<Transform> GetChildren(this Transform transform)
    {
        List<Transform> children = new List<Transform>();

        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }

        return children;
    }

    public static GameObject[] GetGameObjectsInChildren(this Transform transform)
    {
        List<GameObject> gameObjects = new List<GameObject>();

        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            gameObjects.Add(transform.GetChild(i).gameObject);
        }

        return gameObjects.ToArray();
    }

    public static Transform FindChildWithName(this Transform transform, string childName)
    {
        foreach (Transform child in transform)
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    public static void RemoveChildren(this Transform transform)
    {
        foreach (Transform child in transform.GetChildren())
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public static void RemoveChildrenImmediate(this Transform transform)
    {
        foreach (Transform child in transform.GetChildren())
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
    }

    public static float GetCoordinate(this Transform transform, TransformComponent component, Axis axis, Space space)
    {
        switch (component)
        {
            case TransformComponent.Position:

                switch (space)
                {
                    case Space.World:

                        switch (axis)
                        {
                            case Axis.X: return transform.position.x;
                            case Axis.Y: return transform.position.y;
                            case Axis.Z: return transform.position.z;
                        }

                        break;

                    case Space.Self:

                        switch (axis)
                        {
                            case Axis.X: return transform.localPosition.x;
                            case Axis.Y: return transform.localPosition.y;
                            case Axis.Z: return transform.localPosition.z;
                        }

                        break;
                }

                break;

            case TransformComponent.Rotation:

                switch (space)
                {
                    case Space.World:

                        switch (axis)
                        {
                            case Axis.X: return transform.eulerAngles.x;
                            case Axis.Y: return transform.eulerAngles.y;
                            case Axis.Z: return transform.eulerAngles.z;
                        }

                        break;

                    case Space.Self:

                        switch (axis)
                        {
                            case Axis.X: return transform.localEulerAngles.x;
                            case Axis.Y: return transform.localEulerAngles.y;
                            case Axis.Z: return transform.localEulerAngles.z;
                        }

                        break;
                }

                break;

            case TransformComponent.Scale:

                switch (space)
                {
                    case Space.World:

                        switch (axis)
                        {
                            case Axis.X: return transform.lossyScale.x;
                            case Axis.Y: return transform.lossyScale.y;
                            case Axis.Z: return transform.lossyScale.z;
                        }

                        break;

                    case Space.Self:

                        switch (axis)
                        {
                            case Axis.X: return transform.localScale.x;
                            case Axis.Y: return transform.localScale.y;
                            case Axis.Z: return transform.localScale.z;
                        }

                        break;
                }

                break;
        }

        return 0;
    }

    public static void SetCoordinate(this Transform transform, TransformComponent component, Axis axis, Space space, float value)
    {
        switch (component)
        {
            case TransformComponent.Position:

                switch (space)
                {
                    case Space.World:

                        switch (axis)
                        {
                            case Axis.X: transform.position = new Vector3(value, transform.position.y, transform.position.z); break;
                            case Axis.Y: transform.position = new Vector3(transform.position.x, value, transform.position.z); break;
                            case Axis.Z: transform.position = new Vector3(transform.position.x, transform.position.y, value); break;
                        }

                        break;

                    case Space.Self:

                        switch (axis)
                        {
                            case Axis.X: transform.localPosition = new Vector3(value, transform.localPosition.y, transform.localPosition.z); break;
                            case Axis.Y: transform.localPosition = new Vector3(transform.localPosition.x, value, transform.localPosition.z); break;
                            case Axis.Z: transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, value); break;
                        }

                        break;
                }

                break;

            case TransformComponent.Rotation:

                switch (space)
                {
                    case Space.World:

                        switch (axis)
                        {
                            case Axis.X: transform.eulerAngles = new Vector3(value, transform.eulerAngles.y, transform.eulerAngles.z); break;
                            case Axis.Y: transform.eulerAngles = new Vector3(transform.eulerAngles.x, value, transform.eulerAngles.z); break;
                            case Axis.Z: transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, value); break;
                        }

                        break;

                    case Space.Self:

                        switch (axis)
                        {
                            case Axis.X: transform.localEulerAngles = new Vector3(value, transform.localEulerAngles.y, transform.localEulerAngles.z); break;
                            case Axis.Y: transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, value, transform.localEulerAngles.z); break;
                            case Axis.Z: transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, value); break;
                        }

                        break;
                }

                break;

            case TransformComponent.Scale:

                switch (space)
                {
                    case Space.World:

                        Debug.LogError("Global scale assignation attempt: lossyScale is read only");

                        break;

                    case Space.Self:

                        switch (axis)
                        {
                            case Axis.X: transform.localScale = new Vector3(value, transform.localScale.y, transform.localScale.z); break;
                            case Axis.Y: transform.localScale = new Vector3(transform.localScale.x, value, transform.localScale.z); break;
                            case Axis.Z: transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, value); break;
                        }

                        break;
                }

                break;
        }
    }

    public static List<T> Invert<T>(this List<T> list)
    {
        List<T> newList = new List<T>(list.Capacity);

        for (int i = list.Count - 1; i < list.Count; i++)
        {
            newList.Add(list[i]);
        }

        return newList;
    }

    public static T TryGet<T>(this IList<T> list, int index) where T : class
    {
        if (index >= 0 && index < list.Count)
        {
            return list[index];
        }
        else
        {
            return null;
        }
    }

    public static T GetFirst<T>(this IList<T> list)
    {
        return list[0];
    }

    public static T GetLast<T>(this IList<T> list)
    {
        return list[list.Count - 1];
    }

    public static T GetRandom<T>(this IList<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static T CutAt<T>(this List<T> list, int index)
    {
        if (index < list.Count)
        {
            T cuttingItem = list[index];

            list.RemoveAt(index);

            return cuttingItem;
        }

        return default;
    }

    public static T CutRandom<T>(this List<T> list)
    {
        return list.CutAt(UnityEngine.Random.Range(0, list.Count));
    }

    public static string Colorize(this string text, Color color)
    {
        return $"<color=#{(byte)(color.r * 255f):X2}{(byte)(color.g * 255f):X2}{(byte)(color.b * 255f):X2}>{text}</color>";
    }

    public static string SelectFrom(this string text, char character)
    {
        if (text.Contains(character.ToString()))
        {
            return text.Substring(text.IndexOf(character) + 1);
        }

        return string.Empty;
    }

    public static string SelectTo(this string text, char character)
    {
        if (text.Contains(character.ToString()))
        {
            return text.Substring(0, text.IndexOf(character));
        }

        return string.Empty;
    }

    public static string Select(this string text, char from, char to)
    {
        if (text.Contains(from.ToString()) && text.Contains(to.ToString()))
        {
            int entryCharacterIndex = text.IndexOf(from) + 1;

            return text.Substring(entryCharacterIndex, text.IndexOf(to) - entryCharacterIndex);
        }

        return string.Empty;
    }

    public static ComparisonType CompareTo(this object a, object b)
    {
        if (a.IsNumeric() && b.IsNumeric())
        {
            double doubleA = Convert.ToDouble(a);
            double doubleB = Convert.ToDouble(b);

            if (doubleA == doubleB)
            {
                if (doubleA > doubleB)
                {
                    return ComparisonType.GreaterThanOrEqualTo;
                }
                if (doubleA < doubleB)
                {
                    return ComparisonType.LessThanOrEqualTo;
                }

                return ComparisonType.EqualTo;
            }
            else
            {
                if (doubleA > doubleB)
                {
                    return ComparisonType.GreaterThan;
                }
                if (doubleA < doubleB)
                {
                    return ComparisonType.LessThan;
                }
            }
        }

        return ComparisonType.NotEqualTo;
    }

    public static Color Transparent(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    public static string[] GetNames(this IList<GameObject> gameObjects)
    {
        string[] names = new string[gameObjects.Count];

        for (int i = 0; i < names.Length; i++)
        {
            names[i] = gameObjects[i].name;
        }

        return names;
    }

    public static string[] GetNames(this IList<Transform> transforms)
    {
        string[] names = new string[transforms.Count];

        for (int i = 0; i < names.Length; i++)
        {
            names[i] = transforms[i].gameObject.name;
        }

        return names;
    }

    public static Transform SetData(this Transform transform, TransformData transformData)
    {
        if (transformData.space == Space.World)
        {
            transform.position = transformData.position;
            transform.eulerAngles = transformData.rotation;
        }
        else
        {
            transform.localPosition = transformData.position;
            transform.localEulerAngles = transformData.rotation;
        }

        transform.localScale = transformData.scale;

        return transform;
    }

    public static Transform GetClosest(this IList<Transform> array, Transform transform)
    {
        Transform closestTransform = null;

        float sqrDistance = 0;
        float minSqrDistance = float.MaxValue;

        for (int i = 0; i < array.Count; i++)
        {
            sqrDistance = (array[i].position - transform.position).sqrMagnitude;

            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;

                closestTransform = array[i];
            }
        }

        return closestTransform;
    }

    public static Transform GetClosest(this IList<Transform> array, Vector3 position)
    {
        Transform closestTransform = null;

        float sqrDistance = 0;
        float minSqrDistance = float.MaxValue;

        for (int i = 0; i < array.Count; i++)
        {
            sqrDistance = (array[i].position - position).sqrMagnitude;

            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;

                closestTransform = array[i];
            }
        }

        return closestTransform;
    }

    public static Transform GetClosestOnPlane(this IList<Transform> array, Transform transform, Axis planeAxis)
    {
        Transform closestTransform = null;

        float sqrDistance = 0;
        float minSqrDistance = float.MaxValue;

        for (int i = 0; i < array.Count; i++)
        {
            sqrDistance = (array[i].position - transform.position).GetPlanarSqrMagnitude(planeAxis);

            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;

                closestTransform = array[i];
            }
        }

        return closestTransform;
    }
}