using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static float speed = 3f; //dummy speed to test hay harvesting function
    public static Vector3 epsilon = new Vector3(0.2f, 0.2f, 0.2f);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(Controller.kbMoveLeft))
            transform.position -= new Vector3(Time.deltaTime * speed, 0, 0);
        else if (Input.GetKey(Controller.kbMoveRight))
            transform.position += new Vector3(Time.deltaTime * speed, 0, 0);
        if (Input.GetKey(Controller.kbMoveForward))
            transform.position += new Vector3(0, 0, Time.deltaTime * speed);
        else if (Input.GetKey(Controller.kbMoveBackward))
            transform.position -= new Vector3(0, 0, Time.deltaTime * speed);

        if (Input.GetKeyDown(Controller.kbEnterExitTractor))
            HandleEnterTractor
    }

    //
    private void HandleEnterTractor()
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

            HandleTractorDetected(tractor);
        }
    }

}
