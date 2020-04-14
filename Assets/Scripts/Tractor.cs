using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tractor : MonoBehaviour
{
    private static float fuelMax = 100f;
    private static float fuelDepletePerSec = 10f;
    private static float speed = 7f;

    private bool hasHay = false;
    private bool hasPlayer = false;

    // Start is called before the first frame update
   void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hasPlayer)
        {
           
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        
    }

}
