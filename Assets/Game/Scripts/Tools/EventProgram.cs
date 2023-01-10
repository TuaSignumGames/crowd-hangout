using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventProgram : MonoBehaviour
{
    public UnityEvent onStart;
    [Space]
    public List<EventProgramStepData> steps;
    public bool looping;

    private IEnumerator programCoroutine;

    private bool isProcessing;

    private void Start()
    {
        onStart?.Invoke();

        Launch();
    }

    public void Launch()
    {
        programCoroutine = ProgramCoroutine();

        isProcessing = true;

        StartCoroutine(ProgramCoroutine());
    }

    public void Stop()
    {
        isProcessing = false;

        StopCoroutine(programCoroutine);
    }

    private IEnumerator ProgramCoroutine()
    {
        while (isProcessing)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                yield return new WaitForSeconds(steps[i].delay);

                steps[i].action.Invoke();
            }

            isProcessing = looping;
        }
    }

    [System.Serializable]
    public struct EventProgramStepData
    {
        public float delay;
        public UnityEvent action;
    }
}
