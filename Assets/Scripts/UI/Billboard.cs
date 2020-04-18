using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class makes sure that a UI element always faces the camera
public class Billboard : MonoBehaviour
{
    public Transform cameraTransform;

    private void Start()
    {
        // There should be only one camera in the scene
        Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
        cameraTransform = cameras[0].transform;       
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + cameraTransform.forward);
    }
}
