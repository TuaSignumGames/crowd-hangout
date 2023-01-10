using UnityEngine;

[System.Serializable]
public struct Vector3Bool
{
    public bool x;
    public bool y;
    public bool z;

    public static Vector3Bool one => new Vector3Bool(true, true, true);
    public static Vector3Bool zero => new Vector3Bool(false, false, false);

    public Vector3Bool(bool x, bool y, bool z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override bool Equals(object obj)
    {
        return this == (Vector3Bool)obj;
    }

    public override int GetHashCode()
    {
        return (x, y, z).GetHashCode();
    }

    public static Vector3Bool Compare(Vector3 a, Vector3 b)
    {
        return new Vector3Bool(a.x == b.x, a.y == b.y, a.z == b.z);
    }

    public static bool operator == (Vector3Bool left, Vector3Bool right)
    {
        return left.x == right.x && left.y == right.y && left.z == right.z;
    }

    public static bool operator != (Vector3Bool left, Vector3Bool right)
    {
        return left.x != right.x || left.y != right.y || left.z != right.z;
    }

    public static Vector3Bool operator ! (Vector3Bool vector3Bool)
    {
        return new Vector3Bool(!vector3Bool.x, !vector3Bool.y, !vector3Bool.z);
    }
}
