using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        Renderer thisRenderer = gameObject.GetComponent<Renderer>();
        Bounds bounds = thisRenderer.bounds;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer render = renderers[i];
            if (render != thisRenderer) bounds.Encapsulate(render.bounds);
        }
        Debug.Log("x: " + bounds.extents.x);
        Debug.Log("y: " + bounds.extents.y);
        Debug.Log("z: " + bounds.extents.z);
        Debug.Log("x*: " + bounds.extents.x * transform.localScale.x);
        Debug.Log("y*: " + bounds.extents.y * transform.localScale.y);
        Debug.Log("z*: " + bounds.extents.z * transform.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
    }

}
