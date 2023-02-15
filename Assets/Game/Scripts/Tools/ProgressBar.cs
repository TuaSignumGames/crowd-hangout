using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProgressBar
{
    public GameObject barContainer;
    public Transform fillingTransform;
    [Space]
    public bool hideOnMinValue;
    public bool hideOnMaxValue;

    private float fillingLength;

    public void Initialize()
    {
        fillingLength = fillingTransform.localScale.x;
    }

    public void Update(float value)
    {
        if (hideOnMinValue)
        {
            if (value <= 0)
            {
                barContainer.SetActive(false);
            }
        }

        if (hideOnMaxValue)
        {
            if (value >= 1f)
            {
                barContainer.SetActive(false);
            }
        }

        if (barContainer.activeSelf)
        {
            barContainer.transform.forward = CameraController.Instance.camera.transform.position - barContainer.transform.position;

            fillingTransform.localScale = new Vector3(fillingLength * value, fillingTransform.localScale.y, fillingTransform.localScale.z);
        }
    }
}