using System;
using UnityEngine;

public static class Serializables
{
    public static bool IsDataSerializable(object data)
    {
        return data.GetType().IsPrimitive() || data.GetType() == typeof(Vector3) || data.GetType() == typeof(AnimationCurve);
    }
}

[Serializable]
public class SerializableAnimationCurve : ISerializable<AnimationCurve>
{
    public SerializableKeyframe[] serializableKeyframes;

    public int postWrapModeIndex;
    public int preWrapModeIndex;

    public SerializableAnimationCurve(AnimationCurve animationCurve)
    {
        serializableKeyframes = new SerializableKeyframe[animationCurve.keys.Length];

        postWrapModeIndex = (int)animationCurve.postWrapMode;
        preWrapModeIndex = (int)animationCurve.preWrapMode;

        for (int i = 0; i < serializableKeyframes.Length; i++)
        {
            serializableKeyframes[i] = new SerializableKeyframe(animationCurve.keys[i]);
        }
    }

    public AnimationCurve Deserialize()
    {
        AnimationCurve animationCurve = new AnimationCurve();

        animationCurve.postWrapMode = (WrapMode)postWrapModeIndex;
        animationCurve.preWrapMode = (WrapMode)preWrapModeIndex;

        for (int i = 0; i < serializableKeyframes.Length; i++)
        {
            animationCurve.AddKey(serializableKeyframes[i].Deserialize());
        }

        return animationCurve;
    }

    [Serializable]
    public class SerializableKeyframe : ISerializable<Keyframe>
    {
        public float value;
        public float time;

        public float inTangent;
        public float inWeight;
        public float outTangent;
        public float outWeight;

        public int tangentMode;
        public int weightedModeIndex;

        public SerializableKeyframe(Keyframe keyframe)
        {
            value = keyframe.value;
            time = keyframe.time;

            inTangent = keyframe.inTangent;
            inWeight = keyframe.inWeight;
            outTangent = keyframe.outTangent;
            outWeight = keyframe.outWeight;

            tangentMode = keyframe.tangentMode;
            weightedModeIndex = (int)keyframe.weightedMode;
        }

        public Keyframe Deserialize()
        {
            Keyframe keyframe = new Keyframe();

            keyframe.value = value;
            keyframe.time = time;

            keyframe.inTangent = inTangent;
            keyframe.inWeight = inWeight;
            keyframe.outTangent = outTangent;
            keyframe.outWeight = outWeight;

            keyframe.tangentMode = tangentMode;
            keyframe.weightedMode = (WeightedMode)weightedModeIndex;

            return keyframe;
        }
    }
}

[Serializable]
public class SerializableVector3 : ISerializable<Vector3>
{
    private float _x;
    private float _y;
    private float _z;

    public SerializableVector3(Vector3 vector3)
    {
        _x = vector3.x;
        _y = vector3.y;
        _z = vector3.z;
    }

    public Vector3 Deserialize()
    {
        return new Vector3(_x, _y, _z);
    }
}
