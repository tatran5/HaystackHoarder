using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float timeHarvestHay = 0f;
    private bool hasHay = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
       
    }

    private void OnCollisionStay(Collision collision)
    {
        GameObject objectCollided = collision.gameObject;
        if (objectCollided.name.Equals("Haystack") && Input.GetKey(KeyCode.Space) && !hasHay)
        {
            if (timeHarvestHay >= Haystack.timeHarvestRequired)
            {
                hasHay = true;
                objectCollided.GetComponent<Haystack>().decreaseHay();
                timeHarvestHay = 0;
            } else
            {
                timeHarvestHay += Time.fixedDeltaTime;
            }
                 
         }
    }

    private void OnCollisionExit(Collision collision)
    {
        GameObject objectCollided = collision.gameObject;
        if (objectCollided.name.Equals("Haystack") && Input.GetKey(KeyCode.Space) && !hasHay)
        {
            timeHarvestHay = 0;
        } 
    }
}
