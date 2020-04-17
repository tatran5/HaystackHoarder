using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableObject : MonoBehaviour
{
    public static KeyCode kbMoveForward = KeyCode.UpArrow;
    public static KeyCode kbMoveBackward = KeyCode.DownArrow;
    public static KeyCode kbMoveLeft = KeyCode.LeftArrow;
    public static KeyCode kbMoveRight = KeyCode.RightArrow;

    public static KeyCode kbInteract = KeyCode.Space;
    public static KeyCode kbPlaceTrap = KeyCode.C;
    public static KeyCode kbEnterExitTractor = KeyCode.LeftShift;

    public float speed = 0f;
    public static Vector3 epsilon = new Vector3(0.15f, 0.15f, 0.15f);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool HandleMovement()
    {
        bool moved = false;
        if (Input.GetKey(kbMoveLeft))
        {
            transform.position -= new Vector3(Time.deltaTime * speed, 0, 0);
            moved = true;
        } else if (Input.GetKey(kbMoveRight))
        {
            transform.position += new Vector3(Time.deltaTime * speed, 0, 0);
            moved = true;
        }
            
        if (Input.GetKey(kbMoveForward))
        {
            transform.position += new Vector3(0, 0, Time.deltaTime * speed);
            moved = true;
        } else if (Input.GetKey(kbMoveBackward))
        {
            transform.position -= new Vector3(0, 0, Time.deltaTime * speed);
            moved = true;
        }

        return moved;
    }
}
