using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static float speed = 3f; //dummy speed to test hay harvesting function

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
    }

    private void OnCollisionStay(Collision collision)
    {
        GameObject collidedObject = collision.gameObject;

        Tractor tractor = collidedObject.GetComponent<Tractor>();
        if (tractor && Input.GetKey(Controller.kbEnterExitTractor))
        {
            tractor.SetTractorHasPlayer(true);
            Destroy(gameObject);
            Debug.Log(Random.Range(0f, 10f));
        } 
    }

    private void OnCollisionExit(Collision collision)
    {

    
    }
}
