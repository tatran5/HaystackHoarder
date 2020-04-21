using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState {HasHay, HasBale, HasFuel, Empty}

// Player should only hold one thing at a time?
public class Player : ControllableObject
{

    public ProgressBar progressBar;
    public int team = 0;
    public PlayerState state = PlayerState.Empty;


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
                InteractOnceWithTractor(collidedObject.GetComponent<Tractor>());
            else if (collidedObject.tag.Equals("Barn"))
                InteractOnceWithBarn(collidedObject.GetComponent<Barn>());
            else if (collidedObject.tag.Equals("FuelStation"))
                GetFuelFromStation(collidedObject.GetComponent<FuelStation>());
        }
    }

    /* Player and fuel station must be on the same team for the player to get fuel from the station*/
    private void GetFuelFromStation(FuelStation fuelStation) { 
        if (state == PlayerState.Empty && team == fuelStation.team)
            state = PlayerState.HasFuel;
    } 

    // Handle give hay & take bale
    private void InteractOnceWithBarn(Barn barn)
    {
        if (state == PlayerState.HasHay && barn.state == BarnState.Empty)
        {
            barn.StartProcessingHay();
            state = PlayerState.Empty;
            gameObject.GetComponent<MeshRenderer>().material = testPlayerMaterial; // TODO: delete after finishing debugging with hasHay
        }
        else if (state == PlayerState.Empty && barn.GetBale())
        {
            state = PlayerState.HasBale;
            gameObject.GetComponent<MeshRenderer>().material = testHasHayMaterial; // TODO: delete after finishing debugging with hasHay
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
            if (tractor && !tractor.HasPlayer() && tractor.team == team)
            {
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                tractor.PlayerEnter();
                Destroy(gameObject);
            }
        }
    }

    public void InteractOnceWithTractor(Tractor tractor)
    {
        if (state == PlayerState.HasFuel)
        {
            state = PlayerState.Empty;
            tractor.RefillFuel();
            Debug.Log("Refilled");
        } else if (state == PlayerState.Empty)
        {
            GetHayFromTractor(tractor);
        }
    }
    private void GetHayFromTractor(Tractor tractor)
    {
        if (tractor.GetHay())
        {
            state = PlayerState.HasHay;
            gameObject.GetComponent<MeshRenderer>().material = testHasHayMaterial; // TODO: delete after finishing debugging with hasHay
        }
    }
}
