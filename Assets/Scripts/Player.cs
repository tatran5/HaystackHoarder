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
	public bool toDestroy = false;

    // Motion
    public float rotationSpeed = 450;
    private Quaternion targetRotation;
    private Animator animator;


    // TODO: Delete variables once finish testing
    private float testProgress = 0f;
    public Material testHasHayMaterial;
    public Material testPlayerMaterial;
	public Material tractorColor;
	public Material P1;
	public Material P2;
	public Material P3;
	public Material P1HasHay;
	public Material P2HasHay;
	public Material P1HasFuel;
	public Material P2HasFuel;

	// Start is called before the first frame update
	void Start()
    {
        progressBar.SetActive(false);
        speed = 5f;

    }

    // Update is called once per frame
    void Update()
    {
		if (team == 1)
		{
			if (state == PlayerState.HasHay || state == PlayerState.HasBale)
			{
				gameObject.GetComponent<MeshRenderer>().material = P1HasHay;
			}
			else if (state == PlayerState.HasFuel)
			{
				gameObject.GetComponent<MeshRenderer>().material = P1HasFuel;
			}
			else
			{
				gameObject.GetComponent<MeshRenderer>().material = P1;
			}
		}
		else if (team == 2)
		{
			if (state == PlayerState.HasHay || state == PlayerState.HasBale)
			{
				gameObject.GetComponent<MeshRenderer>().material = P2HasHay;
			}
			else if (state == PlayerState.HasFuel)
			{
				gameObject.GetComponent<MeshRenderer>().material = P2HasFuel;
			} else 
			{
				gameObject.GetComponent<MeshRenderer>().material = P2;
			}
		}
		else
		{
			gameObject.GetComponent<MeshRenderer>().material = P3;
		}

		HandlePlayerMovement();

        if (Input.GetKeyDown(kbEnterExitTractor))
                EnterTractor();
            else if (Input.GetKeyDown(kbInteract))
                InteractOnce();
            else if (Input.GetKey(kbInteract))
                InteractOverTime();
    }

	bool HandlePlayerMovement()
	{
		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		if (input != Vector3.zero)
		{
			//Debug.Log("Movement input");
			targetRotation = Quaternion.LookRotation(input);
			transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y,
				targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
			// animator.Play("Move_L");
			transform.position += input * Time.deltaTime;
			return true;
		}
		else
			// animator.Play("Idle");
			return false;
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
        if (state == PlayerState.HasHay && barn.state == BarnState.Empty && team == barn.team)
        {
            barn.StartProcessingHay();
            state = PlayerState.Empty;
            gameObject.GetComponent<MeshRenderer>().material = testPlayerMaterial; // TODO: delete after finishing debugging with hasHay
        }
        else if (state == PlayerState.Empty && barn.GetBale(team))
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
				//Destroy(gameObject);
				gameObject.GetComponent<PUN2_PlayerSync>().destroy = true;
				//Destroy(tractor);
				//gameObject.GetComponent<MeshRenderer>().material = tractorColor;
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
            tractor.RemoveHay();
            state = PlayerState.HasHay;
            gameObject.GetComponent<MeshRenderer>().material = testHasHayMaterial; // TODO: delete after finishing debugging with hasHay
        }
    }

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.tag == "Animal")
		{
			other.gameObject.GetComponent<Animal>().Highlighted();
			if (state == PlayerState.HasBale && Input.GetKeyDown(kbInteract))
			{
				state = PlayerState.Empty;
				other.gameObject.GetComponent<Animal>().FeedAnimal();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Animal")
		{
			other.gameObject.GetComponent<Animal>().Normal();
		}
	}
}
