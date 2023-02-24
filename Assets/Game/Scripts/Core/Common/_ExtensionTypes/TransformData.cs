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

    public static TransformData Lerp(TransformData a, TransformData b, float t)
    {
        if (a.space == b.space)
        {
            TransformData resultData = new TransformData();

            resultData.position = Vector3.Lerp(a.position, b.position, t);
            resultData.scale = Vector3.Lerp(a.scale, b.scale, t);

            resultData.rotation = new Vector3(Mathf.LerpAngle(a.rotation.x, b.rotation.x, t), Mathf.LerpAngle(a.rotation.y, b.rotation.y, t), Mathf.LerpAngle(a.rotation.z, b.rotation.z, t));

            resultData.space = a.space;

            return resultData;
        }
        else
        {
            throw new UnityException($"Transform spaces are not equal: a ({a.space}), b ({b.space})");
        }
    }
}
