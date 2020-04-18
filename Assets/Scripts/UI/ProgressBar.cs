using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxValue(float maxValue)
    {
        slider.maxValue = maxValue;
    }

    public float GetMaxValue()
    {
        return slider.maxValue;
    }

    public void SetValue(float value, float newMaxValue = 1f)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            SetMaxValue(newMaxValue);
        }
        value = value < slider.maxValue ? value : slider.maxValue;
        slider.value = value;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
