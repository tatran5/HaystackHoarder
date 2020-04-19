using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum State { Empty, Processing, HasBale }

public class Barn : MonoBehaviour
{
    public static float timeProcessHayRequired = 3f; // in second

    public float timeProcessingHay = 0f;

    /* If progress baar is not active, barn is empty. 
     * If progress bar is active but not full, hay is being processed. 
     * If progress bar is active and full, hay has turned into bale*/
    public ProgressBar progressBar; 
    State state = State.Empty;


    private void Start()
    {
        progressBar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Processing)
        {
            if (timeProcessingHay < timeProcessHayRequired)
            {
                timeProcessingHay += Time.deltaTime;
                progressBar.SetValue(timeProcessingHay, timeProcessHayRequired);
            } else
            {
                timeProcessingHay = 0f;
                progressBar.SetValue(timeProcessHayRequired, timeProcessHayRequired);
                state = State.HasBale;
            }              

        }
    }

    public void StartProcessingHay()
    {
        if (!progressBar.IsActive())
            progressBar.SetActive(true);
    }

    // Deplete bale if there's any in the barn and return whether there's bale to take in the first place
    public bool GetBale()
    {
        bool hasBale = progressBar.IsFull();
        if (hasBale) {
            progressBar.SetValue(0, timeProcessHayRequired);
            progressBar.SetActive(false);
        }
        return hasBale;
    }
}
