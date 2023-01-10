using UnityEngine;

[System.Serializable]
public struct TransformData
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public Space space;

    public TransformData(Transform transform, Space space)
    {
        this.space = space;

        if (space == Space.World)
        {
            position = transform.position;
            rotation = transform.eulerAngles;
        }
        else
        {
            position = transform.localPosition;
            rotation = transform.localEulerAngles;
        }

        scale = transform.localScale;
    }
}
