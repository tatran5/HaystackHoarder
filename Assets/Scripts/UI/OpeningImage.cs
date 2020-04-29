using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpeningImage : MonoBehaviour
{
    
    public Image img;
    float fade;
    float difference;

    int waitTimer;
    int waitTimerLength;

    // Start is called before the first frame update
    void Start()
    {
        fade = 1;
        difference = 0.2f;

        waitTimer = 0;
        waitTimerLength = 100;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (waitTimer < waitTimerLength)
        {
            waitTimer += 1;
        }
        else {
            fade -= difference * Time.deltaTime;
            if (fade < 0)
            {
                fade = 0;
            }

            Color temp = img.color;
            temp.a = fade;
            img.color = temp;
        }
    }
}
