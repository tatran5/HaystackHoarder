using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : ControllableObject
{
   
    // Start is called before the first frame update
    void Start()
    {
        speed = 3f;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement(); 

        if (Input.GetKeyDown(Controller.kbEnterExitTractor))
            HandleEnterTractor();
    }

    private void HandleEnterTractor()
    {
        if (Input.GetKeyDown(Controller.kbEnterExitTractor))
        {
            Collider[] colliders = Physics.OverlapBox(transform.position,
                   transform.localScale + epsilon, transform.rotation);
            for (int i = 0; i < colliders.Length; i++)
            {
                Tractor tractor = colliders[i].gameObject.GetComponent<Tractor>();
                if (tractor && !tractor.GetHasPlayer())
                {
                    GetComponent<Rigidbody>().velocity = Vector3.zero;
                    GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    tractor.SetHasPlayer(true);
                    Destroy(gameObject);
                }
            }
        }
    }

}
