using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : ControllableObject
{
    public ProgressBar progressBar;

    private State state = State.Empty;


    // TODO: Delete variables once finish testing
    private float testProgress = 0f;
    public Material testHasHayMaterial;
    public Material testPlayerMaterial;

    // Start is called before the first frame update
    void Start()
    {
        progressBar.SetActive(false);
        speed = 3f;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();

        if (Input.GetKeyDown(kbEnterExitTractor))
                EnterTractor();
            else if (Input.GetKeyDown(kbInteract))
                InteractOnce();
            else if (Input.GetKey(kbInteract))
                InteractOverTime();
    }

    public override void InteractOnce()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position,
        transform.localScale + epsilon, transform.rotation);
        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject collidedObject = colliders[i].gameObject;

            if (collidedObject.tag.Equals("Tractor"))
                GetHayFromTractor(collidedObject.GetComponent<Tractor>());
            else if (collidedObject.tag.Equals("Barn"))
            {
                Barn barn = collidedObject.GetComponent<Barn>();
                
                if (state == State.HasHay && barn.state == State.Empty)
                {
                    barn.StartProcessingHay();
                    state = State.Empty;
                    gameObject.GetComponent<MeshRenderer>().material = testPlayerMaterial; // TODO: delete after finishing debugging with hasHay
                } else if (state == State.Empty && barn.GetBale())
                {
                    state = State.HasBale;
                    gameObject.GetComponent<MeshRenderer>().material = testHasHayMaterial; // TODO: delete after finishing debugging with hasHay
                }
            }
        }
    }

    public override void InteractOverTime()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position,
                    transform.localScale + epsilon, transform.rotation);
        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject collidedObject = colliders[i].gameObject;

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

    private void GetHayFromTractor(Tractor tractor)
    {
        if (tractor.GetHay())
        {
            state = State.HasHay;
            gameObject.GetComponent<MeshRenderer>().material = testHasHayMaterial; // TODO: delete after finishing debugging with hasHay
        }
    }
    private void ProcessHayAtBarn()
    {

    }
}
