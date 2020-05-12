using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haystack : MonoBehaviour
{
    /* This task involves defining the haystack class and keeping track 
     * of how much hay is left in the stack and how quickly it can be harvested. 
     * Must destroy itself upon running out of hay */

    public int hayAmountInitial = 3;
    public float timeHarvestRequired = 2f; // in second

    public int hayAmountLeft;

    // Start is called before the first frame update
    void Start()
    {
        hayAmountLeft = hayAmountInitial;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* Decrease hay. If there's no hay left, set haystack to be inactive, and make another
     * inactive haystack to be active */
    public void DecreaseHay()
    {
		gameObject.GetComponent<PUN2_HaystackController>().callDecrease();
        Debug.Log("decreaseHay called");
   //     if (hayAmountLeft == 0)
   //     {
			//gameObject.GetComponent<PUN2_HaystackController>().deactivated = true;
   //     }
    }
}
