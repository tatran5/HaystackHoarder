﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tractor : MonoBehaviour
{
    private static float speed = 7f;
    private static float fuelMax = 100f;
    private static float fuelDepletePerSec = 5f;

    private float fuelLeft = fuelMax;
    private bool hasHay = false;
    private bool hasPlayer = true;

    private float timeHarvestHay = 0f;

    // Start is called before the first frame update
   void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hasPlayer && fuelLeft > 0 && 
            (Input.GetKey(Controller.kbMoveLeft) || Input.GetKey(Controller.kbMoveRight) ||
             Input.GetKey(Controller.kbMoveForward) || Input.GetKey(Controller.kbMoveBackward)))
        {
            if (Input.GetKey(Controller.kbMoveLeft))
                transform.position -= new Vector3(Time.deltaTime * speed, 0, 0);
            else if (Input.GetKey(Controller.kbMoveRight))
                transform.position += new Vector3(Time.deltaTime * speed, 0, 0);
            if (Input.GetKey(Controller.kbMoveForward))
                transform.position += new Vector3(0, 0, Time.deltaTime * speed);
            else if (Input.GetKey(Controller.kbMoveBackward))
                transform.position -= new Vector3(0, 0, Time.deltaTime * speed);

            fuelLeft -= (Time.deltaTime % 1) * fuelDepletePerSec;
            Debug.Log("fuelLeft: " + fuelLeft);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Haystack haystack = collision.gameObject.GetComponent<Haystack>();
        if (hasPlayer && haystack)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            if (!hasHay)
            {
                if (Input.GetKey(Controller.kbInteract))
                {
                    if (timeHarvestHay >= Haystack.timeHarvestRequired)
                    {
                        hasHay = true;
                        timeHarvestHay = 0f;
                        haystack.DecreaseHay();
                    }
                    else
                        timeHarvestHay += Time.fixedDeltaTime;
                }
                else
                    timeHarvestHay = 0f;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    { 
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        timeHarvestHay = 0;
    }
}
