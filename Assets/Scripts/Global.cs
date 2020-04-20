using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Global : MonoBehaviour
{
    public static float timeLeft = 420f; //7 minute
    public Text textTimeLeft;

    // Update is called once per frame
    void Update()
    {
        UpdateTimer();
    }

    // Return false if out of time
    private void UpdateTimer()
    {
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0) textTimeLeft.text = "00:00";
        
        float min = Mathf.Floor(timeLeft / 60);
        float sec = Mathf.RoundToInt(timeLeft % 60);
        string minStr = (min < 10 ? "0" : "") + min.ToString();
        string secStr = (sec < 10 ? "0" : "") + Mathf.RoundToInt(sec).ToString();
        textTimeLeft.text = minStr + ":" + secStr;
    }
}
