using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : ControllableObject
{
    public ProgressBar progressBar;

    private float hayAmount = 0f;
    private bool isHayProcessed = false;
    private float timeProcessHay = 0f;

    // TODO: Delete variables once finish testing
    private float testProgress = 0f;

    // Start is called before the first frame update
    void Start()
    {
        progressBar.SetMaxValue(1);
        speed = 3f;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();

        //TODO: delete this if statement once finish testing
        if (Input.GetKeyDown(KeyCode.T))
        {
            testProgress += 0.1f;
            progressBar.SetProgress(testProgress);
        }

        if (Input.GetKeyDown(kbEnterExitTractor))
                EnterTractor();
            else if (Input.GetKeyDown(kbInteract))
                InteractOnce();
            else if (Input.GetKey(kbInteract))
                InteractOverTime();

            else if (Input.GetKeyUp(kbInteract))
            {
                timeProcessHay = 0f;
            }
    }

    private void InteractOnce()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position,
        transform.localScale + epsilon, transform.rotation);
        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject collidedObject = colliders[i].gameObject;

            if (collidedObject.tag.Equals("Tractor"))
            {
                // Get hay from tractor
                Tractor tractor = collidedObject.GetComponent<Tractor>();
                hayAmount = tractor.hayAmount;
                tractor.hayAmount = 0f;
            }
        }
    }

    private void InteractOverTime()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position,
                    transform.localScale + epsilon, transform.rotation);
        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject collidedObject = colliders[i].gameObject;

            if (collidedObject.tag.Equals("Barn"))
            {
                // Process hay
                if (timeProcessHay >= Barn.timeProcessHayRequired)
                {
                    isHayProcessed = true;
                    timeProcessHay = 0f;
                }
                else
                    timeProcessHay += Time.deltaTime;
            }
            else
            {
                timeProcessHay = 0f;
            }
        }

    }

    private void EnterTractor()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position,
                    transform.localScale + epsilon, transform.rotation);
        for (int i = 0; i < colliders.Length; i++)
        {
            Tractor tractor = colliders[i].gameObject.GetComponent<Tractor>();
            if (tractor && !tractor.hasPlayer)
            {
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                tractor.hasPlayer = true;
                Destroy(gameObject);
            }
        }
    }

    private void ProcessHayAtBarn()
    {

    }
}
