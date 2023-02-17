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

    public void Update()
    {
        if (barContainer.activeSelf)
        {
            barContainer.transform.forward = CameraController.Instance.camera.transform.position - barContainer.transform.position;
        }
    }

    public void SetValue(float value)
    {
        fillingTransform.localScale = new Vector3(Mathf.Clamp01(value), fillingTransform.localScale.y, fillingTransform.localScale.z);

        if (hideOnMinValue && hideOnMaxValue)
        {
            barContainer.SetActive(value > 0 && value < 1f);
        }
        else
        {
            if (hideOnMinValue)
            {
                barContainer.SetActive(value > 0);
            }
            else if (hideOnMaxValue)
            {
                barContainer.SetActive(value < 1f);
            }
        }
    }
}