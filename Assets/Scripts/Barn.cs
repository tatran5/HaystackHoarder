using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BarnState {HasHay, HasBale, Processing, Empty}
public class Barn : MonoBehaviour
{
    public static float timeProcessHayRequired = 5f; // in second

    public float timeProcessingHay = 0f;

    //TODO: delete these fields used for debugging for hashay
    public Material testBarnMaterial;
    public Material testHasBaleMaterial;
    public Material testProcessHayMaterial;

    /* If progress baar is not active, barn is empty. 
     * If progress bar is active, hay is being processed.*/
    public ProgressBar progressBar;
    public BarnState state = BarnState.Empty;


    private void Start()
    {
        progressBar.SetMaxValue(timeProcessHayRequired);
        progressBar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (state == BarnState.Processing)
        {
            if (timeProcessingHay < timeProcessHayRequired)
            {
                timeProcessingHay += Time.deltaTime;
                progressBar.SetValue(timeProcessingHay, timeProcessHayRequired);
            }
            else
            {
                timeProcessingHay = 0f;
                state = BarnState.HasBale;
                progressBar.SetActive(false);
                gameObject.GetComponent<MeshRenderer>().material = testHasBaleMaterial;
            }

        }
    }

    public void StartProcessingHay()
    {
        if (!progressBar.IsActive())
        {
            state = BarnState.Processing;
            progressBar.SetActive(true);
            gameObject.GetComponent<MeshRenderer>().material = testProcessHayMaterial; //TODO: delete this after finish testing
        }
    }

    // Deplete bale if there's any in the barn and return whether there's bale to take in the first place
    public bool GetBale()
    {
        if (state == BarnState.HasBale)
        {
            progressBar.SetValue(0, timeProcessHayRequired);
            progressBar.SetActive(false);
            gameObject.GetComponent<MeshRenderer>().material = testBarnMaterial;
            state = BarnState.Empty;
            return true;
        }
        return false; ;
    }
}
