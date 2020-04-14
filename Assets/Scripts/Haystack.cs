using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haystack : MonoBehaviour
{
    /* This task involves defining the haystack class and keeping track 
     * of how much hay is left in the stack and how quickly it can be harvested. 
     * Must destroy itself upon running out of hay */

    private static int hayAmountInitial = 2;
    public static float timeHarvestRequired = 3f; // in second

    private int hayAmountLeft = hayAmountInitial;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* Decrease hay. If there's no hay left, set haystack to be inactive, and make another
     * inactive haystack to be active */
    public void DecreaseHay()
    {
        hayAmountLeft--;
        Debug.Log("decreaseHay called");
        if (hayAmountLeft == 0)
        {
            Haystack[] haystacks = Resources.FindObjectsOfTypeAll<Haystack>();
            int loopCount = 0;

            while (true)
            {
                Haystack chosenHaystack = haystacks[Random.Range(0, haystacks.Length - 1)];

                if (!chosenHaystack.gameObject.activeSelf)
                {
                    chosenHaystack.gameObject.SetActive(true);
                    gameObject.SetActive(false);
                    hayAmountLeft = hayAmountInitial;
                    break;
                }
                if (loopCount >= haystacks.Length * haystacks.Length * 4)
                {
                    Debug.Log("Haystack::decreaseHay() might run into infinite loop");
                    break;
                }
                loopCount++;
            }
        }
    }
}
