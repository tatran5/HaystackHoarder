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
    public void SetProgress(float progressValue)
    {
        slider.value = progressValue;
    }
}
