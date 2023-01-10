using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GizmoRenderMode { Solid, Wire }

public static class GizmoDrawer
{
    public static void DrawSphereGizmo(Vector3 position, float radius, Color color, GizmoRenderMode renderMode)
    {
        Gizmos.color = color;

        if (renderMode == GizmoRenderMode.Solid)
        {
            Gizmos.DrawSphere(position, radius);
        }
        else
        {
            Gizmos.DrawWireSphere(position, radius);
        }
    }

    public static void DrawLine(Vector3 from, Vector3 to, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(from, to);
    }
}
