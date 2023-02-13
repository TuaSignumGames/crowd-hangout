using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionController : MonoBehaviour
{
    public bool translation;
    public MotionData translationSettings;
    [Space]
    public bool rotation;
    public MotionData rotationSettings;

    private TransformEvaluator evaluator;

    private void Start()
    {
        evaluator = new TransformEvaluator(transform, MonoUpdateType.FixedUpdate);

        Launch();
    }

    public void Launch()
    {
        if (translation)
        {
            StartCoroutine(TranslatingCoroutine());
        }

        if (rotation)
        {
            StartCoroutine(RotatingCoroutine());
        }
    }

    private void FixedUpdate()
    {
        if (evaluator.Evaluating)
        {
            evaluator.Update();
        }
    }

    private IEnumerator TranslatingCoroutine()
    {
        while (gameObject.activeInHierarchy && translationSettings.looping)
        {
            evaluator.TranslateLocal(transform.localPosition + translationSettings.offset, translationSettings.duration, EvaluationType.Smooth);

            while (evaluator.Evaluating) { yield return null; }

            yield return new WaitForSeconds(translationSettings.delay);

            evaluator.TranslateLocal(transform.localPosition - translationSettings.offset, translationSettings.duration, EvaluationType.Smooth);

            while (evaluator.Evaluating) { yield return null; }

            yield return new WaitForSeconds(translationSettings.delay);
        }
    }

    private IEnumerator RotatingCoroutine()
    {
        while (gameObject.activeInHierarchy && rotationSettings.looping)
        {
            evaluator.RotateLocal(transform.localEulerAngles + rotationSettings.offset, rotationSettings.duration, EvaluationType.Smooth);

            while (evaluator.Evaluating) { yield return null; }

            yield return new WaitForSeconds(rotationSettings.delay);

            evaluator.RotateLocal(transform.localEulerAngles - rotationSettings.offset, rotationSettings.duration, EvaluationType.Smooth);

            while (evaluator.Evaluating) { yield return null; }

            yield return new WaitForSeconds(rotationSettings.delay);
        }
    }

    [System.Serializable]
    public struct MotionData
    {
        public Vector3 offset;
        public float duration;
        public bool looping;
        public float delay;
    }
}